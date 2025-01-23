namespace RouteVersioning;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;

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

	/// <summary>
	/// Configures conventions for all endpoints of a specific API version.
	/// </summary>
	public static RouteVersionMetadataBuilder<T> ConfigureEndpoints<T>(
		this RouteVersionMetadataBuilder<T> builder,
		ConfigureEndpointConventionsDelegate configure
	)
		where T : struct
	{
		// TODO: implement this!
		throw new NotImplementedException();
	}

	public delegate void ConfigureEndpointConventionsDelegate(IEndpointConventionBuilder builder);
}
