namespace RouteVersioning.OpenApi.Transformers;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// <list type="bullet">
/// <item>Sets <see cref="OpenApiInfo.Version"/> to the version slug.</item>
/// <item>Runs configuration delegates added via <see cref="Extensions.ConfigureOpenApiInfo{T}"/>.</item>
/// </list>
/// </summary>
internal class DocumentInfoTransformer<T>(RouteVersionMetadata<T> meta)
	: IOpenApiDocumentTransformer
	where T : struct
{
	public Task TransformAsync(OpenApiDocument doc, OpenApiDocumentTransformerContext ctx, CancellationToken ct)
	{
		// Configure info.
		var info = doc.Info ??= new();
		info.Version = meta.Set.GetSlug(meta.Version);

		// Run config delegates.
		foreach (var configure in meta.GetFeatures<RouteVersioningOpenApiExtensions.ConfigureOpenApiInfoDelegate>())
		{
			configure(info);
		}

		return Task.CompletedTask;
	}
}
