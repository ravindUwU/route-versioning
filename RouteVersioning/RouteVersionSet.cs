namespace RouteVersioning;

using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Defines all available API versions.
/// </summary>
public class RouteVersionSet<T> : IEnumerable<T>, IRouteVersionSet<T>
	where T : struct
{
	private readonly IReadOnlyDictionary<T, RouteVersionMetadata<T>> versions;
	private readonly Func<T, string> slug;
	private readonly IComparer<T> comparer;

	internal RouteVersionSet(
		string? name,
		IReadOnlyDictionary<T, RouteVersionMetadata<T>> versions,
		Func<T, string> slug,
		IComparer<T> comparer
	)
	{
		Name = name;
		this.versions = versions;
		this.slug = slug;
		this.comparer = comparer;
	}

	public string? Name { get; }

	/// <summary>
	/// Retrieve the slug for the specified version.
	/// </summary>
	public string GetSlug(T version)
	{
		return slug(version);
	}

	/// <summary>
	/// Retrieve the slug that includes the name of the version set (<see cref="Name"/>) if defined,
	/// for the specified version.
	/// </summary>
	public string GetNamedSlug(T version)
	{
		return Name is null ? slug(version) : $"{Name}-{slug(version)}";
	}

	/// <summary>
	/// Retrieve metadata of the specified version.
	/// </summary>
	/// <exception cref="ArgumentException">
	/// Thrown if specified version hasn't been registered with <see cref="RouteVersionSetBuilder{T}.Version"/>.
	/// </exception>
	public RouteVersionMetadata<T> GetMetadata(T version)
	{
		return versions.TryGetValue(version, out var meta)
			? meta
			: throw new ArgumentException(
				message: $"Invalid version {version}.",
				paramName: nameof(version)
			);
	}

	/// <summary>
	/// Returns whether the specified version has been registered.
	/// </summary>
	public bool Contains(T version)
	{
		return versions.ContainsKey(version);
	}

	/// <summary>
	/// Compares 2 versions, returning <c>-1</c> if the first precedes the second, <c>0</c> if
	/// they're equal or <c>1</c> if the first succeeds the second (<see cref="IComparer"/> behaviour).
	/// </summary>
	public int Compare(T first, T second)
	{
		return comparer.Compare(first, second);
	}

	#region IEnumerator

	public IEnumerator<T> GetEnumerator() => versions.Keys.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	#endregion
}
