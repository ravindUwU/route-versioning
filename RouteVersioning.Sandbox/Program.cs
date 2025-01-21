namespace RouteVersioning.Sandbox;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

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
		services.AddOpenApi("current");
	}

	private static void ConfigureApp(WebApplication app)
	{
		app.MapGet("/uwu", () => "UwU");

		// /openapi/current.json
		app.MapOpenApi();

		// /scalar/current
		app.MapScalarApiReference((options) => options
			.WithDefaultOpenAllTags(true)
			.WithDefaultFonts(false)
			.WithModels(false)
			.WithDefaultHttpClient(ScalarTarget.Http, ScalarClient.Http11)
		);
	}
}
