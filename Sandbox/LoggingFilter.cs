namespace Sandbox;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

internal class LoggingFilter(string name) : IEndpointFilter
{
	private readonly string name = name;

	public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
	{
		var req = ctx.HttpContext.Request;
		var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<LoggingFilter>>();
		var label = $"{req.Method} {req.GetEncodedPathAndQuery()}";
		try
		{
			logger.LogInformation("Started {label} - {name}", label, name);
			return await next(ctx);
		}
		finally
		{
			logger.LogInformation("Finished {label} - {name}", label, name);
		}
	}
}
