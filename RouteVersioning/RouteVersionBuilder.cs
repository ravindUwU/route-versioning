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

	public RouteVersionBuilder<T> WithPrefix(string pattern)
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

		prefix = (v) => String.Format(pattern, v);
		return this;
	}

	public RouteVersions<T> Build()
	{
		return new RouteVersions<T>(versions, prefix);
	}
}
