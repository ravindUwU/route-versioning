namespace RouteVersioning;

using System;
using System.Collections.Generic;

public class RouteVersionBuilder<T, TContext>
	where T : struct, IComparable
{
	private ISet<T> versions;

	public RouteVersionBuilder(ISet<T> versions, TContext context)
	{
		this.versions = versions;
		Context = context;
	}

	public TContext Context { get; }

	private Func<T, string> prefix = (v) => $"v{v}";

	public RouteVersionBuilder<T, TContext> WithPrefix(Func<T, string> prefix)
	{
		this.prefix = prefix;
		return this;
	}

	public RouteVersions<T> Build()
	{
		return new RouteVersions<T>(versions, prefix);
	}
}
