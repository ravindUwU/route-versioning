namespace RouteVersioning.Sunset;

using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Adds RFC 8594 <c>Sunset</c> and <c>Link</c> headers to an <see cref="HttpResponse"/> according
/// a <see cref="SunsetFeature"/>.
/// </summary>
/// <seealso href="https://datatracker.ietf.org/doc/html/rfc8594"/>
/// <seealso href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Link"/>
internal class SunsetEndpointFilter : IEndpointFilter
{
	private readonly SunsetFeature sunset;

	internal SunsetEndpointFilter(SunsetFeature sunset)
	{
		this.sunset = sunset;
	}

	public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
	{
		var result = await next(ctx);
		var httpCtx = ctx.HttpContext;

		httpCtx.Response.Headers["Sunset"] = sunset.At.ToString("r");

		if (sunset.Link?.Invoke(httpCtx) is Uri sunsetUri)
		{
			httpCtx.Response.Headers.Append("Link", sunset.LinkMediaType is null
				? $@"<{sunsetUri}>; rel=""sunset"""
				: $@"<{sunsetUri}>; rel=""sunset""; type=""{sunset.LinkMediaType}"""
			);
		}

		return result;
	}
}
