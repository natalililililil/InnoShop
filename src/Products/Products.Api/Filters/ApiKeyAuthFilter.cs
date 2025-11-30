using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Products.Api.Filters
{
    public class ApiKeyAuthFilter : IAsyncActionFilter
    {
        private const string APIKEYNAME = "X-Internal-Api-Key";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                context.Result = new UnauthorizedObjectResult("API ключ отсутствует");
                return;
            }

            var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var apiKey = configuration.GetValue<string>("ApiKeys:InternalServiceKey");

            if (!apiKey.Equals(extractedApiKey))
            {
                context.Result = new UnauthorizedObjectResult("Предоставлен неверный API ключ");
                return;
            }

            await next();
        }
    }
}
