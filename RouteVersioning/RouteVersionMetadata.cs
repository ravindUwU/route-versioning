namespace RouteVersioning;

using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Defines metadata associated with a specific API version.
/// </summary>
public class RouteVersionMetadata<T> : IRouteVersionMetadata<T>
	where T : struct
{
	private readonly IDictionary<Type, IEnumerable<object>> features;
	internal readonly IReadOnlyList<Action<EndpointBuilder>> conventions;
	internal readonly IReadOnlyList<Action<EndpointBuilder>> finallyConventions;

	internal RouteVersionMetadata(
		RouteVersionSet<T> set,
		T version,
		IDictionary<Type, IEnumerable<object>> features,
		IReadOnlyList<Action<EndpointBuilder>> conventions,
		IReadOnlyList<Action<EndpointBuilder>> finallyConventions
	)
	{
		Set = set;
		Version = version;
		this.features = features;
		this.conventions = conventions;
		this.finallyConventions = finallyConventions;
	}

	/// <summary>
	/// The associated version set.
	/// </summary>
	public RouteVersionSet<T> Set { get; }

	IRouteVersionSet<T> IRouteVersionMetadata<T>.Set => Set;

	/// <summary>
	/// The associated API version within of version set.
	/// </summary>
	public T Version { get; }

	/// <summary>
	/// Retrieve all features of the specified type, associated with the version via
	/// <see cref="RouteVersionMetadataBuilder{T}.WithFeature"/>.
	/// </summary>
	public IEnumerable<F> GetFeatures<F>()
		where F : notnull
	{
		return features.TryGetValue(typeof(F), out var f)
			? f.OfType<F>()
			: [];
	}
}
