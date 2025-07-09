namespace Rihla.WebAPI.Options
{
    public class ActiveDirectoryOptions
    {
        public const string SectionName = "ActiveDirectory";

        public bool Enabled { get; set; } = false;
        public string Domain { get; set; } = string.Empty;
        public string LdapServer { get; set; } = string.Empty;
        public int LdapPort { get; set; } = 389;
        public bool UseSsl { get; set; } = true;
        public string ServiceAccountUsername { get; set; } = string.Empty;
        public string ServiceAccountPassword { get; set; } = string.Empty;
        public string UserSearchBase { get; set; } = string.Empty;
        public string GroupSearchBase { get; set; } = string.Empty;
        public string UserFilter { get; set; } = "(&(objectClass=user)(sAMAccountName={0}))";
        public string GroupFilter { get; set; } = "(&(objectClass=group)(member={0}))";
        
        public AzureAdOptions AzureAd { get; set; } = new AzureAdOptions();
        
        public Dictionary<string, string> GroupRoleMapping { get; set; } = new Dictionary<string, string>();
        
        public bool EnableFallbackToInternal { get; set; } = true;
        public int ConnectionTimeoutSeconds { get; set; } = 30;
        public int CacheExpirationMinutes { get; set; } = 60;
    }

    public class AzureAdOptions
    {
        public bool Enabled { get; set; } = false;
        public string TenantId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string Authority { get; set; } = string.Empty;
        public string CallbackPath { get; set; } = "/signin-oidc";
        public string SignedOutCallbackPath { get; set; } = "/signout-callback-oidc";
        public List<string> Scopes { get; set; } = new List<string> { "openid", "profile", "email" };
    }
}
