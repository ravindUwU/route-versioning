namespace RouteVersioning.OpenApi.Transformers;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal class ExcludeInapplicableOperationsTransformer<T>(T version)
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
				if (
					docActions.TryGetAction(op, out var action)
					&& action.EndpointMetadata.OfType<IRouteVersionMetadata>().SingleOrDefault() is { } meta
					&& !meta.IsVersion(version)
				)
				{
					opKeysToRemove.Add(opKey);
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
