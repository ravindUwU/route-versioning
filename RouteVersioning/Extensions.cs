namespace RouteVersioning;

using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;

public static class Extensions
{
	public static RouteVersionBuilder<T, IEndpointRouteBuilder> WithVersions<T>(
		this IEndpointRouteBuilder routeBuilder,
		params HashSet<T> versions
	)
		where T : struct, IComparable
	{
		return new RouteVersionBuilder<T, IEndpointRouteBuilder>(
			versions,
			context: routeBuilder
		);
	}

	public static VersionedEndpointRouteBuilder<T> Map<T>(
		this RouteVersionBuilder<T, IEndpointRouteBuilder> versionBuilder
	)
		where T : struct, IComparable
	{
		return new VersionedEndpointRouteBuilder<T>(
			routeBuilder: versionBuilder.Context,
			versions: versionBuilder.Build()
		);
	}
}
