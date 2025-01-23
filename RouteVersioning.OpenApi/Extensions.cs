namespace RouteVersioning.OpenApi;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using RouteVersioning.OpenApi.Transformers;
using System;

public static class Extensions
{
	public static IServiceCollection AddVersionedOpenApi<T>(
		this IServiceCollection services,
		RouteVersions<T> versions,
		Action<OpenApiOptions>? configure = null,
		bool includeUnversionedEndpoints = true
	)
		where T : struct
	{
		foreach (var version in versions)
		{
			var meta = versions.GetMetadata(version);

			services.AddOpenApi(versions.GetSlug(version), (options) =>
			{
				options.AddDocumentTransformer(new DocumentInfoTransformer<T>(versions, version));
				options.AddDocumentTransformer(new ExcludeInapplicableOperationsTransformer<T>(
					version,
					includeUnversionedEndpoints
				));

				foreach (var vConfigure in meta.GetFeatures<ConfigureOpenApiOptionsDelegate>())
				{
					vConfigure(options);
				}

				configure?.Invoke(options);
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

	public static OpenApiOptions ExcludeVersionedOperations(this OpenApiOptions options)
	{
		return options.AddDocumentTransformer(new ExcludeVersionedOperationsTransformer());
	}
}
