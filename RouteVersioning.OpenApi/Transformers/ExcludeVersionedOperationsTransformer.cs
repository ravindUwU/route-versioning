namespace RouteVersioning.OpenApi.Transformers;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// <list type="bullet">
/// <item>Removes operations of endpoints that are associated with any API version.</item>
/// </list>
/// </summary>
internal class ExcludeVersionedOperationsTransformer : IOpenApiDocumentTransformer
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
					&& action.EndpointMetadata.OfType<IRouteVersionMetadata>().Any()
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
