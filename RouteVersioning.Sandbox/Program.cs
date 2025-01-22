namespace RouteVersioning.Sandbox;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RouteVersioning.OpenApi;
using Scalar.AspNetCore;
using System.Threading.Tasks;

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

	private static readonly RouteVersions<int> apiVersions = new RouteVersionBuilder<int>()
		.WithVersion(1)
		.WithVersion(2)
		.WithVersion(3)
		.Build();

	private static void ConfigureServices(WebApplicationBuilder app)
	{
		var services = app.Services;

		services.AddOpenApi("current");
		services.AddVersionedOpenApi(apiVersions);
	}

	private static void ConfigureApp(WebApplication app)
	{
		app.MapGet("/uwu", () => "UwU");

		var api = app.MapGroup("api").WithVersions(apiVersions);

		api.MapGet(1, "1-onward", () => "1-onward")
			.AddEndpointFilter<IEndpointConventionBuilder, LoggingEndpointFilter>();

		api.MapGet(2, "2-onward", () => "2-onward");
		api.MapGet(3, "3-onward", () => "3-onward");

		api.MapGet((1, 2), "1-to-2", () => "1-to-2");
		api.MapGet((2, 3), "2-to-3", () => "2-to-3");

		// /openapi/current.json
		// /openapi/v[1-3].json
		app.MapOpenApi();

		// /scalar/current
		app.MapScalarApiReference((options) => options
			.WithDefaultOpenAllTags(true)
			.WithDefaultFonts(false)
			.WithDefaultHttpClient(ScalarTarget.Http, ScalarClient.Http11)
		);
	}

	private class LoggingEndpointFilter(
		ILogger<LoggingEndpointFilter> logger
	) : IEndpointFilter
	{
		public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
		{
			var req = ctx.HttpContext.Request;
			var label = $"{req.Method} {req.Path}";
			try
			{
				logger.LogInformation("Started: {label}", label);
				return await next(ctx);
			}
			finally
			{
				logger.LogInformation("Finished: {label}", label);
			}
		}
	}
}
