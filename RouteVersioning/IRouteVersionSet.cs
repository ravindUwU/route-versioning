namespace RouteVersioning;

public interface IRouteVersionSet
{
}

public interface IRouteVersionSet<T> : IRouteVersionSet
	where T : struct
{
}
