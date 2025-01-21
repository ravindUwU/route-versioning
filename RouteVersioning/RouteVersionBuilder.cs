namespace RouteVersioning;

using System;
using System.Collections.Generic;

public class RouteVersionBuilder<T>
	where T : struct, IComparable
{
	public RouteVersionBuilder(IReadOnlySet<T> versions)
	{
		Versions = versions;
	}

	public IReadOnlySet<T> Versions { get; }
}
