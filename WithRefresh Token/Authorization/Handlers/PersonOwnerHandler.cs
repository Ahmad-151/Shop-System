using Microsoft.AspNetCore.Authorization;
using ShopAPI.Authorization.Requirements;
using ShopAPI.Extensions;

namespace ShopAPI.Authorization.Handlers
{
    public class PersonOwnerHandler : AuthorizationHandler<PersonOwnerOrAdminRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PersonOwnerOrAdminRequirement requirement)
        {
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var currentPersonId = context.User.GetPersonId();
            if (currentPersonId == null)
                return Task.CompletedTask;

            var httpContext = context.Resource as HttpContext;
            var routeValue = httpContext?.GetRouteValue(requirement.RouteParamName);

            if (routeValue == null || !int.TryParse(routeValue.ToString(), out var targetPersonId))
                return Task.CompletedTask;

            if (targetPersonId == currentPersonId)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}