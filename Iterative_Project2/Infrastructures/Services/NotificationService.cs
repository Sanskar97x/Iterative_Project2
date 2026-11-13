using Iterative_Project2.Application.DTOs.NotificationDto;
using Iterative_Project2.Core.Interface;
using Iterative_Project2.Infrastructures.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Iterative_Project2.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _http;

        public NotificationService(AppDbContext db, IHttpContextAccessor http)
        {
            _db = db;
            _http = http;
        }

        // Assumes the JWT stores the user id in the NameIdentifier claim.
        private Guid CurrentUserId =>
            Guid.TryParse(_http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
                ? id : throw new UnauthorizedAccessException();

        public async Task<NotificationListResponseDto> GetMyNotifications(NotificationFilterRequestDto filter)
        {
            var me = CurrentUserId;
            var page = Math.Max(1, filter.Page);
            var size = Math.Clamp(filter.PageSize, 1, 50);

            var mine = _db.Notifications.AsNoTracking().Where(n => n.UserId == me);
            var unreadCount = await mine.CountAsync(n => !n.IsRead);

            var query = mine;
            if (filter.UnreadOnly) query = query.Where(n => !n.IsRead);
            if (filter.Type is { } type) query = query.Where(n => n.Type == type);

            var total = await query.CountAsync();

            var rows = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return new NotificationListResponseDto
            {
                Items = rows.Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Type = n.Type.ToString(),
                    Title = n.Title,
                    Message = n.Message,
                    RelatedEntityId = n.RelatedEntityId,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    ReadAt = n.ReadAt
                }).ToList(),
                UnreadCount = unreadCount,
                Page = page,
                PageSize = size,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)size)
            };
        }

        public async Task<int> GetUnreadCount()
        {
            var me = CurrentUserId;
            return await _db.Notifications.AsNoTracking().CountAsync(n => n.UserId == me && !n.IsRead);
        }

        // ExecuteUpdate / ExecuteDelete need EF Core 7 or newer
       
        public async Task<bool> MarkAsRead(Guid notificationId)
        {
            var me = CurrentUserId;
            var now = DateTime.UtcNow;

            var notification = await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == me);

            if (notification == null)
                return false;

            notification.IsRead = true;
            notification.ReadAt = notification.ReadAt ?? now;

            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<int> MarkAllAsRead()
        {
            var me = CurrentUserId;
            var now = DateTime.UtcNow;

            var notifications = await _db.Notifications
                .Where(n => n.UserId == me && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = now;
            }

            await _db.SaveChangesAsync();

            return notifications.Count;
        }

        public async Task<bool> DeleteNotification(Guid notificationId)
        {
            var me = CurrentUserId;

            var notification = await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == me);

            if (notification == null)
                return false;

            _db.Notifications.Remove(notification);

            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<int> DeleteAllRead()
        {
            var me = CurrentUserId;

            var notifications = await _db.Notifications
                .Where(n => n.UserId == me && n.IsRead)
                .ToListAsync();

            _db.Notifications.RemoveRange(notifications);

            await _db.SaveChangesAsync();

            return notifications.Count;
        }
    }
}