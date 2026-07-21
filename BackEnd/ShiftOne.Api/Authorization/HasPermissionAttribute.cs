using Microsoft.AspNetCore.Authorization;
using ShiftOne.Shared.Constants;

namespace ShiftOne.Api.Authorization
{
    public sealed class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(string permission)
        {
            Policy = $"{Permissions.PolicyPrefix}:{permission}";
        }
    }
}
