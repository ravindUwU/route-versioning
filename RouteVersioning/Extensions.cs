namespace RouteVersioning;

using Microsoft.AspNetCore.Builder;
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

	public static RouteVersionMetadataBuilder<T> ConfigureEndpoints<T>(
		this RouteVersionMetadataBuilder<T> builder,
		ConfigureEndpointConventionsDelegate configure
	)
		where T : struct, IComparable
	{
		return builder.WithFeature(configure);
	}

	public delegate void ConfigureEndpointConventionsDelegate(IEndpointConventionBuilder builder);
}
