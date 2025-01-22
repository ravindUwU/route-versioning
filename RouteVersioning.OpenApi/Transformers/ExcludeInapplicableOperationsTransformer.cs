namespace RouteVersioning.OpenApi.Transformers;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal class ExcludeInapplicableOperationsTransformer<T>(
	T version,
	bool includeUnversionedEndpoints
)
	: IOpenApiDocumentTransformer
	where T : struct
{
	public Task TransformAsync(OpenApiDocument doc, OpenApiDocumentTransformerContext ctx, CancellationToken ct)
	{
		var docActions = new Helpers.OpenApiDocumentActions(ctx);

		var pathKeysToRemove = new List<string>();
		foreach (var (pathKey, path) in doc.Paths)
		{
			var opKeysToRemove = new List<OperationType>();
			foreach (var (opKey, op) in path.Operations)
			{
				if (docActions.TryGetAction(op, out var action))
				{
					var meta = action.EndpointMetadata.OfType<IRouteVersionMetadata>().SingleOrDefault();

					if (
						(meta is null && !includeUnversionedEndpoints)
						|| (meta is not null && !meta.IsVersion(version))
					)
					{
						opKeysToRemove.Add(opKey);
					}
				}
			}

			foreach (var opKey in opKeysToRemove)
			{
				path.Operations.Remove(opKey);
			}

			if (path.Operations.Count is 0)
			{
				pathKeysToRemove.Add(pathKey);
			}
		}

		foreach (var pathKey in pathKeysToRemove)
		{
			doc.Paths.Remove(pathKey);
		}

		return Task.CompletedTask;
	}
}
