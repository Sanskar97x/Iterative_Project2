using Iterative_Project2.Core.Enums;
using System;
using System.Collections.Generic;

namespace Iterative_Project2.Application.DTOs.NotificationDto
{
    public class NotificationFilterRequestDto
    {
        public bool UnreadOnly { get; set; }
        public Notificationtype Type { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Guid? RelatedEntityId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    public class NotificationListResponseDto
    {
        public List<NotificationDto> Items { get; set; } = new();
        public int UnreadCount { get; set; }          // all unread, regardless of the filter
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}