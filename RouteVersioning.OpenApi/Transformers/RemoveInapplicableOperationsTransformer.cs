namespace RouteVersioning.OpenApi.Transformers;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal class RemoveInapplicableOperationsTransformer<T>(T version) : IOpenApiDocumentTransformer
	where T : struct
{
	public Task TransformAsync(OpenApiDocument doc, OpenApiDocumentTransformerContext ctx, CancellationToken ct)
	{
		var actionsById = ctx.DescriptionGroups
			.SelectMany((g) => g.Items)
			.Select((d) => d.ActionDescriptor)
			.ToDictionary((d) => d.Id);

		var pathKeysToRemove = new List<string>();

		foreach (var (pathKey, path) in doc.Paths)
		{
			var opKeysToRemove = new List<OperationType>();

			foreach (var (opKey, op) in path.Operations)
			{
				if (
					op.Annotations.TryGetValue("x-aspnetcore-id", out var _actionId)
					&& _actionId is string actionId
					&& actionsById.TryGetValue(actionId, out var action)
					&& action.EndpointMetadata.OfType<RouteVersionEndpointMetadata>().SingleOrDefault() is { } meta
					&& !meta.VersionComparer.Equals(meta.Version, version)
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
