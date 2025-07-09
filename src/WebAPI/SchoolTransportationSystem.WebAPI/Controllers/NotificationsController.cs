using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Rihla.Application.DTOs;
using Rihla.Application.Interfaces;
using Rihla.WebAPI.Services;

namespace Rihla.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ManagerOrAbove")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IUserContext _userContext;

        public NotificationsController(INotificationService notificationService, IUserContext userContext)
        {
            _notificationService = notificationService;
            _userContext = userContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotifications([FromQuery] bool unreadOnly = false)
        {
            try
            {
                var tenantId = _userContext.GetTenantId().ToString();
                var userId = _userContext.GetUserId();

                if (string.IsNullOrEmpty(tenantId) || userId <= 0)
                {
                    return BadRequest(new { message = "Invalid user context" });
                }

                var result = await _notificationService.GetNotificationsAsync(tenantId, userId, unreadOnly);

                if (result.IsSuccess)
                {
                    return Ok(result.Value);
                }

                return BadRequest(new { message = result.Error });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving notifications", error = ex.Message });
            }
        }

        [HttpPost("{id}/mark-read")]
        public async Task<ActionResult> MarkAsRead(int id)
        {
            try
            {
                var tenantId = _userContext.GetTenantId().ToString();
                var username = _userContext.GetUsername();

                if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(username))
                {
                    return BadRequest(new { message = "Invalid user context" });
                }

                var result = await _notificationService.MarkNotificationAsReadAsync(id, username, tenantId);

                if (result.IsSuccess)
                {
                    return Ok(new { message = "Notification marked as read" });
                }

                return BadRequest(new { message = result.Error });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error marking notification as read", error = ex.Message });
            }
        }

        [HttpPost("mark-all-read")]
        public async Task<ActionResult> MarkAllAsRead()
        {
            try
            {
                var tenantId = _userContext.GetTenantId().ToString();
                var userId = _userContext.GetUserId();
                var username = _userContext.GetUsername();

                if (string.IsNullOrEmpty(tenantId) || userId <= 0 || string.IsNullOrEmpty(username))
                {
                    return BadRequest(new { message = "Invalid user context" });
                }

                var notificationsResult = await _notificationService.GetNotificationsAsync(tenantId, userId, true);

                if (!notificationsResult.IsSuccess)
                {
                    return BadRequest(new { message = notificationsResult.Error });
                }

                var markReadTasks = notificationsResult.Value.Select(n => 
                    _notificationService.MarkNotificationAsReadAsync(n.Id, username, tenantId));

                await Task.WhenAll(markReadTasks);

                return Ok(new { message = "All notifications marked as read" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error marking all notifications as read", error = ex.Message });
            }
        }

        [HttpPost("send")]
        public async Task<ActionResult> SendNotification([FromBody] CreateNotificationDto notificationData)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var tenantId = _userContext.GetTenantId().ToString();
                var username = _userContext.GetUsername();

                if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(username))
                {
                    return BadRequest(new { message = "Invalid user context" });
                }

                notificationData.TenantId = tenantId;
                notificationData.CreatedBy = username;

                return Ok(new { message = "Notification creation endpoint - implement based on specific notification type" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error sending notification", error = ex.Message });
            }
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<NotificationStatisticsDto>> GetNotificationStatistics()
        {
            try
            {
                var tenantId = _userContext.GetTenantId().ToString();
                var userId = _userContext.GetUserId();

                if (string.IsNullOrEmpty(tenantId) || userId <= 0)
                {
                    return BadRequest(new { message = "Invalid user context" });
                }

                var allNotificationsResult = await _notificationService.GetNotificationsAsync(tenantId, userId, false);
                var unreadNotificationsResult = await _notificationService.GetNotificationsAsync(tenantId, userId, true);

                if (!allNotificationsResult.IsSuccess || !unreadNotificationsResult.IsSuccess)
                {
                    return BadRequest(new { message = "Error retrieving notification data" });
                }

                var allNotifications = allNotificationsResult.Value;
                var unreadNotifications = unreadNotificationsResult.Value;

                var today = DateTime.UtcNow.Date;
                var todayNotifications = allNotifications.Where(n => n.CreatedAt.Date == today).Count();
                var urgentNotifications = unreadNotifications.Where(n => n.Priority == Rihla.Core.Enums.NotificationPriority.Urgent).Count();
                var criticalNotifications = unreadNotifications.Where(n => n.Priority == Rihla.Core.Enums.NotificationPriority.Critical).Count();

                var stats = new NotificationStatisticsDto
                {
                    TotalNotifications = allNotifications.Count,
                    UnreadNotifications = unreadNotifications.Count,
                    UrgentNotifications = urgentNotifications,
                    CriticalNotifications = criticalNotifications,
                    TodayNotifications = todayNotifications,
                    EmailsSent = allNotifications.Count(n => n.EmailSent),
                    SmssSent = allNotifications.Count(n => n.SmsSent),
                    FailedEmails = allNotifications.Count(n => !string.IsNullOrEmpty(n.EmailError)),
                    FailedSms = allNotifications.Count(n => !string.IsNullOrEmpty(n.SmsError)),
                    LastUpdated = DateTime.UtcNow
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving notification statistics", error = ex.Message });
            }
        }

        [HttpPost("emergency")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> SendEmergencyAlert([FromBody] EmergencyAlertDto alertData)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var tenantId = _userContext.GetTenantId().ToString();

                if (string.IsNullOrEmpty(tenantId))
                {
                    return BadRequest(new { message = "Invalid user context" });
                }

                var result = await _notificationService.SendEmergencyAlertAsync(tenantId, alertData.Message);

                if (result.IsSuccess)
                {
                    return Ok(new { message = "Emergency alert sent successfully" });
                }

                return BadRequest(new { message = result.Error });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error sending emergency alert", error = ex.Message });
            }
        }

        [HttpPost("pickup/{studentId}")]
        public async Task<ActionResult> SendPickupNotification(int studentId, [FromBody] PickupNotificationDto pickupData)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var tenantId = _userContext.GetTenantId().ToString();

                if (string.IsNullOrEmpty(tenantId))
                {
                    return BadRequest(new { message = "Invalid user context" });
                }

                var result = await _notificationService.SendStudentPickupNotificationAsync(tenantId, studentId, pickupData.Status);

                if (result.IsSuccess)
                {
                    return Ok(new { message = "Pickup notification sent successfully" });
                }

                return BadRequest(new { message = result.Error });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error sending pickup notification", error = ex.Message });
            }
        }
    }

    public class EmergencyAlertDto
    {
        public string Message { get; set; } = string.Empty;
    }

    public class PickupNotificationDto
    {
        public string Status { get; set; } = string.Empty;
    }
}

