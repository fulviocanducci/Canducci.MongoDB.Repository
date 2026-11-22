using Canducci.MongoDB.Repository;
using Canducci.MongoDB.Repository.DependencyInjection;

namespace Web11
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;
            builder.Services.AddMongoDBRepository(config =>
            {
                config.ConnectionString = "mongodb://admin:770301Ab%40@localhost:27017/?authSource=admin";
                config.Database = "db";
                config.RegistrationMode = RepositoryRegistrationMode.Proxy;
            });
            builder.Services.AddControllersWithViews();

            WebApplication app = builder.Build();
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.MapStaticAssets();
            app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}").WithStaticAssets();
            app.Run();
        }
    }
}
