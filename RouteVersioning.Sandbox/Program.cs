namespace RouteVersioning.Sandbox;

using Microsoft.AspNetCore.Builder;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);
		ConfigureServices(builder);

		var app = builder.Build();
		ConfigureApp(app);

		app.Run();
	}

	private static void ConfigureServices(WebApplicationBuilder app)
	{
		var services = app.Services;
	}

	private static void ConfigureApp(WebApplication app)
	{
		app.MapGet("/uwu", () => "UwU");
	}
}
