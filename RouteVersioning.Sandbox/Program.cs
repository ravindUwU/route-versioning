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

		var api = app.MapGroup("api").WithVersions(1, 2, 3).Map();

		api.MapGet(1, "1-onward", () => "1-onward");
		api.MapGet(2, "2-onward", () => "2-onward");
		api.MapGet(3, "3-onward", () => "3-onward");

		api.MapGet((1, 2), "1-to-2", () => "1-to-2");
		api.MapGet((2, 3), "2-to-3", () => "2-to-3");

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
