using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.OpenApi.Models;

namespace TradeCars.Server;

public static class Program
{
    public static int Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Allow local static files to be hosted locally with an environment besides "development" - https://stackoverflow.com/a/74498969/7453
        // TODO: how to do this in ASP.NET Core 7 to get rid of the compiler warning?  See https://stackoverflow.com/a/74498969/7453
        builder.WebHost.ConfigureAppConfiguration((ctx, _) =>
            StaticWebAssetsLoader.UseStaticWebAssets(ctx.HostingEnvironment, ctx.Configuration)
        );

        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

        OpenApiInfo apiInfo = new() { Title = "TradeCars", Version = "v1", Description = "TradeCars API" };
        builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", apiInfo));

        var app = builder.Build();

        Console.WriteLine($"{builder.Environment.EnvironmentName} environment detected.");

        if (builder.Environment.IsEnvironment("Development"))
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseResponseCompression(); // hurts hot reload, so only use in non-developer
        }
        app.UseDeveloperExceptionPage();

        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        app.UseRouting();
        app.MapRazorPages();
        app.MapControllers();
        app.MapDefaultControllerRoute();
        app.MapFallbackToFile("index.html");

        Console.WriteLine("Starting...");
        app.Run();

        return 0;
    }

}