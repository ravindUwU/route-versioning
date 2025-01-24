namespace RouteVersioning;

using System;
using System.Collections.Generic;

/// <inheritdoc cref="RouteVersionSet{T}"/>
public class RouteVersionSetBuilder<T>(string? name = null)
	where T : struct
{
	private readonly Dictionary<T, RouteVersionMetadataBuilder<T>> versions = [];

	private readonly string? name = name;
	private Func<T, string> slug = (v) => $"v{v}";
	private IComparer<T> comparer = Comparer<T>.Default;

	/// <summary>
	/// Define an API version.
	/// </summary>
	/// <returns>
	/// Builder that allows configuring behaviours across all routes of the API version being defined.
	/// </returns>
	/// <exception cref="InvalidOperationException">
	/// Thrown if the specified API version is already been defined. An API version may be defined
	/// only once.
	/// </exception>
	public RouteVersionSetBuilder<T> Version(
		T version,
		Action<RouteVersionMetadataBuilder<T>>? configure = null
	)
	{
		if (versions.ContainsKey(version))
		{
			throw new InvalidOperationException($"The version {version} has already been defined.");
		}

		var builder = new RouteVersionMetadataBuilder<T>(version);
		versions[version] = builder;
		configure?.Invoke(builder);
		return this;
	}

	/// <summary>
	/// Sets a function that, given an API version, computes the corresponding slug.
	/// </summary>
	/// <remarks>
	/// Defaults to the format <c>v{0}</c> where <c>{0}</c> is the default string representation of
	/// the version.
	/// </remarks>
	public RouteVersionSetBuilder<T> WithSlug(Func<T, string> slug)
	{
		this.slug = slug;
		return this;
	}

	/// <summary>
	/// Sets the pattern according to which the slug is computed. The pattern must include a single
	/// placeholder <c>{0}</c>, which is substituted with the default string representation of the
	/// version, to form the slug.
	/// </summary>
	/// <remarks>
	/// Defaults to the format <c>v{0}</c> where <c>{0}</c> is the default string representation of
	/// the version.
	/// </remarks>
	/// <exception cref="ArgumentException">
	/// Thrown if the pattern doesn't include the placeholder, or includes more than 1 placeholder.
	/// </exception>
	public RouteVersionSetBuilder<T> WithSlug(string pattern)
	{
		var firstIdx = pattern.IndexOf("{0}");
		if (firstIdx is -1)
		{
			throw new ArgumentException(
				message: @"The specified pattern must include the version placeholder ""{0}"".",
				paramName: nameof(pattern)
			);
		}

		if (firstIdx != pattern.LastIndexOf("{0}"))
		{
			throw new ArgumentException(
				message: @"The specified pattern must include a single version placeholder ""{0}"".",
				paramName: nameof(pattern)
			);
		}

		slug = (v) => String.Format(pattern, v);
		return this;
	}

	/// <summary>
	/// Sets the <see cref="IComparer{T}"/> which is used to compare versions to one another.
	/// </summary>
	/// <remarks>
	/// Defaults to <see cref="Comparer{T}.Default"/>.
	/// </remarks>
	public RouteVersionSetBuilder<T> WithComparer(IComparer<T> comparer)
	{
		this.comparer = comparer;
		return this;
	}

	public RouteVersionSet<T> Build()
	{
		var versions = new Dictionary<T, RouteVersionMetadata<T>>();
		var set = new RouteVersionSet<T>(name, versions, slug, comparer);

		foreach (var (key, value) in this.versions)
		{
			versions[key] = value.Build(set);
		}

		return set;
	}
}
