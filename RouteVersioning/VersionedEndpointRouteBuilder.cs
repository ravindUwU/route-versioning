namespace RouteVersioning;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Contains methods to map versioned minimal API endpoints, via <see cref="Extensions.WithVersions"/>.
/// </summary>
public class VersionedEndpointRouteBuilder<T> where T : struct
{
	private readonly IEndpointRouteBuilder routeBuilder;
	private readonly RouteVersions<T> versions;

	public VersionedEndpointRouteBuilder(
		IEndpointRouteBuilder routeBuilder,
		RouteVersions<T> versions
	)
	{
		this.routeBuilder = routeBuilder;
		this.versions = versions;
	}

	// GET

	/// <inheritdoc cref="MapMethods(T, IEnumerable{string}, string, Delegate)"/>
	public IEndpointConventionBuilder MapGet(T from, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(from, null, pattern, [HttpMethods.Get], handler);

	/// <inheritdoc cref="MapMethods(ValueTuple{T, T}, IEnumerable{string}, string, Delegate)"/>
	public IEndpointConventionBuilder MapGet((T From, T To) range, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(range.From, range.To, pattern, [HttpMethods.Get], handler);

	// POST

	/// <inheritdoc cref="MapMethods(T, IEnumerable{string}, string, Delegate)"/>
	public IEndpointConventionBuilder MapPost(T from, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(from, null, pattern, [HttpMethods.Post], handler);

	/// <inheritdoc cref="MapMethods(ValueTuple{T, T}, IEnumerable{string}, string, Delegate)"/>
	public IEndpointConventionBuilder MapPost((T From, T To) range, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(range.From, range.To, pattern, [HttpMethods.Post], handler);

	// PUT

	/// <inheritdoc cref="MapMethods(T, IEnumerable{string}, string, Delegate)"/>
	public IEndpointConventionBuilder MapPut(T from, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(from, null, pattern, [HttpMethods.Put], handler);

	/// <inheritdoc cref="MapMethods(ValueTuple{T, T}, IEnumerable{string}, string, Delegate)"/>
	public IEndpointConventionBuilder MapPut((T From, T To) range, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(range.From, range.To, pattern, [HttpMethods.Put], handler);

	// DELETE

	/// <inheritdoc cref="MapMethods(T, IEnumerable{string}, string, Delegate)"/>
	public IEndpointConventionBuilder MapDelete(T from, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(from, null, pattern, [HttpMethods.Delete], handler);

	/// <inheritdoc cref="MapMethods(ValueTuple{T, T}, IEnumerable{string}, string, Delegate)"/>
	public IEndpointConventionBuilder MapDelete((T From, T To) range, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(range.From, range.To, pattern, [HttpMethods.Delete], handler);

	// PATCH

	/// <inheritdoc cref="MapMethods(T, IEnumerable{string}, string, Delegate)"/>
	public IEndpointConventionBuilder MapPatch(T from, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(from, null, pattern, [HttpMethods.Patch], handler);


	/// <inheritdoc cref="MapMethods(ValueTuple{T, T}, IEnumerable{string}, string, Delegate)"/>
	public IEndpointConventionBuilder MapPatch((T From, T To) range, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(range.From, range.To, pattern, [HttpMethods.Patch], handler);

	// Any method

	/// <summary>
	/// Map an endpoint that is available from the specified API version onward.
	/// </summary>
	/// <returns>
	/// Builder that allows configuring conventions for the endpoint across all API versions that
	/// offer it.
	/// </returns>
	public IEndpointConventionBuilder MapMethods(T from, IEnumerable<string> methods, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(from, null, pattern, methods, handler);

	/// <summary>
	/// Map an endpoint that is available in all versions within the specified inclusive range of
	/// API versions.
	/// </summary>
	/// <returns>
	/// Builder that allows configuring conventions for the endpoint across all API versions that
	/// offer it.
	/// </returns>
	public IEndpointConventionBuilder MapMethods((T From, T To) range, IEnumerable<string> methods, [StringSyntax("Route")] string pattern, Delegate handler)
		=> Map(range.From, range.To, pattern, methods, handler);

	// Implementation

	private EndpointConventionBuilder Map(
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
			var meta = versions.GetMetadata(version);

			var shouldMap =
				versions.Compare(version, from) >= 0
				&& (to is null || versions.Compare(version, to.Value) <= 0);

			if (shouldMap)
			{
				var vPattern = $"{versions.GetSlug(version)}/{pattern.TrimStart('/')}";

				var handlerBuilder = routeBuilder.MapMethods(vPattern, methods, handler)
					.WithMetadata(meta);

				foreach (var configure in meta.GetFeatures<Extensions.ConfigureEndpointConventionsDelegate>())
				{
					configure(handlerBuilder);
				}

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
