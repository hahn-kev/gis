using Microsoft.AspNetCore.Authorization;

namespace Backend.Authorization
{
    public class MyAuthorizeAttribute : AuthorizeAttribute
    {
        public MyAuthorizeAttribute(MyPolicies policy)
        {
            Policy = policy.ToString();
        }
    }
}