namespace RouteVersioning.OpenApi.Tests;

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using RouteVersioning.Tests.Common;
using System.Threading.Tasks;

class OpenApiTestApp : TestApp
{
	public async Task<OpenApiDocument> GetOpenApiDocumentAsync(string path)
		=> new OpenApiStreamReader().Read(await GetStreamAsync(path), out _);

	public static new OpenApiTestApp Make(
		ConfigureServicesDelegate? configureServices = null,
		ConfigureAppDelegate? configureApp = null
	)
	{
		return Make<OpenApiTestApp>(configureServices, configureApp);
	}

	public static new async Task<OpenApiTestApp> StartAsync(
		ConfigureServicesDelegate? configureServices = null,
		ConfigureAppDelegate? configureApp = null
	)
	{
		return await StartAsync<OpenApiTestApp>(configureServices, configureApp);
	}
}
