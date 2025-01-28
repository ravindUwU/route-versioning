namespace RouteVersioning.Sunset;

using Microsoft.AspNetCore.Http;
using System;

/// <summary>
/// Indicate that a specific API version is, or will be retired.
/// </summary>
public class SunsetFeature
{
	internal SunsetFeature()
	{
	}

	/// <summary>
	/// The instant at which point onward, the endpoints are considered retired.
	/// </summary>
	public required DateTime At { get; init; }

	/// <summary>
	/// Function that returns a URL to a resource that includes details about the API retirement.
	/// </summary>
	public Func<HttpContext, Uri>? Link { get; init; }

	/// <summary>
	/// The media type of the resource linked via <see cref="Link"/>.
	/// </summary>
	public string? LinkMediaType { get; init; }
}
