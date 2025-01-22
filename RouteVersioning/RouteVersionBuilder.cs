namespace RouteVersioning;

using System;
using System.Collections.Generic;
using System.Linq;

public class RouteVersionBuilder<T>
	where T : struct, IComparable
{
	private readonly Dictionary<T, RouteVersionMetadataBuilder<T>> versions = [];
	private Func<T, string> slug = (v) => $"v{v}";

	public RouteVersionBuilder<T> WithVersion(
		T version,
		Action<RouteVersionMetadataBuilder<T>>? configure = null
	)
	{
		if (versions.ContainsKey(version))
		{
			throw new InvalidOperationException($"The version {version} has already been added.");
		}

		var builder = new RouteVersionMetadataBuilder<T>(version);
		versions[version] = builder;
		configure?.Invoke(builder);
		return this;
	}

	public RouteVersionBuilder<T> WithSlug(Func<T, string> slug)
	{
		this.slug = slug;
		return this;
	}

	public RouteVersionBuilder<T> WithSlug(string pattern)
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

	public RouteVersions<T> Build()
	{
		return new RouteVersions<T>(
			versions.ToDictionary((kv) => kv.Key, (kv) => kv.Value.Build()),
			slug
		);
	}
}
