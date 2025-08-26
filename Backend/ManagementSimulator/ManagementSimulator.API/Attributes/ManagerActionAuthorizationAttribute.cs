using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace ManagementSimulator.API.Attributes
{
    public class ManagerActionAuthorizationAttribute : Attribute, IAsyncActionFilter
    {
        public bool AllowViewOnly { get; set; } = false;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;
            var user = httpContext.User;

            Console.WriteLine($"[ManagerActionAuthorization] Starting authorization check. AllowViewOnly: {AllowViewOnly}");

            if (!user.Identity?.IsAuthenticated == true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var isManager = user.IsInRole("Manager");
            var isAdmin = user.IsInRole("Admin");

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var authorizationService = httpContext.RequestServices.GetRequiredService<IResourceAuthorizationService>();

            var isActingAsSecondManager = await authorizationService.IsUserActingAsSecondManagerAsync(userId);
            Console.WriteLine($"[ManagerActionAuthorization] User {userId} isActingAsSecondManager: {isActingAsSecondManager}");

            if (!isManager && !isAdmin && !isActingAsSecondManager)
            {
                Console.WriteLine($"[ManagerActionAuthorization] User {userId} denied access - not manager, admin, or second manager");
                context.Result = new ForbidResult();
                return;
            }

            if (isAdmin)
            {
                Console.WriteLine($"[ManagerActionAuthorization] User {userId} is admin - allowing access");
                await next();
                return;
            }

            if (isActingAsSecondManager)
            {
                Console.WriteLine($"[ManagerActionAuthorization] User {userId} is acting as second manager - allowing access");
                await next();
                return;
            }



            if (!AllowViewOnly)
            {
                var canModify = await authorizationService.CanManagerModifyDataAsync(userId);
                Console.WriteLine($"[ManagerActionAuthorization] User {userId} canModify: {canModify}");

                if (!canModify)
                {
                    var activeSecondManager = await authorizationService.GetActiveSecondManagerForManagerAsync(userId);
                    Console.WriteLine($"[ManagerActionAuthorization] User {userId} blocked - active second manager: {activeSecondManager}");
                    throw new ManagerViewOnlyException(userId, activeSecondManager,
                        "Nu puteți efectua această acțiune deoarece sunteți temporar în modul view-only. Un manager secundar a preluat responsabilitățile dvs.");
                }
            }

            Console.WriteLine($"[ManagerActionAuthorization] User {userId} authorization passed - allowing access");
            await next();
        }
    }
}