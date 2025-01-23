namespace RouteVersioning;

using System;
using System.Collections.Generic;
using System.Linq;

public class RouteVersionMetadataBuilder<T>
	where T : struct
{
	private readonly T version;
	private readonly Dictionary<Type, List<object>> features = [];

	internal RouteVersionMetadataBuilder(T version)
	{
		this.version = version;
	}

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
