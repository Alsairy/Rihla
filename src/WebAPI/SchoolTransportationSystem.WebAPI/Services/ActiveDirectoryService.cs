using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using Rihla.WebAPI.Options;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Text;
using Rihla.Core.Entities;
using Rihla.Application.Interfaces;
using Rihla.Core.Common;

namespace Rihla.WebAPI.Services
{
    public interface IActiveDirectoryService
    {
        Task<Result<User>> AuthenticateAsync(string username, string password);
        Task<Result<List<string>>> GetUserGroupsAsync(string username);
        Task<Result<User>> SyncUserAsync(string username);
        Task<bool> IsAvailableAsync();
        string MapGroupToRole(string groupName);
    }

    public class ActiveDirectoryService : IActiveDirectoryService
    {
        private readonly ActiveDirectoryOptions _options;
        private readonly IUserService _userService;
        private readonly ILogger<ActiveDirectoryService> _logger;
        private readonly IMemoryCache _cache;

        public ActiveDirectoryService(
            IOptions<ActiveDirectoryOptions> options,
            IUserService userService,
            ILogger<ActiveDirectoryService> logger,
            IMemoryCache cache)
        {
            _options = options.Value;
            _userService = userService;
            _logger = logger;
            _cache = cache;
        }

        public async Task<Result<User>> AuthenticateAsync(string username, string password)
        {
            try
            {
                if (!_options.Enabled)
                {
                    return Result<User>.Failure("Active Directory authentication is disabled");
                }

                if (!string.IsNullOrEmpty(_options.LdapServer))
                {
                    var ldapResult = await AuthenticateWithLdapAsync(username, password);
                    if (ldapResult.IsSuccess)
                    {
                        return ldapResult;
                    }
                }

                if (_options.EnableFallbackToInternal)
                {
                    _logger.LogInformation("Falling back to internal authentication for user: {Username}", username);
                    var internalResult = await _userService.AuthenticateAsync(username, password, "1");
                    return internalResult;
                }

                return Result<User>.Failure("Authentication failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication for user: {Username}", username);
                
                if (_options.EnableFallbackToInternal)
                {
                    try
                    {
                        var fallbackResult = await _userService.AuthenticateAsync(username, password, "1");
                        return fallbackResult;
                    }
                    catch (Exception fallbackEx)
                    {
                        _logger.LogError(fallbackEx, "Fallback authentication also failed for user: {Username}", username);
                    }
                }

                return Result<User>.Failure($"Authentication error: {ex.Message}");
            }
        }

