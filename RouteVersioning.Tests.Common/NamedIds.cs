namespace RouteVersioning.Tests.Common;

using System;
using System.Collections.Generic;

public class NamedIds
{
	private readonly Dictionary<string, string> dictionary = [];

	public string this[string name]
	{
		get
		{
			return dictionary.TryGetValue(name, out var id)
				? id
				: (dictionary[name] = $"{name}-{Guid.NewGuid()}");
		}
	}
}
