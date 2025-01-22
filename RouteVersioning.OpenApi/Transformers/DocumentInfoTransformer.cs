namespace RouteVersioning.OpenApi.Transformers;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

internal class DocumentInfoTransformer<T>(RouteVersions<T> versions, T version)
	: IOpenApiDocumentTransformer
	where T : struct, IComparable
{
	public Task TransformAsync(OpenApiDocument doc, OpenApiDocumentTransformerContext ctx, CancellationToken ct)
	{
		var meta = versions.GetMetadata(version);

		var info = doc.Info ??= new();
		info.Version = versions.Prefix(version);

		foreach (var configure in meta.GetFeatures<Extensions.ConfigureOpenApiInfoDelegate>())
		{
			configure(info);
		}

		return Task.CompletedTask;
	}
}
