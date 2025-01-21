namespace RouteVersioning;

using System;
using System.Collections.Generic;
using System.Linq;

public class RouteVersions<T>
	where T : struct, IComparable
{
	public RouteVersions(ISet<T> set, Func<T, string> prefix)
	{
		Set = set is IReadOnlySet<T> s ? s : set.ToHashSet();
		Prefix = prefix;
	}

	public IReadOnlySet<T> Set { get; }

	public Func<T, string> Prefix { get; }
}
