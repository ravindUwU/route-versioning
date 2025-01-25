namespace RouteVersioning.Tests.Common;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

public class TestApp
{
	public WebApplication App { get; private set; } = null!;

	public IServiceProvider Services => App.Services;

	// Endpoints

	public RouteEndpoint? GetRouteEndpoint(string method, [StringSyntax("Route")] string pattern)
	{
		return Services.GetRequiredService<EndpointDataSource>().Endpoints
			.OfType<RouteEndpoint>()
			.SingleOrDefault((e) =>
				e.Metadata.Any((m) => m is HttpMethodMetadata meta && meta.HttpMethods.Contains(method))
				&& e.RoutePattern.RawText == pattern
			);
	}

	// Client

	private HttpClient? _Client;
	public HttpClient Client => _Client ??= App.GetTestClient();

	public async Task<HttpResponseMessage> GetAsync(string path)
		=> (await Client.GetAsync(path, TestContext.Current.CancellationToken));

	public async Task<Stream> GetStreamAsync(string path)
		=> (await Client.GetStreamAsync(path, TestContext.Current.CancellationToken));

	public async Task<HttpStatusCode> GetStatusCodeAsync(string path)
		=> (await GetAsync(path)).StatusCode;

	public async Task<string?> GetSingleHeaderAsync(string path, string name)
		=> GetSingleHeaderAsync(await GetAsync(path), name);

	public string? GetSingleHeaderAsync(HttpResponseMessage message, string name)
		=> message.Headers.TryGetValues(name, out var v) ? v.SingleOrDefault() : null;

	public async Task<string> GetStringAsync(string path)
		=> await Client.GetStringAsync(path, TestContext.Current.CancellationToken);

	// Helpers

	public delegate void ConfigureServicesDelegate(IServiceCollection services, WebApplicationBuilder app);
	public delegate void ConfigureAppDelegate(WebApplication app);

	public static TestApp Make(
		ConfigureServicesDelegate? configureServices = null,
		ConfigureAppDelegate? configureApp = null
	)
	{
		return Make<TestApp>(configureServices, configureApp);
	}

	public static T Make<T>(
		ConfigureServicesDelegate? configureServices = null,
		ConfigureAppDelegate? configureApp = null
	)
		where T : TestApp, new()
	{
		var builder = WebApplication.CreateEmptyBuilder(new());
		builder.WebHost.UseTestServer();

		builder.Services.AddRouting();
		configureServices?.Invoke(builder.Services, builder);

		var app = builder.Build();
		configureApp?.Invoke(app);

		return new T
		{
			App = app,
		};
	}

	public static async Task<TestApp> StartAsync(
		ConfigureServicesDelegate? configureServices = null,
		ConfigureAppDelegate? configureApp = null
	)
	{
		return await StartAsync<TestApp>(configureServices, configureApp);
	}

	public static async Task<T> StartAsync<T>(
		ConfigureServicesDelegate? configureServices = null,
		ConfigureAppDelegate? configureApp = null
	)
		where T : TestApp, new()
	{
		var app = Make<T>(configureServices, configureApp);
		await app.App.StartAsync(TestContext.Current.CancellationToken);
		return app;
	}
}
