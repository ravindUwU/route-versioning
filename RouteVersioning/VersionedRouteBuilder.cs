namespace RouteVersioning;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

/// <summary>
/// Allows mapping routes associated with the specified <see cref="RouteVersionSet{T}"/>.
/// </summary>
public sealed class VersionedRouteBuilder<T>(IEndpointRouteBuilder outer, RouteVersionSet<T> set)
	where T : struct
{
	/// <summary>
	/// Map endpoints available from the specified API version onward.
	/// </summary>
	public IEndpointRouteBuilder From(T from)
	{
		return MakeBuilder(from, to: null);
	}

	/// <summary>
	/// Map endpoints available in all API versions between the specified inclusive range.
	/// </summary>
	public IEndpointRouteBuilder Between(T from, T to)
	{
		return MakeBuilder(from, to);
	}

	private InternalBuilder MakeBuilder(T from, T? to)
	{
		if (!set.Contains(from))
		{
			throw new ArgumentOutOfRangeException(
				message: $@"Invalid version specified: {from}.",
				paramName: nameof(from),
				actualValue: from
			);
		}

		if (to is not null && !set.Contains(to.Value))
		{
			throw new ArgumentOutOfRangeException(
				message: $@"Invalid version specified: {to}.",
				paramName: nameof(to),
				actualValue: to
			);
		}

		var builder = new InternalBuilder(outer);
		var ds = new DataSource(set, builder, from, to);
		outer.DataSources.Add(ds);
		return builder;
	}

	// I used the RouteGroupBuilder (i.e., app.MapGroup(...)) implementation for reference:
	// https://github.com/dotnet/aspnetcore/blob/05b1bc9644f09de97521bcdb23818be131f77291/src/Http/Routing/src/RouteGroupBuilder.cs

	// A builder is made for an API version range, and collects data sources (introduced when routes
	// are added) and conventions.
	internal class InternalBuilder(IEndpointRouteBuilder outer)
		: IEndpointRouteBuilder
	{
		internal readonly IEndpointRouteBuilder outer = outer;
		internal readonly List<EndpointDataSource> dataSources = [];

		#region IEndpointRouteBuilder

		IServiceProvider IEndpointRouteBuilder.ServiceProvider => outer.ServiceProvider;
		ICollection<EndpointDataSource> IEndpointRouteBuilder.DataSources => dataSources;
		IApplicationBuilder IEndpointRouteBuilder.CreateApplicationBuilder() => outer.CreateApplicationBuilder();

		#endregion
	}

	// The data source yields endpoints collected by the builder, across all applicable API versions.
	internal class DataSource(
		RouteVersionSet<T> set,
		InternalBuilder builder,
		T from,
		T? to
	)
		: EndpointDataSource
	{
		// Used when versions are mapped via a route builder.
		// i.e., app.WithVersions(...).*
		public override IReadOnlyList<Endpoint> Endpoints
			=> GetEndpoints(groupCtx: null);

		// Used when versions are mapped via a route group builder.
		// i.e., app.MapGroup(...).WithVersions(...).*
		public override IReadOnlyList<Endpoint> GetGroupedEndpoints(RouteGroupContext groupCtx)
			=> GetEndpoints(groupCtx);

		private List<Endpoint> GetEndpoints(RouteGroupContext? groupCtx)
		{
			var list = new List<Endpoint>();

			foreach (var version in set)
			{
				var shouldMap =
					set.Compare(version, from) >= 0
					&& (to is null || set.Compare(version, to.Value) <= 0);

				if (shouldMap)
				{
					var meta = set.GetMetadata(version);

					var versionedGroupCtx = new RouteGroupContext
					{
						Prefix = RoutePatternFactory.Combine(
							groupCtx?.Prefix,
							RoutePatternFactory.Parse(set.GetSlug(version))
						),
						ApplicationServices = builder.outer.ServiceProvider,
						Conventions = [
							// Add convention to add route version metadata to the endpoint.
							(b) => b.Metadata.Add(meta),

							// Group conventions.
							.. groupCtx?.Conventions ?? [],

							// Add version-specific conventions.
							.. meta.conventions,
						],
						FinallyConventions = [
							// Group conventions.
							.. groupCtx?.FinallyConventions ?? [],

							// Add version-specific conventions.
							.. meta.finallyConventions,
						],
					};

					foreach (var ds in builder.dataSources)
					{
						list.AddRange(ds.GetGroupedEndpoints(versionedGroupCtx));
					}
				}
			}

			return list;
		}

		private CompositeEndpointDataSource? changeTokenDataSource;

		public override IChangeToken GetChangeToken()
		{
			return builder.dataSources.Count switch
			{
				0 => NullChangeToken.Singleton,
				1 => builder.dataSources[0].GetChangeToken(),

				_ => (changeTokenDataSource ??= new CompositeEndpointDataSource(builder.dataSources))
					.GetChangeToken(),
			};
		}
	}
}
