using System.Security.Claims;

namespace IMDBApi
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, CurrentUserService currentUserService)
        {
            var id = context.User.Claims.FirstOrDefault(s => s.Type == ClaimTypes.NameIdentifier)?.Value;

            if (id != null)
            {
                currentUserService.SetCurrentUser(Convert.ToInt32(id));
            }

            await _next(context);
        }
    }
}
