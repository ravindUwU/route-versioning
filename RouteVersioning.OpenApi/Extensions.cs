namespace RouteVersioning.OpenApi;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using RouteVersioning.OpenApi.Transformers;

public static class Extensions
{
	public static IServiceCollection AddVersionedOpenApi<T>(
		this IServiceCollection services,
		RouteVersions<T> versions
	)
		where T : struct
	{
		foreach (var version in versions)
		{
			var meta = versions.GetMetadata(version);

			services.AddOpenApi(versions.GetSlug(version), (options) =>
			{
				options.AddDocumentTransformer(new DocumentInfoTransformer<T>(versions, version));
				options.AddDocumentTransformer(new RemoveInapplicableOperationsTransformer<T>(version));

				foreach (var configure in meta.GetFeatures<ConfigureOpenApiOptionsDelegate>())
				{
					configure(options);
				}
			});
		}
		return services;
	}

	public static RouteVersionMetadataBuilder<T> ConfigureOpenApiOptions<T>(
		this RouteVersionMetadataBuilder<T> builder,
		ConfigureOpenApiOptionsDelegate configure
	)
		where T : struct
	{
		return builder.WithFeature(configure);
	}

	public delegate void ConfigureOpenApiOptionsDelegate(OpenApiOptions options);

	public static RouteVersionMetadataBuilder<T> ConfigureOpenApiInfo<T>(
		this RouteVersionMetadataBuilder<T> builder,
		ConfigureOpenApiInfoDelegate configure
	)
		where T : struct
	{
		return builder.WithFeature(configure);
	}

	public delegate void ConfigureOpenApiInfoDelegate(OpenApiInfo info);
}
