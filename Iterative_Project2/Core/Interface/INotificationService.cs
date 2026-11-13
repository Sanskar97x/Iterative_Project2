using Iterative_Project2.Application.DTOs.NotificationDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Core.Interface
{
    // What the logged-in user does with their own notifications (reading side)
    public interface INotificationService
    {
        Task<NotificationListResponseDto> GetMyNotifications(NotificationFilterRequestDto filter);
        Task<int> GetUnreadCount();
        Task<bool> MarkAsRead(Guid notificationId);
        Task<int> MarkAllAsRead();                  // returns how many were updated
        Task<bool> DeleteNotification(Guid notificationId);
        Task<int> DeleteAllRead();                  // returns how many were removed
    }
}