        private async Task<Result<User>> AuthenticateWithLdapAsync(string username, string password)
        {
            try
            {
                using var connection = new LdapConnection(new LdapDirectoryIdentifier(_options.LdapServer, _options.LdapPort));
                
                connection.SessionOptions.SecureSocketLayer = _options.UseSsl;
                connection.SessionOptions.ProtocolVersion = 3;
                connection.Timeout = TimeSpan.FromSeconds(_options.ConnectionTimeoutSeconds);

                if (!string.IsNullOrEmpty(_options.ServiceAccountUsername))
                {
                    var serviceCredential = new NetworkCredential(_options.ServiceAccountUsername, _options.ServiceAccountPassword, _options.Domain);
                    connection.Bind(serviceCredential);
                }

                var searchFilter = string.Format(_options.UserFilter, username);
                var searchRequest = new SearchRequest(_options.UserSearchBase, searchFilter, SearchScope.Subtree, "distinguishedName", "mail", "displayName", "memberOf");
                var searchResponse = (SearchResponse)connection.SendRequest(searchRequest);

                if (searchResponse.Entries.Count == 0)
                {
                    return Result<User>.Failure("User not found in Active Directory");
                }

                var userEntry = searchResponse.Entries[0];
                var userDn = userEntry.DistinguishedName;

                try
                {
                    var userCredential = new NetworkCredential(username, password, _options.Domain);
                    using var userConnection = new LdapConnection(new LdapDirectoryIdentifier(_options.LdapServer, _options.LdapPort));
                    userConnection.SessionOptions.SecureSocketLayer = _options.UseSsl;
                    userConnection.SessionOptions.ProtocolVersion = 3;
                    userConnection.Bind(userCredential);
                }
                catch (LdapException)
                {
                    return Result<User>.Failure("Invalid credentials");
                }

                var email = userEntry.Attributes["mail"]?[0]?.ToString() ?? $"{username}@{_options.Domain}";
                var displayName = userEntry.Attributes["displayName"]?[0]?.ToString() ?? username;

                var groups = await GetUserGroupsFromEntryAsync(userEntry);
                var roles = groups.Select(MapGroupToRole).Where(role => !string.IsNullOrEmpty(role)).ToList();

                if (!roles.Any())
                {
                    roles.Add("User"); // Default role
                }

                var nameParts = displayName.Split(' ', 2);
                var firstName = nameParts.Length > 0 ? nameParts[0] : username;
                var lastName = nameParts.Length > 1 ? nameParts[1] : "";
                
                var user = new User
                {
                    Username = username,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Role = roles.First(), // Primary role
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "ActiveDirectory",
                    TenantId = "1" // Default tenant
                };

                var existingUserResult = await _userService.GetByEmailAsync(email, "1");
                if (existingUserResult.IsSuccess)
                {
                    var existingUser = existingUserResult.Value;
                    existingUser.Email = email;
                    existingUser.FirstName = firstName;
                    existingUser.LastName = lastName;
                    existingUser.Role = roles.First();
                    existingUser.IsActive = true;
                    existingUser.UpdatedAt = DateTime.UtcNow;
                    existingUser.UpdatedBy = "ActiveDirectory";

                    var updateResult = await _userService.UpdateAsync(existingUser.Id, new Rihla.Application.DTOs.UpdateUserDto
                    {
                        Email = email,
                        FirstName = firstName,
                        LastName = lastName,
                        Role = roles.First(),
                        IsActive = true
                    }, "1");

                    if (updateResult.IsSuccess)
                    {
                        return Result<User>.Success(existingUser);
                    }
                }
                else
                {
                    var createResult = await _userService.CreateAsync(new Rihla.Application.DTOs.CreateUserDto
                    {
                        Username = username,
                        Email = email,
                        FirstName = firstName,
                        LastName = lastName,
                        Role = roles.First(),
                        IsActive = true,
                        Password = Guid.NewGuid().ToString(), // Random password since AD handles auth
                        TenantId = "1"
                    }, "1");

                    if (createResult.IsSuccess)
                    {
                        return Result<User>.Success(createResult.Value);
                    }
                }

                return Result<User>.Success(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LDAP authentication failed for user: {Username}", username);
                return Result<User>.Failure($"LDAP authentication error: {ex.Message}");
            }
        }

        public async Task<Result<List<string>>> GetUserGroupsAsync(string username)
        {
            try
            {
                var cacheKey = $"user_groups_{username}";
                if (_cache.TryGetValue(cacheKey, out List<string>? cachedGroups) && cachedGroups != null)
                {
                    return Result<List<string>>.Success(cachedGroups);
                }

                using var connection = new LdapConnection(new LdapDirectoryIdentifier(_options.LdapServer, _options.LdapPort));
                connection.SessionOptions.SecureSocketLayer = _options.UseSsl;
                connection.SessionOptions.ProtocolVersion = 3;

                if (!string.IsNullOrEmpty(_options.ServiceAccountUsername))
                {
                    var serviceCredential = new NetworkCredential(_options.ServiceAccountUsername, _options.ServiceAccountPassword, _options.Domain);
                    connection.Bind(serviceCredential);
                }

                var searchFilter = string.Format(_options.UserFilter, username);
                var searchRequest = new SearchRequest(_options.UserSearchBase, searchFilter, SearchScope.Subtree, "memberOf");
                var searchResponse = (SearchResponse)connection.SendRequest(searchRequest);

                if (searchResponse.Entries.Count == 0)
                {
                    return Result<List<string>>.Failure("User not found");
                }

                var groups = await GetUserGroupsFromEntryAsync(searchResponse.Entries[0]);
                
                _cache.Set(cacheKey, groups, TimeSpan.FromMinutes(_options.CacheExpirationMinutes));

                return Result<List<string>>.Success(groups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user groups for: {Username}", username);
                return Result<List<string>>.Failure($"Error getting user groups: {ex.Message}");
            }
        }

        private async Task<List<string>> GetUserGroupsFromEntryAsync(SearchResultEntry userEntry)
        {
            var groups = new List<string>();

            if (userEntry.Attributes["memberOf"] != null)
            {
                foreach (var memberOf in userEntry.Attributes["memberOf"])
                {
                    var groupDn = memberOf.ToString();
                    if (!string.IsNullOrEmpty(groupDn))
                    {
                        var cnStart = groupDn.IndexOf("CN=", StringComparison.OrdinalIgnoreCase);
                        if (cnStart >= 0)
                        {
                            cnStart += 3; // Skip "CN="
                            var cnEnd = groupDn.IndexOf(',', cnStart);
                            if (cnEnd > cnStart)
                            {
                                var groupName = groupDn.Substring(cnStart, cnEnd - cnStart);
                                groups.Add(groupName);
                            }
                        }
                    }
                }
            }

            return await Task.FromResult(groups);
        }

        public async Task<Result<User>> SyncUserAsync(string username)
        {
            try
            {
                if (!_options.Enabled)
                {
                    return Result<User>.Failure("Active Directory is disabled");
                }

                var groupsResult = await GetUserGroupsAsync(username);
                if (!groupsResult.IsSuccess)
                {
                    return Result<User>.Failure(groupsResult.Error);
                }

                var existingUserResult = await _userService.GetByEmailAsync($"{username}@{_options.Domain}", "1");
                if (!existingUserResult.IsSuccess)
                {
                    return Result<User>.Failure("User not found in local database");
                }

                var user = existingUserResult.Value;
                var roles = groupsResult.Value?.Select(MapGroupToRole).Where(role => !string.IsNullOrEmpty(role)).ToList() ?? new List<string>();
                
                if (roles.Any())
                {
                    user.Role = roles.First();
                    user.UpdatedAt = DateTime.UtcNow;
                    user.UpdatedBy = "ActiveDirectorySync";

                    var updateResult = await _userService.UpdateAsync(user.Id, new Rihla.Application.DTOs.UpdateUserDto
                    {
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Role = roles.First(),
                        IsActive = user.IsActive
                    }, "1");

                    if (updateResult.IsSuccess)
                    {
                        return Result<User>.Success(user);
                    }
                }

                return Result<User>.Success(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing user: {Username}", username);
                return Result<User>.Failure($"Sync error: {ex.Message}");
            }
        }

        public async Task<bool> IsAvailableAsync()
        {
            try
            {
                if (!_options.Enabled || string.IsNullOrEmpty(_options.LdapServer))
                {
                    return false;
                }

                using var connection = new LdapConnection(new LdapDirectoryIdentifier(_options.LdapServer, _options.LdapPort));
                connection.SessionOptions.SecureSocketLayer = _options.UseSsl;
                connection.SessionOptions.ProtocolVersion = 3;
                connection.Timeout = TimeSpan.FromSeconds(5); // Short timeout for availability check

                if (!string.IsNullOrEmpty(_options.ServiceAccountUsername))
                {
                    var serviceCredential = new NetworkCredential(_options.ServiceAccountUsername, _options.ServiceAccountPassword, _options.Domain);
                    connection.Bind(serviceCredential);
                }

                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Active Directory is not available");
                return false;
            }
        }

        public string MapGroupToRole(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
                return string.Empty;

            if (_options.GroupRoleMapping.TryGetValue(groupName, out var mappedRole))
            {
                return mappedRole;
            }

            var lowerGroupName = groupName.ToLowerInvariant();
            
            if (lowerGroupName.Contains("admin") || lowerGroupName.Contains("administrator"))
                return "Admin";
            
            if (lowerGroupName.Contains("manager") || lowerGroupName.Contains("supervisor"))
                return "Manager";
            
            if (lowerGroupName.Contains("dispatcher") || lowerGroupName.Contains("operator"))
                return "Dispatcher";
            
            if (lowerGroupName.Contains("driver"))
                return "Driver";
            
            if (lowerGroupName.Contains("parent") || lowerGroupName.Contains("guardian"))
                return "Parent";

            return string.Empty; // No mapping found
        }
    }
}
