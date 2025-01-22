namespace RouteVersioning;

using System;
using System.Collections;
using System.Collections.Generic;

public class RouteVersions<T> : IEnumerable<T>
	where T : struct
{
	private readonly IDictionary<T, RouteVersionMetadata<T>> versions;
	private readonly Func<T, string> slug;
	private readonly IComparer<T> comparer;

	internal RouteVersions(
		IDictionary<T, RouteVersionMetadata<T>> versions,
		Func<T, string> slug,
		IComparer<T> comparer
	)
	{
		this.versions = versions;
		this.slug = slug;
		this.comparer = comparer;
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

	public int Compare(T left, T right)
	{
		return comparer.Compare(left, right);
	}

	#region IEnumerator

	public IEnumerator<T> GetEnumerator() => versions.Keys.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	#endregion
}
