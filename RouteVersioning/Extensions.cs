namespace RouteVersioning;

using Microsoft.AspNetCore.Routing;
using System;

public static class Extensions
{
	public static VersionedEndpointRouteBuilder<T> WithVersions<T>(
		this IEndpointRouteBuilder routeBuilder,
		RouteVersions<T> versions
	)
		where T : struct, IComparable
	{
		return new VersionedEndpointRouteBuilder<T>(routeBuilder, versions);
	}
}
