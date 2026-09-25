using Microsoft.AspNetCore.Authorization;

namespace ShopAPI.Authorization.Requirements
{
    public class PersonOwnerOrAdminRequirement : IAuthorizationRequirement
    {
        public string RouteParamName { get; }
        public PersonOwnerOrAdminRequirement(string routeParamName = "id")
        {
            RouteParamName = routeParamName;
        }
    }
}
