namespace RouteVersioning;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RouteVersioning.Sunset;
using System;

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
		string? link = null,
		string? linkMediaType = null
	)
		where T : struct
	{
		return builder.Sunset(
			at,
			link is string linkString ? (ctx) => linkString : null,
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
		Func<HttpContext, string>? link = null,
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
