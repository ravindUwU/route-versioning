namespace RouteVersioning.OpenApi.Transformers;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// <list type="bullet">
/// <item>Removes operations of endpoints that aren't included in a specific API version.</item>
/// <item>Removes operations of unversioned endpoints if <see cref="includeUnversionedEndpoints"/> is <see langword="false"/>.</item>
/// </list>
/// </summary>
internal class ExcludeInapplicableOperationsTransformer<T> : IOpenApiDocumentTransformer
	where T : struct
{
	private readonly T version;
	private readonly bool includeUnversionedEndpoints;

	public ExcludeInapplicableOperationsTransformer(
		T version,
		bool includeUnversionedEndpoints
	)
	{
		this.version = version;
		this.includeUnversionedEndpoints = includeUnversionedEndpoints;
	}

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
