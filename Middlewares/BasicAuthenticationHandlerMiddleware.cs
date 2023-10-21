using System.Text;

namespace ejercicioSemana3.Middlewares
{
    public class BasicAuthenticationHandlerMiddleware
    {
        private readonly RequestDelegate next;
        private readonly string relm;
        public BasicAuthenticationHandlerMiddleware(RequestDelegate next, string relm)
        {
            this.next = next;
            this.relm = relm;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
            }

            //Basic userid:password
            var header = context.Request.Headers["Authorization"];
            var encodedCreds = header.ToString().Substring(6);
            var creds = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCreds));
            string[] uidpwd = creds.Split(":");
            var uid = uidpwd[0];
            var pwd = uidpwd[1];
            if ((uid != "username" && pwd != "password") || uid != "username" || pwd != "password")
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }
            await next(context);
        }
    }
}
