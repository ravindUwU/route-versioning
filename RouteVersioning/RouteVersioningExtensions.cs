namespace RouteVersioning;

using Microsoft.AspNetCore.Routing;

public static class RouteVersioningExtensions
{
	/// <inheritdoc cref="VersionedRouteContext{T}"/>
	public static VersionedRouteContext<T> WithVersions<T>(
		this IEndpointRouteBuilder routeBuilder,
		RouteVersionSet<T> versions
	)
		where T : struct

	{
		return new VersionedRouteContext<T>(routeBuilder, versions);
	}
}
