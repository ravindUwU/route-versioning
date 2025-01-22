namespace RouteVersioning;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

public class VersionedEndpointRouteBuilder<T>(
	IEndpointRouteBuilder routeBuilder,
	RouteVersions<T> versions
)
	where T : struct, IComparable
{

	// GET

	public IEndpointConventionBuilder MapGet(T from, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(from, null, pattern, [HttpMethods.Get], handler);

	public IEndpointConventionBuilder MapGet((T From, T To) range, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(range.From, range.To, pattern, [HttpMethods.Get], handler);

	// POST

	public IEndpointConventionBuilder MapPost(T from, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(from, null, pattern, [HttpMethods.Post], handler);

	public IEndpointConventionBuilder MapPost((T From, T To) range, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(range.From, range.To, pattern, [HttpMethods.Post], handler);

	// PUT

	public IEndpointConventionBuilder MapPut(T from, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(from, null, pattern, [HttpMethods.Put], handler);

	public IEndpointConventionBuilder MapPut((T From, T To) range, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(range.From, range.To, pattern, [HttpMethods.Put], handler);

	// DELETE

	public IEndpointConventionBuilder MapDelete(T from, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(from, null, pattern, [HttpMethods.Delete], handler);

	public IEndpointConventionBuilder MapDelete((T From, T To) range, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(range.From, range.To, pattern, [HttpMethods.Delete], handler);

	// PATCH

	public IEndpointConventionBuilder MapPatch(T from, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(from, null, pattern, [HttpMethods.Patch], handler);

	public IEndpointConventionBuilder MapPatch((T From, T To) range, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(range.From, range.To, pattern, [HttpMethods.Patch], handler);

	// Methods

	public IEndpointConventionBuilder MapMethods(T from, IEnumerable<string> methods, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(from, null, pattern, methods, handler);

	public IEndpointConventionBuilder MapMethods((T From, T To) range, IEnumerable<string> methods, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(range.From, range.To, pattern, methods, handler);

	// Implementation

	private IEndpointConventionBuilder Map(
		T from,
		T? to,
		[StringSyntax("Route")] string pattern,
		IEnumerable<string> methods,
		Delegate handler
	)
	{
		if (!versions.Contains(from))
		{
			throw new ArgumentOutOfRangeException(
				message: $@"Invalid ""from"" version specified while mapping `{String.Join(",", methods)} {pattern}`.",
				paramName: nameof(from),
				actualValue: from
			);
		}

		if (to is not null && !versions.Contains(to.Value))
		{
			throw new ArgumentOutOfRangeException(
				message: $@"Invalid ""to"" version specified while mapping `{String.Join(",", methods)} {pattern}`.",
				paramName: nameof(to),
				actualValue: to
			);
		}

		var endpointBuilders = new List<IEndpointConventionBuilder>();

		foreach (var version in versions)
		{
			var meta = new RouteVersionEndpointMetadata
			{
				Version = version,
				VersionComparer = EqualityComparer<T>.Default,
			};

			var shouldMap =
				version.CompareTo(from) >= 0
				&& (to is null || version.CompareTo(to) <= 0);

			if (shouldMap)
			{
				var vPattern = $"{versions.Prefix(version)}/{pattern.TrimStart('/')}";

				var handlerBuilder = routeBuilder.MapMethods(vPattern, methods, handler)
					.WithMetadata(meta);

				endpointBuilders.Add(handlerBuilder);
			}
		}

		return new EndpointConventionBuilder(endpointBuilders);
	}

	private class EndpointConventionBuilder(IEnumerable<IEndpointConventionBuilder> builders)
		: IEndpointConventionBuilder
	{
		public void Add(Action<EndpointBuilder> convention)
		{
			foreach (var builder in builders)
			{
				builder.Add(convention);
			}
		}

		public void Finally(Action<EndpointBuilder> convention)
		{
			foreach (var builder in builders)
			{
				builder.Finally(convention);
			}
		}
	}
}
