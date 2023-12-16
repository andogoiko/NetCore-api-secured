
namespace Middlewares{

    public class AuthorizationHeaderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthorizationHeaderMiddleware(RequestDelegate next, IHttpContextAccessor httpContextAccessor)
        {
            _next = next;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Invoke(HttpContext context)
        {
            string accessToken = context.Request.Cookies["AccessToken"]; // Implementa lógica para obtener el access token

            if (!string.IsNullOrEmpty(accessToken))
            {
                if (!context.Request.Headers.ContainsKey("Authorization"))
                {
                    context.Request.Headers.Add("Authorization", $"Bearer {accessToken}");
                }
            }

            await _next(context);
        }
    }

}
