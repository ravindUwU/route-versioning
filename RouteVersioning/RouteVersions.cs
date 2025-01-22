namespace RouteVersioning;

using System;
using System.Collections;
using System.Collections.Generic;

public class RouteVersions<T> : IEnumerable<T>
	where T : struct, IComparable
{
	private readonly IDictionary<T, RouteVersionMetadata<T>> versions;
	private readonly Func<T, string> slug;

	internal RouteVersions(
		IDictionary<T, RouteVersionMetadata<T>> versions,
		Func<T, string> slug
	)
	{
		this.versions = versions;
		this.slug = slug;
	}

	public string GetSlug(T version)
	{
		return slug(version);
	}

	public RouteVersionMetadata<T> GetMetadata(T version)
	{
		return versions.TryGetValue(version, out var meta)
			? meta
			: throw new ArgumentException(
				message: $"Invalid version {version}.",
				paramName: nameof(version)
			);
	}

	public bool Contains(T version)
	{
		return versions.ContainsKey(version);
	}

	#region IEnumerator

	public IEnumerator<T> GetEnumerator() => versions.Keys.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	#endregion
}
