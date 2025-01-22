namespace RouteVersioning;

using System;
using System.Collections.Generic;
using System.Linq;

public class RouteVersionMetadata<T> : IRouteVersionMetadata<T>
	where T : struct, IComparable
{
	private readonly IDictionary<Type, IEnumerable<object>> features;

	internal RouteVersionMetadata(T version, IDictionary<Type, IEnumerable<object>> features)
	{
		Version = version;
		this.features = features;
	}

	public T Version { get; }

	public IEnumerable<F> GetFeatures<F>()
		where F : notnull
	{
		return features.TryGetValue(typeof(F), out var f)
			? f.OfType<F>()
			: [];
	}
}
