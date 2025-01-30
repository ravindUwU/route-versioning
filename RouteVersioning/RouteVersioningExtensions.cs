namespace RouteVersioning;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RouteVersioning.Sunset;
using System;

public static class RouteVersioningExtensions
{
	/// <inheritdoc cref="VersionedRouteBuilder{T}"/>
	public static VersionedRouteBuilder<T> WithVersions<T>(
		this IEndpointRouteBuilder routeBuilder,
		RouteVersionSet<T> set
	)
		where T : struct
	{
		return new VersionedRouteBuilder<T>(routeBuilder, set);
	}

	/// <summary>
	/// Indicate that a specific API version is, or will be retired; prior to possibly being
	/// completely removed.
	/// </summary>
	/// <param name="at">
	/// The instant at which point onward, the endpoints are considered retired.
	/// </param>
	public static RouteVersionMetadataBuilder<T> Sunset<T>(
		this RouteVersionMetadataBuilder<T> builder,
		DateTime at
	)
		where T : struct
	{
		return builder.Sunset(
			at,
			null as Func<HttpContext, Uri>,
			null
		);
	}

	/// <summary>
	/// Indicate that a specific API version is, or will be retired; prior to possibly being
	/// completely removed.
	/// </summary>
	/// <param name="at">
	/// The instant at which point onward, the endpoints are considered retired.
	/// </param>
	/// <param name="link">
	/// URL to a resource that includes details about the API retirement.
	/// </param>
	/// <param name="linkMediaType">
	/// The media type (e.g., <c>text/html</c>) of the resource linked via <paramref name="link"/>.
	/// </param>
	public static RouteVersionMetadataBuilder<T> Sunset<T>(
		this RouteVersionMetadataBuilder<T> builder,
		DateTime at,
		Uri? link = null,
		string? linkMediaType = null
	)
		where T : struct
	{
		return builder.Sunset(
			at,
			link is Uri linkUri ? (ctx) => linkUri : null,
			linkMediaType
		);
	}

	/// <summary>
	/// Indicate that a specific API version is, or will be retired; prior to possibly being
	/// completely removed.
	/// </summary>
	/// <param name="at">
	/// The instant at which point onward, the endpoints are considered retired.
	/// </param>
	/// <param name="link">
	/// Function that returns a URL to a resource that includes details about the API retirement.
	/// </param>
	/// <param name="linkMediaType">
	/// The media type (e.g., <c>text/html</c>) of the resource linked via <paramref name="link"/>.
	/// </param>
	public static RouteVersionMetadataBuilder<T> Sunset<T>(
		this RouteVersionMetadataBuilder<T> builder,
		DateTime at,
		Func<HttpContext, Uri>? link = null,
		string? linkMediaType = null
	)
		where T : struct
	{
		var sunset = new SunsetFeature
		{
			At = at,
			Link = link,
			LinkMediaType = linkMediaType,
		};

		return builder
			.WithFeature(sunset)
			.AddEndpointFilter(new SunsetEndpointFilter(sunset));
	}
}
