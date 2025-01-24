namespace RouteVersioning;

using System.Collections;
using System.Collections.Generic;

public interface IRouteVersionMetadata
{
	IRouteVersionSet Set { get; }

	object Version { get; }

	IComparer Comparer { get; }

	IEnumerable<F> GetFeatures<F>() where F : notnull;

	bool IsVersion(object? v)
	{
		return Comparer.Compare(Version, v) is 0;
	}
}

public interface IRouteVersionMetadata<T> : IRouteVersionMetadata
	where T : struct
{
	new IRouteVersionSet<T> Set { get; }

	IRouteVersionSet IRouteVersionMetadata.Set => Set;

	new T Version { get; }

	object IRouteVersionMetadata.Version => Version;

	IComparer IRouteVersionMetadata.Comparer => Comparer<T>.Default;
}
