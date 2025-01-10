
using Canducci.MongoDB.Repository;
using WebAPI.Repositories;

namespace WebAPI
{
   public class Program
   {
      public static void Main(string[] args)
      {
         WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
         IConfiguration configuration = builder.Configuration;
         // 
         builder.Services.AddControllers();
         builder.Services.AddOpenApi();
         builder.Services.AddScoped<IConnect, Connect>(_ => new Connect(configuration));
         builder.Services.AddScoped<PeopleRepositoryAbstract, PeopleRepository>();
         builder.Services.Configure<RouteOptions>(configuration =>
         {
            configuration.LowercaseUrls = true;
            //configuration.LowercaseQueryStrings = true;
         });
         //
         builder.Services.AddSwaggerGen(action =>
         {
            action.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Description = "Repository API", Version = "1.0.0" });
         });
         //
         WebApplication app = builder.Build();
         if (app.Environment.IsDevelopment())
         {
            app.MapOpenApi();
         }
         app.UseSwagger();
         app.UseSwaggerUI(action =>
         {
            action.SwaggerEndpoint("/swagger/v1/swagger.json", "Repository API");
         });
         app.UseHttpsRedirection();
         app.UseAuthorization();
         app.MapControllers();
         app.Run();
      }
   }
}