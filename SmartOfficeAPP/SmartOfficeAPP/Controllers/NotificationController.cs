using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Services.IServices;
using System.Security.Claims;

namespace SmartOfficeAPP.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Retrieves all notifications for the authenticated user.
    /// </summary>
    /// <returns>A list of the user's notifications.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetUserNotifications()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var notifications = await _notificationService.GetNotificationsForUserAsync(userId);
        return Ok(notifications);
    }

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    /// <param name="id">The ID of the notification to mark as read.</param>
    /// <returns>An HTTP 204 No Content response on success.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var notification = await _notificationService.GetNotificationByIdAsync(id); // assuming there's a method to get a single notification

        if (notification == null || notification.UserId != userId)
        {
            return NotFound();
        }

        var result = await _notificationService.MarkNotificationAsReadAsync(id);
        if (!result)
        {
            return BadRequest("Could not update the notification.");
        }

        return NoContent();
    }
}
