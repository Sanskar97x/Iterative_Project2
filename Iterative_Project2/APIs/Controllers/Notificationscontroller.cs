using Iterative_Project2.Application.DTOs.NotificationDto;
using Iterative_Project2.Core.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Iterative_Project2.Controllers
{
    // Every logged-in user (candidate, employer, admin) only ever sees their own notifications
    [ApiController]
    [Authorize]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationsController(INotificationService service) => _service = service;

        // GET /api/notifications?unreadOnly=true&type=ApplicationStatusChanged&page=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> GetMine([FromQuery] NotificationFilterRequestDto filter) =>
            Ok(await _service.GetMyNotifications(filter));

        // GET /api/notifications/unread-count  -> { "unreadCount": 3 }   (poll this for the bell icon)
        [HttpGet("unread-count")]
        public async Task<IActionResult> UnreadCount() =>
            Ok(new { unreadCount = await _service.GetUnreadCount() });

        [HttpPost("{id:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id) =>
            await _service.MarkAsRead(id) ? NoContent() : NotFound();

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllAsRead() =>
            Ok(new { updated = await _service.MarkAllAsRead() });

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) =>
            await _service.DeleteNotification(id) ? NoContent() : NotFound();

        // DELETE /api/notifications/read  -> removes all notifications already read
        [HttpDelete("read")]
        public async Task<IActionResult> DeleteRead() =>
            Ok(new { removed = await _service.DeleteAllRead() });
    }
}