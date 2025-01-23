namespace RouteVersioning;

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

	internal RouteVersionMetadata(T version, IDictionary<Type, IEnumerable<object>> features)
	{
		Version = version;
		this.features = features;
	}

	/// <summary>
	/// The associated API version.
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
