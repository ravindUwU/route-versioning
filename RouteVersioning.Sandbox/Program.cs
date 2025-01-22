namespace RouteVersioning.Sandbox;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using RouteVersioning.OpenApi;
using Scalar.AspNetCore;
using System.Threading;
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
		.WithVersion(1, (v) => v
			.ConfigureEndpoints((e) => e
				.AddEndpointFilter<IEndpointConventionBuilder, FilterV1Endpoints>()
			)
			.ConfigureOpenApiInfo((i) =>
			{
				i.Description = "v1 Description!!!";
			})
			.ConfigureOpenApiOptions((options) => options
				.AddDocumentTransformer<V1DocumentTransformer>()
			)
		)
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
			.AddEndpointFilter<IEndpointConventionBuilder, Filter1OnwardEndpoint>();

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

	private class Filter1OnwardEndpoint(ILogger<Filter1OnwardEndpoint> logger) : IEndpointFilter
	{
		public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
		{
			var req = ctx.HttpContext.Request;
			var label = $"{req.Method} {req.GetEncodedPathAndQuery()}";
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

	private class FilterV1Endpoints(ILogger<FilterV1Endpoints> logger) : IEndpointFilter
	{
		public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
		{
			var req = ctx.HttpContext.Request;
			var label = $"{req.Method} {req.GetEncodedPathAndQuery()}";
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

	private class V1DocumentTransformer(
		ILogger<V1DocumentTransformer> logger
	) : IOpenApiDocumentTransformer
	{
		public Task TransformAsync(OpenApiDocument doc, OpenApiDocumentTransformerContext ctx, CancellationToken ct)
		{
			logger.LogInformation("Transformed!!");
			return Task.CompletedTask;
		}
	}
}
