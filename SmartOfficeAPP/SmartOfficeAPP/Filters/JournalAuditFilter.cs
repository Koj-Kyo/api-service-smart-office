using Microsoft.AspNetCore.Mvc.Filters;
using SmartOfficeAPP.Services.IServices;
using System.Security.Claims;

namespace SmartOfficeAPP.Filters;

public class JournalAuditFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Execute the action first
        var resultContext = await next();

        // Check if the action was successful
        if (resultContext.Exception == null)
        {
            // Retrieve the service from the request scope
            var auditService = context.HttpContext.RequestServices.GetService<IJournalAuditService>();

            if (auditService != null)
            {
                // Get the user ID from the JWT token
                var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var actionName = context.ActionDescriptor.DisplayName;
                var contextName = "API Request";

                if (!string.IsNullOrEmpty(userId))
                {
                    // Log the action asynchronously
                    await auditService.LogActionAsync(userId, actionName, contextName);
                }
            }
        }
    }
}
