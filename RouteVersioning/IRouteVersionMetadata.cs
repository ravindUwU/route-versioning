namespace RouteVersioning;

using System.Collections;
using System.Collections.Generic;

public interface IRouteVersionMetadata
{
	object Version { get; }

	IComparer VersionComparer { get; }

	IEnumerable<F> GetFeatures<F>() where F : notnull;

	bool IsVersion(object? v)
	{
		return VersionComparer.Compare(Version, v) is 0;
	}
}

public interface IRouteVersionMetadata<T> : IRouteVersionMetadata
	where T : struct
{
	new T Version { get; }

	object IRouteVersionMetadata.Version => Version;

	IComparer IRouteVersionMetadata.VersionComparer => Comparer<T>.Default;
}
