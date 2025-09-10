using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TreasureHuntApp.Infrastructure.Services;

namespace TreasureHuntApp.API.Attributes;

public class AdminAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthenticationService>();
        var token = context.HttpContext.Request.Headers["Authorization"]
            .FirstOrDefault()?.Split(" ").Last();

        if (string.IsNullOrEmpty(token))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var isValidTask = authService.ValidateAdminTokenAsync(token);
        isValidTask.Wait();

        if (!isValidTask.Result)
        {
            context.Result = new UnauthorizedResult();
        }
    }
}

