using Microsoft.AspNetCore.Authorization;

namespace ShopAPI.Authorization.Requirements
{
    public class OrderOwnerOrAdminRequirement : IAuthorizationRequirement
    {
        public string RouteParamName { get; }

        public OrderOwnerOrAdminRequirement(string routeParamName = "id")
        {
            RouteParamName = routeParamName;
        }
    }
}