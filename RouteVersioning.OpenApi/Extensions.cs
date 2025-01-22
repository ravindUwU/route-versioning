namespace RouteVersioning.OpenApi;

using Microsoft.Extensions.DependencyInjection;
using RouteVersioning.OpenApi.Transformers;
using System;

public static class Extensions
{
	public static IServiceCollection AddVersionedOpenApi<T>(
		this IServiceCollection services,
		RouteVersions<T> versions
	)
		where T : struct, IComparable
	{
		foreach (var version in versions)
		{
			services.AddOpenApi(versions.Prefix(version), (options) =>
			{
				options.AddDocumentTransformer(new RemoveInapplicableOperationsTransformer<T>(version));
			});
		}

		return services;
	}
}
