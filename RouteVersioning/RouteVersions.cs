namespace RouteVersioning;

using System;
using System.Collections.Generic;
using System.Linq;

public class RouteVersions<T>
	where T : struct, IComparable
{
	public RouteVersions(ISet<T> set)
	{
		Set = set is IReadOnlySet<T> s ? s : set.ToHashSet();
	}

	public IReadOnlySet<T> Set { get; }
}
