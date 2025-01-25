namespace RouteVersioning.Tests;

using Microsoft.AspNetCore.Builder;
using RouteVersioning.Tests.Common;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

public class SunsetTests
{
	[Fact]
	public async Task Includes_headers_in_marked_versions()
	{
		var sunsetDate = new DateTime(2025, 6, 7, 8, 9, 10);
		var sunsetLink = $"https://www.example.com/{Guid.NewGuid()}";

		var set = new RouteVersionSetBuilder<int>()
			.Version(1, (v) => v.Sunset(sunsetDate))
			.Version(2, (v) => v.Sunset(sunsetDate, sunsetLink))
			.Version(3)
			.Build();

		var app = await TestApp.StartAsync(configureApp: (app) =>
		{
			var v = app.WithVersions(set);
			v.From(1).MapGet("a", () => { });
		});

		{
			var r = await app.GetAsync("v1/a");
			Assert.Equal(HttpStatusCode.OK, r.StatusCode);
			Assert.NotNull(app.GetSingleHeaderAsync(r, "Sunset"));
			Assert.Null(app.GetSingleHeaderAsync(r, "Link"));
		}

		{
			var r = await app.GetAsync("v2/a");
			Assert.Equal(HttpStatusCode.OK, r.StatusCode);
			Assert.NotNull(app.GetSingleHeaderAsync(r, "Sunset"));
			Assert.NotNull(app.GetSingleHeaderAsync(r, "Link"));
		}

		{
			var r = await app.GetAsync("v3/a");
			Assert.Equal(HttpStatusCode.OK, r.StatusCode);
			Assert.Null(app.GetSingleHeaderAsync(r, "Sunset"));
			Assert.Null(app.GetSingleHeaderAsync(r, "Link"));
		}
	}

	[Fact]
	public async Task Correctly_formats_sunset_header()
	{
		var sunsetDate = new DateTime(2025, 6, 7, 8, 9, 10);

		var set = new RouteVersionSetBuilder<int>()
			.Version(1, (v) => v.Sunset(sunsetDate))
			.Build();

		var app = await TestApp.StartAsync(configureApp: (app) =>
		{
			var v = app.WithVersions(set);
			v.From(1).MapGet("a", () => { });
		});

		var header = await app.GetSingleHeaderAsync("v1/a", "Sunset");
		Assert.NotNull(header);

		var headerDate = DateTime.ParseExact(header, "r", null);
		Assert.Equal(sunsetDate, headerDate);
	}

	[Fact]
	public async Task Correctly_formats_link_header()
	{
		var sunsetDate = new DateTime(2025, 6, 7, 8, 9, 10);
		var sunsetLink = $"https://www.example.com/{Guid.NewGuid()}";
		var sunsetMediaType = "text/html";

		var set = new RouteVersionSetBuilder<int>()
			.Version(1, (v) => v.Sunset(sunsetDate, sunsetLink))
			.Version(2, (v) => v.Sunset(sunsetDate, sunsetLink, sunsetMediaType))
			.Build();

		var app = await TestApp.StartAsync(configureApp: (app) =>
		{
			var v = app.WithVersions(set);
			v.From(1).MapGet("a", () => { });
		});

		{
			var header = await app.GetSingleHeaderAsync("v1/a", "Link");
			Assert.Equal($@"<{sunsetLink}>; rel=""sunset""", header);
		}

		{
			var header = await app.GetSingleHeaderAsync("v2/a", "Link");
			Assert.Equal($@"<{sunsetLink}>; rel=""sunset""; type=""{sunsetMediaType}""", header);
		}
	}
}
