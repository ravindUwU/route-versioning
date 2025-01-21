namespace RouteVersioning;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;


public class VersionedEndpointRouteBuilder<T>
	where T : struct, IComparable
{
	private readonly IEndpointRouteBuilder routeBuilder;
	private readonly RouteVersionBuilder<T> versionBuilder;

	public VersionedEndpointRouteBuilder(
		IEndpointRouteBuilder routeBuilder,
		RouteVersionBuilder<T> versionBuilder
	)
	{
		this.routeBuilder = routeBuilder;
		this.versionBuilder = versionBuilder;
	}

	// GET

	public void MapGet(T from, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(from, null, pattern, [HttpMethods.Get], handler);

	public void MapGet((T From, T To) range, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(range.From, range.To, pattern, [HttpMethods.Get], handler);

	// POST

	public void MapPost(T from, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(from, null, pattern, [HttpMethods.Post], handler);

	public void MapPost((T From, T To) range, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(range.From, range.To, pattern, [HttpMethods.Post], handler);

	// PUT

	public void MapPut(T from, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(from, null, pattern, [HttpMethods.Put], handler);

	public void MapPut((T From, T To) range, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(range.From, range.To, pattern, [HttpMethods.Put], handler);

	// DELETE

	public void MapDelete(T from, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(from, null, pattern, [HttpMethods.Delete], handler);

	public void MapDelete((T From, T To) range, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(range.From, range.To, pattern, [HttpMethods.Delete], handler);

	// PATCH

	public void MapPatch(T from, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(from, null, pattern, [HttpMethods.Patch], handler);

	public void MapPatch((T From, T To) range, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(range.From, range.To, pattern, [HttpMethods.Patch], handler);

	// Methods

	public void MapMethods(T from, IEnumerable<string> methods, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(from, null, pattern, methods, handler);

	public void MapMethods((T From, T To) range, IEnumerable<string> methods, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(range.From, range.To, pattern, methods, handler);

	// Implementation

	private void Map(
		T from,
		T? to,
		[StringSyntax("Route")] string pattern,
		IEnumerable<string> methods,
		Delegate handler
	)
	{
		foreach (var version in versionBuilder.Versions)
		{
			var shouldMap =
				version.CompareTo(from) >= 0
				&& (to is null || version.CompareTo(to) <= 0);

			if (shouldMap)
			{
				var vPattern = $"v{version}/{pattern.TrimStart('/')}";
				routeBuilder.MapMethods(vPattern, methods, handler);
			}
		}
	}
}
