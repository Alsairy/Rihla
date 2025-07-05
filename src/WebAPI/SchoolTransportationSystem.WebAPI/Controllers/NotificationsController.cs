using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Rihla.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetNotifications()
        {
            try
            {
                var notifications = new[]
                {
                    new {
                        Id = 1,
                        Title = "Vehicle Maintenance Due",
                        Message = "Vehicle SAB-1234 is due for maintenance",
                        Type = "maintenance",
                        Priority = "high",
                        IsRead = false,
                        CreatedAt = DateTime.Now.AddHours(-2),
                        UserId = 1
                    },
                    new {
                        Id = 2,
                        Title = "Late Pickup Alert",
                        Message = "Student Ahmed Ali was not picked up at scheduled time",
                        Type = "attendance",
                        Priority = "urgent",
                        IsRead = false,
                        CreatedAt = DateTime.Now.AddHours(-1),
                        UserId = 1
                    },
                    new {
                        Id = 3,
                        Title = "Payment Received",
                        Message = "Payment of 500 SAR received from parent",
                        Type = "payment",
                        Priority = "normal",
                        IsRead = true,
                        CreatedAt = DateTime.Now.AddHours(-3),
                        UserId = 1
                    }
                };

                return Ok(notifications);
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
                // Simulate marking notification as read
                await Task.Delay(100);
                return Ok(new { message = "Notification marked as read" });
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
                // Simulate marking all notifications as read
                await Task.Delay(200);
                return Ok(new { message = "All notifications marked as read" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error marking all notifications as read", error = ex.Message });
            }
        }

        [HttpPost("send")]
        public async Task<ActionResult> SendNotification([FromBody] object notificationData)
        {
            try
            {
                // Simulate sending notification
                await Task.Delay(500);
                return Ok(new { message = "Notification sent successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error sending notification", error = ex.Message });
            }
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetNotificationStatistics()
        {
            try
            {
                var stats = new
                {
                    TotalNotifications = 15,
                    UnreadNotifications = 5,
                    UrgentNotifications = 2,
                    TodayNotifications = 8
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving notification statistics", error = ex.Message });
            }
        }
    }
}

