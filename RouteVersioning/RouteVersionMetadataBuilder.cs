namespace RouteVersioning;

using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;

/// <inheritdoc cref="RouteVersionMetadata{T}"/>
public class RouteVersionMetadataBuilder<T>
	: IEndpointConventionBuilder
	where T : struct
{
	private readonly T version;
	private readonly Dictionary<Type, List<object>> features = [];
	private readonly List<Action<EndpointBuilder>> conventions = [];
	private readonly List<Action<EndpointBuilder>> finallyConventions = [];

	internal RouteVersionMetadataBuilder(T version)
	{
		this.version = version;
	}

	void IEndpointConventionBuilder.Add(Action<EndpointBuilder> convention) => conventions.Add(convention);
	void IEndpointConventionBuilder.Finally(Action<EndpointBuilder> convention) => finallyConventions.Add(convention);

	/// <summary>
	/// Adds a feature associated with the API version. The feature can be subsequently retrieved
	/// via <see cref="RouteVersionMetadata{T}.GetFeatures"/>.
	/// </summary>
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

	public RouteVersionMetadata<T> Build(RouteVersionSet<T> set)
	{
		return new RouteVersionMetadata<T>(
			set,
			version,
			features: features.ToDictionary((kv) => kv.Key, (kv) => kv.Value.AsEnumerable()),
			conventions,
			finallyConventions
		);
	}
}
