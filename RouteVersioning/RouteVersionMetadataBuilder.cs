namespace RouteVersioning;

using System;
using System.Collections.Generic;
using System.Linq;

public class RouteVersionMetadataBuilder<T>(T version)
	where T : struct, IComparable
{
	private readonly Dictionary<Type, List<object>> features = [];

	public RouteVersionMetadataBuilder<T> WithFeature<F>(F feature)
		where F : notnull
	{
		var key = typeof(F);
		if (features.TryGetValue(key, out var list))
		{
			list.Add(feature);
		}
		else
		{
			features[key] = [feature];
		}
		return this;
	}

	public RouteVersionMetadata<T> Build()
	{
		return new RouteVersionMetadata<T>(
			version,
			features: features.ToDictionary((kv) => kv.Key, (kv) => kv.Value.AsEnumerable())
		);
	}
}
