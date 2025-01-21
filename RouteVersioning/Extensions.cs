namespace RouteVersioning;

using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;

public static class Extensions
{
	public static VersionedEndpointRouteBuilder<T> WithVersions<T>(
		this IEndpointRouteBuilder routeBuilder,
		params T[] versions
	)
		where T : struct, IComparable
	{
		return new VersionedEndpointRouteBuilder<T>(
			routeBuilder,
			new RouteVersionBuilder<T>(versions.ToHashSet())
		);
	}
}
