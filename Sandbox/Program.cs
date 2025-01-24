namespace Sandbox;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using RouteVersioning;
using RouteVersioning.OpenApi;
using Scalar.AspNetCore;
using System;
using System.Linq;
using System.Text.Json;
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

	private static readonly RouteVersionSet<int> api1Versions = new RouteVersionSetBuilder<int>("api1")
		.Version(1, (v) => v
			.Sunset(
				at: DateTime.Now.AddMonths(-1),
				link: "https://www.example.com",
				linkMediaType: "text/html"
			)
			.ConfigureOpenApiInfo((i) =>
			{
				i.Description = "v1 Description!!!";
			})
			.ConfigureOpenApiOptions((options) => options
				.AddDocumentTransformer<V1DocumentTransformer>()
			)
			.AddEndpointFilter<IEndpointConventionBuilder, FilterV1Endpoints>()
		)
		.Version(2, (v) => v
			.Sunset(
				at: DateTime.Now.AddMonths(1),
				link: "https://www.example.com",
				linkMediaType: "text/html"
			)
		)
		.Version(3)
		.Build();

	private static readonly RouteVersionSet<int> api2Versions = new RouteVersionSetBuilder<int>("api2")
		.Version(1)
		.Version(2)
		.Version(3)
		.Build();

	private static void ConfigureServices(WebApplicationBuilder app)
	{
		var services = app.Services;

		services.AddOpenApi("current", (options) => options
			.AddDocumentTransformer(new ClearServers())
			.MarkSunsettedOperations()
		);

		services.AddVersionedOpenApi(
			api1Versions,
			(options) => options.AddDocumentTransformer(new ClearServers()),
			includeUnversionedEndpoints: false
		);

		services.AddVersionedOpenApi(
			api2Versions,
			(options) => options.AddDocumentTransformer(new ClearServers()),
			includeUnversionedEndpoints: false
		);
	}

	private static void ConfigureApp(WebApplication app)
	{
		app.MapGet("uwu", () => "UwU");

		Func<string> handleA = () => "a";
		Func<string> handleB = () => "b";
		Func<string> handleC = () => "c";

		var api1 = app.MapGroup("api1").WithVersions(api1Versions);
		{
			api1.From(1).MapGet("a", handleA).AddEndpointFilter<FilterAEndpoint>();
			api1.From(2).MapGet("b", handleB);
			api1.From(3).MapGet("c", handleC);

			api1.Between(1, 2).MapGet("d", () => "d");

			api1.Between(1, 2).MapGet("e", () => "e");
			api1.From(3).MapGet("e", () => "e");

			api1.Between(1, 2).MapGet("owo", () => "owo");
			api1.From(3).MapGet("owo", () => "OwO");
		}

		var api2 = app.MapGroup("api2").WithVersions(api2Versions);
		{
			api2.From(1).MapGet("a", handleA);
			api2.From(2).MapGet("b", handleB);
			api2.From(3).MapGet("c", handleC);
		}

		// openapi/current.json
		// openapi/v{1,2,3}.json
		app.MapOpenApi();

		// scalar/current
		// scalar/v{1,2,3}
		app.MapScalarApiReference((options) => options
			.WithDefaultOpenAllTags(true)
			.WithDefaultFonts(false)
			.WithDefaultHttpClient(ScalarTarget.Http, ScalarClient.Http11)
		);

		// Swagger UI
		app.MapGet("swagger", () =>
		{
			var urls = new[] { "current" }
				.Concat(api1Versions.Select(api1Versions.GetNamedSlug))
				.Concat(api2Versions.Select(api2Versions.GetNamedSlug))
				.Select((name) => new { name, url = $"/openapi/{name}.json" });

			return Results.Content(contentType: "text/html", content: $$"""
				<!DOCTYPE html>
				<html>
				<head>
					<meta name="viewport" content="width=device-width, initial-scale=1" />
					<style>body { margin: 0 }</style>
					<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/swagger-ui-dist@5.18.2/swagger-ui.css" />
				</head>
				<body>
					<div id="swagger-ui"></div>
					<script src="https://cdn.jsdelivr.net/npm/swagger-ui-dist@5.18.2/swagger-ui-bundle.js"></script>
					<script src="https://cdn.jsdelivr.net/npm/swagger-ui-dist@5.18.2/swagger-ui-standalone-preset.js"></script>
					<script>
						SwaggerUIBundle({
							urls: {{JsonSerializer.Serialize(urls)}},
							dom_id: '#swagger-ui',
							presets: [
								SwaggerUIBundle.presets.apis,
								SwaggerUIStandalonePreset,
							],
							layout: 'StandaloneLayout',
						});
					</script>
				</body>
				</html>
				"""
			);
		});
	}

	private class FilterAEndpoint(ILogger<FilterAEndpoint> logger) : IEndpointFilter
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

	private class ClearServers : IOpenApiDocumentTransformer
	{
		public Task TransformAsync(OpenApiDocument doc, OpenApiDocumentTransformerContext ctx, CancellationToken ct)
		{
			doc.Servers?.Clear();
			return Task.CompletedTask;
		}
	}
}
