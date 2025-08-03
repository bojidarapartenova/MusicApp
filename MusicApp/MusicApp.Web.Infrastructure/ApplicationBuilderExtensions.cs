namespace MusicApp.Web.Infrastructure
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    public static class ApplicationBuilderExtensions
    {
        public static async Task<IApplicationBuilder> SeedInitialDataAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            await serviceProvider.SeedAdminAsync(); 
            await serviceProvider.SeedUsersAsync(); 

            return app;
        }
    }
}
