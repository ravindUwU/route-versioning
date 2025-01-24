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
	private readonly RouteVersionMetadata<T> meta;
	private readonly bool includeUnversionedEndpoints;

	public ExcludeInapplicableOperationsTransformer(
		RouteVersionMetadata<T> meta,
		bool includeUnversionedEndpoints
	)
	{
		this.meta = meta;
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
					var remove = true;

					var endpointMetas = action.EndpointMetadata.OfType<IRouteVersionMetadata>().ToList();
					if (endpointMetas.Count is 0)
					{
						remove = !includeUnversionedEndpoints;
					}
					else
					{
						foreach (var endpointMeta in endpointMetas)
						{
							if (
								endpointMeta.Set == meta.Set
								&& endpointMeta.IsVersion(meta.Version)
							)
							{
								remove = false;
								break;
							}
						}
					}

					if (remove)
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
