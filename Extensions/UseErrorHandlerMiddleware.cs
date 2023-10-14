using ejercicioSemana3.Middlewares;
namespace ejercicioSemana3.Extensions
{
    public static class UseErrorHandlerMiddleware
    {
        public static void UseErrorHandler(this IApplicationBuilder app)
        {
            app.UseMiddleware<ErrorHandlerMiddleware>();
        }
    }
}
