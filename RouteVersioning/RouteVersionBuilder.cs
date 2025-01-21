namespace RouteVersioning;

using System;
using System.Collections.Generic;
using System.Linq;

public class RouteVersionBuilder<T>
	where T : struct, IComparable
{
	private ISet<T> versions;
	private Func<T, string> prefix = (v) => $"v{v}";

	public RouteVersionBuilder(params T[] versions)
	{
		this.versions = versions.ToHashSet();
	}

	public RouteVersionBuilder<T> WithPrefix(Func<T, string> prefix)
	{
		this.prefix = prefix;
		return this;
	}

	public RouteVersions<T> Build()
	{
		return new RouteVersions<T>(versions, prefix);
	}
}
