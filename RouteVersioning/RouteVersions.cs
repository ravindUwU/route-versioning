namespace RouteVersioning;

using System;
using System.Collections;
using System.Collections.Generic;

public class RouteVersions<T> : IEnumerable<T>
	where T : struct, IComparable
{
	private readonly IDictionary<T, RouteVersionMetadata<T>> versions;

	internal RouteVersions(
		IDictionary<T, RouteVersionMetadata<T>> versions,
		Func<T, string> prefix
	)
	{
		this.versions = versions;
		Prefix = prefix;
	}

	public Func<T, string> Prefix { get; }

	public bool Contains(T version)
	{
		return versions.ContainsKey(version);
	}

	#region IEnumerator

	public IEnumerator<T> GetEnumerator() => versions.Keys.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	#endregion
}
