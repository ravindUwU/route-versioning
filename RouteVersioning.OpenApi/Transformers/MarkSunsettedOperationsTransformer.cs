namespace RouteVersioning.OpenApi.Transformers;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using RouteVersioning.Sunset;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// <list type="bullet">
/// <item>Sets <see cref="OpenApiOperation.Deprecated"/> to <see langword="true"/> if the
/// corresponding API version has a <see cref="SunsetFeature"/>.</item>
/// </list>
/// </summary>
internal class MarkSunsettedOperationsTransformer : IOpenApiDocumentTransformer
{
	public Task TransformAsync(OpenApiDocument doc, OpenApiDocumentTransformerContext ctx, CancellationToken ct)
	{
		var docActions = new Helpers.OpenApiDocumentActions(ctx);

		foreach (var path in doc.Paths.Values)
		{
			foreach (var op in path.Operations.Values)
			{
				if (
					docActions.TryGetAction(op, out var action)
					&& action.EndpointMetadata.OfType<IRouteVersionMetadata>().SingleOrDefault()
						is IRouteVersionMetadata meta
					&& meta.GetFeatures<SunsetFeature>().SingleOrDefault() is not null
				)
				{
					op.Deprecated = true;
				}
			}
		}

		return Task.CompletedTask;
	}
}
