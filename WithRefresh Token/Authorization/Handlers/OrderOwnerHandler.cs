using Microsoft.AspNetCore.Authorization;
using ShopAPI.Authorization.Requirements;
using ShopAPI.Extensions;
using ShopAPI.Repositories;

namespace ShopAPI.Authorization.Handlers
{
    public class OrderOwnerHandler : AuthorizationHandler<OrderOwnerOrAdminRequirement>
    {
        private readonly IOrderRepository _orderRepository;

        public OrderOwnerHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            OrderOwnerOrAdminRequirement requirement)
        {
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return;
            }

            var currentPersonId = context.User.GetPersonId();
            if (currentPersonId == null)
                return;

            var httpContext = context.Resource as HttpContext;
            var routeValue = httpContext?.GetRouteValue(requirement.RouteParamName);

            if (routeValue == null || !int.TryParse(routeValue.ToString(), out var orderId))
                return;

            int? ownerId = await _orderRepository.GetOwnerIdAsync(orderId);

            if (ownerId.HasValue && ownerId == currentPersonId)
                context.Succeed(requirement);
        }
    }
}