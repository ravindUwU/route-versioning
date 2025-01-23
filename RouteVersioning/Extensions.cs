namespace RouteVersioning;

using Microsoft.AspNetCore.Routing;

public static class Extensions
{
	/// <inheritdoc cref="VersionedRouteContext{T}"/>
	public static VersionedRouteContext<T> WithVersions<T>(
		this IEndpointRouteBuilder routeBuilder,
		RouteVersions<T> versions
	)
		where T : struct

	{
		return new VersionedRouteContext<T>(routeBuilder, versions);
	}
}
