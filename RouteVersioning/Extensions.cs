namespace RouteVersioning;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

public static class Extensions
{
	/// <summary>
	/// Map versioned minimal API endpoints.
	/// </summary>
	public static VersionedEndpointRouteBuilder<T> WithVersions<T>(
		this IEndpointRouteBuilder routeBuilder,
		RouteVersions<T> versions
	)
		where T : struct
	{
		return new VersionedEndpointRouteBuilder<T>(routeBuilder, versions);
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
		return builder.WithFeature(configure);
	}

	public delegate void ConfigureEndpointConventionsDelegate(IEndpointConventionBuilder builder);
}
