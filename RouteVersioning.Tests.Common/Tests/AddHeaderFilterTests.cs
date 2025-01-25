namespace RouteVersioning.Tests.Common.Tests;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Xunit;

public class AddHeaderFilterTests
{
	[Fact]
	public async Task Adds_header()
	{
		var headerName = $"X-{nameof(AddHeaderFilter)}-{Guid.NewGuid()}";
		var headerValue = $"{Guid.NewGuid()}";

		var app = await TestApp.StartAsync(configureApp: (app) =>
		{
			app.MapGet("a", () => { }).AddEndpointFilter(new AddHeaderFilter(headerName, headerValue));
		});

		Assert.Equal(headerValue, await app.GetSingleHeaderAsync("a", headerName));
	}
}
