namespace RouteVersioning.OpenApi;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using RouteVersioning.OpenApi.Transformers;
using System;

public static class RouteVersioningOpenApiExtensions
{
	/// <summary>
	/// Adds OpenAPI documents for API versions defined in the specified <see cref="RouteVersionSet{T}"/>;
	/// each version-specific document,
	/// <list type="bullet">
	/// <item>Corresponding to a single API version.</item>
	/// <item>Named using the version slug.</item>
	/// <item>Containing the subset of operations that correspond to its version.</item>
	/// <item>Excluding operations of endpoints of other versions.</item>
	/// </list>
	/// </summary>
	/// <param name="configure">
	/// Configuration delegate for <see cref="OpenApiOptions"/> of all documents (across <em>all
	/// API versions</em>). Use <see cref="ConfigureOpenApiOptions{T}"/> to configure options for a
	/// <em>specific API version</em>.
	/// </param>
	/// <param name="includeUnversionedEndpoints">
	/// Whether the version-specific OpenAPI documents will include operations of unversioned
	/// endpoints. <see langword="true"/> by default.
	/// </param>
	public static IServiceCollection AddVersionedOpenApi<T>(
		this IServiceCollection services,
		RouteVersionSet<T> versions,
		Action<OpenApiOptions>? configure = null,
		bool includeUnversionedEndpoints = true
	)
		where T : struct
	{
		foreach (var version in versions)
		{
			var meta = versions.GetMetadata(version);

			services.AddOpenApi(versions.GetNamedSlug(version), (options) =>
			{
				options.AddDocumentTransformer(new DocumentInfoTransformer<T>(meta));
				options.AddDocumentTransformer(new ExcludeInapplicableOperationsTransformer<T>(
					meta,
					includeUnversionedEndpoints
				));
				options.AddDocumentTransformer(new MarkSunsettedOperationsTransformer());

				foreach (var vConfigure in meta.GetFeatures<ConfigureOpenApiOptionsDelegate>())
				{
					vConfigure(options);
				}

				configure?.Invoke(options);
			});
		}
		return services;
	}

	internal delegate void ConfigureOpenApiOptionsDelegate(OpenApiOptions options);

	/// <summary>
	/// Configure <see cref="OpenApiOptions"/> for a <em>specific API version</em>.
	/// </summary>
	public static RouteVersionMetadataBuilder<T> ConfigureOpenApiOptions<T>(
		this RouteVersionMetadataBuilder<T> builder,
		Action<OpenApiOptions> configure
	)
		where T : struct
	{
		return builder.WithFeature<ConfigureOpenApiOptionsDelegate>((o) => configure(o));
	}

	/// <summary>
	/// Configure the <see cref="OpenApiDocument.Info"/> of the OpenAPI document of a <em>specific
	/// API version</em>.
	/// </summary>
	public static RouteVersionMetadataBuilder<T> ConfigureOpenApiInfo<T>(
		this RouteVersionMetadataBuilder<T> builder,
		Action<OpenApiInfo> configure
	)
		where T : struct
	{
		return builder.WithFeature<DocumentInfoTransformer<T>.ConfigureInfoDelegate>((i) => configure(i));
	}

	/// <summary>
	/// Excludes endpoints that are associated with any API version, from the OpenAPI document.
	/// </summary>
	public static OpenApiOptions ExcludeVersionedOperations(this OpenApiOptions options)
	{
		return options.AddDocumentTransformer(new ExcludeVersionedOperationsTransformer());
	}

	/// <summary>
	/// Marks operations of endpoints associated with retired API versions, as deprecated.
	/// </summary>
	public static OpenApiOptions MarkSunsettedOperations(this OpenApiOptions options)
	{
		return options.AddDocumentTransformer(new MarkSunsettedOperationsTransformer());
	}
}
