namespace TradeCars.Server;

public static class Program
{
    public static int Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

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