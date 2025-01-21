namespace RouteVersioning;

using System.Collections;

public class RouteVersionEndpointMetadata
{
	public required object Version { get; init; }
	public required IEqualityComparer VersionComparer { get; init; }
}
