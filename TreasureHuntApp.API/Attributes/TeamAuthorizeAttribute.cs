using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TreasureHuntApp.Infrastructure.Services;

namespace TreasureHuntApp.API.Attributes;

public class TeamAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthenticationService>();
        var token = context.HttpContext.Request.Headers["X-Team-Session"]
            .FirstOrDefault();

        if (string.IsNullOrEmpty(token))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var isValidTask = authService.ValidateTeamSessionAsync(token);
        isValidTask.Wait();

        if (!isValidTask.Result)
        {
            context.Result = new UnauthorizedResult();
        }
    }
}
