namespace RouteVersioning.Tests.Common;

using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public class AddHeaderFilter(string name, string value) : IEndpointFilter
{
	public string Name { get; } = name;
	public string Value { get; } = value;

	public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
	{
		var result = await next(ctx);
		ctx.HttpContext.Response.Headers[Name] = Value;
		return result;
	}
}
