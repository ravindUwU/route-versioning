namespace RouteVersioning.Tests;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using RouteVersioning.Tests.Common;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

public class RouteMappingTests
{
	public class MappingTests
	{
		[Fact]
		public void Throws_when_mapping_to_undefined_version()
		{
			var set = new RouteVersionSetBuilder<int>().Version(1).Version(3).Build();

			TestApp.Make(configureApp: (app) =>
			{
				var v = app.WithVersions(set);

				{
					var ex = Assert.Throws<ArgumentOutOfRangeException>(() => v.From(2));
					Assert.Equal("from", ex.ParamName);
					Assert.Equal(2, ex.ActualValue);
				}

				{
					var ex = Assert.Throws<ArgumentOutOfRangeException>(() => v.Between(2, 3));
					Assert.Equal("from", ex.ParamName);
					Assert.Equal(2, ex.ActualValue);
				}

				{
					var ex = Assert.Throws<ArgumentOutOfRangeException>(() => v.Between(1, 2));
					Assert.Equal("to", ex.ParamName);
					Assert.Equal(2, ex.ActualValue);
				}
			});
		}

		[Fact]
		public async Task Includes_route_version_metadata()
		{
			var set = new RouteVersionSetBuilder<int>().Version(1).Build();
			var app = await TestApp.StartAsync(configureApp: (app) =>
			{
				var v = app.WithVersions(set);
				v.From(1).MapGet("a", () => { });
			});

			var e = app.GetRouteEndpoint("GET", "v1/a");
			Assert.NotNull(e);

			var m = e.Metadata.OfType<IRouteVersionMetadata>().SingleOrDefault();
			Assert.NotNull(m);
			Assert.Equal(1, m.Version);
			Assert.Equal(set, m.Set);
		}

		[Fact]
		public async Task Maps_from_version_onward()
		{
			var set = new RouteVersionSetBuilder<int>().Version(1).Version(2).Build();
			var ids = new NamedIds();
			var app = await TestApp.StartAsync(configureApp: (app) =>
			{
				var v = app.WithVersions(set);
				v.From(1).MapGet("a", () => ids["a"]);
				v.From(2).MapGet("b", () => ids["b"]);
			});

			Assert.Equal(ids["a"], await app.GetStringAsync("v1/a"));
			Assert.Equal(ids["a"], await app.GetStringAsync("v2/a"));

			Assert.Equal(HttpStatusCode.NotFound, await app.GetStatusCodeAsync("v1/b"));
			Assert.Equal(ids["b"], await app.GetStringAsync("v2/b"));
		}

		[Fact]
		public async Task Maps_between_inclusive_versions()
		{
			var set = new RouteVersionSetBuilder<int>().Version(1).Version(2).Version(3).Build();
			var ids = new NamedIds();
			var app = await TestApp.StartAsync(configureApp: (app) =>
			{
				var v = app.WithVersions(set);
				v.Between(1, 2).MapGet("a", () => ids["a"]);
				v.Between(2, 3).MapGet("b", () => ids["b"]);
				v.Between(1, 3).MapGet("c", () => ids["c"]);
			});

			Assert.Equal(ids["a"], await app.GetStringAsync("v1/a"));
			Assert.Equal(ids["a"], await app.GetStringAsync("v2/a"));
			Assert.Equal(HttpStatusCode.NotFound, await app.GetStatusCodeAsync("v3/a"));

			Assert.Equal(HttpStatusCode.NotFound, await app.GetStatusCodeAsync("v1/b"));
			Assert.Equal(ids["b"], await app.GetStringAsync("v2/b"));
			Assert.Equal(ids["b"], await app.GetStringAsync("v3/b"));

			Assert.Equal(ids["c"], await app.GetStringAsync("v1/c"));
			Assert.Equal(ids["c"], await app.GetStringAsync("v2/c"));
			Assert.Equal(ids["c"], await app.GetStringAsync("v3/c"));
		}

		[Fact]
		public async Task Maps_versioned_groups()
		{
			var set = new RouteVersionSetBuilder<int>().Version(1).Version(2).Version(3).Build();
			var ids = new NamedIds();
			var app = await TestApp.StartAsync(configureApp: (app) =>
			{
				var v = app.WithVersions(set);

				var g1 = v.From(1).MapGroup("g1");
				g1.MapGet("a", () => ids["g1a"]);
				g1.MapGet("b", () => ids["g1b"]);

				var g2 = v.From(2).MapGroup("g2");
				g2.MapGet("a", () => ids["g2a"]);
				g2.MapGet("b", () => ids["g2b"]);

				var g3 = v.Between(1, 2).MapGroup("g3");
				g3.MapGet("a", () => ids["g3a"]);
				g3.MapGet("b", () => ids["g3b"]);
			});

			Assert.Equal(ids["g1a"], await app.GetStringAsync("v1/g1/a"));
			Assert.Equal(ids["g1b"], await app.GetStringAsync("v1/g1/b"));
			Assert.Equal(ids["g1a"], await app.GetStringAsync("v2/g1/a"));
			Assert.Equal(ids["g1b"], await app.GetStringAsync("v2/g1/b"));
			Assert.Equal(ids["g1a"], await app.GetStringAsync("v3/g1/a"));
			Assert.Equal(ids["g1b"], await app.GetStringAsync("v3/g1/b"));

			Assert.Equal(HttpStatusCode.NotFound, await app.GetStatusCodeAsync("v1/g2/a"));
			Assert.Equal(HttpStatusCode.NotFound, await app.GetStatusCodeAsync("v1/g2/b"));
			Assert.Equal(ids["g2a"], await app.GetStringAsync("v2/g2/a"));
			Assert.Equal(ids["g2b"], await app.GetStringAsync("v2/g2/b"));
			Assert.Equal(ids["g2a"], await app.GetStringAsync("v3/g2/a"));
			Assert.Equal(ids["g2b"], await app.GetStringAsync("v3/g2/b"));

			Assert.Equal(ids["g3a"], await app.GetStringAsync("v1/g3/a"));
			Assert.Equal(ids["g3b"], await app.GetStringAsync("v1/g3/b"));
			Assert.Equal(ids["g3a"], await app.GetStringAsync("v2/g3/a"));
			Assert.Equal(ids["g3b"], await app.GetStringAsync("v2/g3/b"));
			Assert.Equal(HttpStatusCode.NotFound, await app.GetStatusCodeAsync("v3/g3/a"));
			Assert.Equal(HttpStatusCode.NotFound, await app.GetStatusCodeAsync("v3/g3/b"));
		}
	}

	public class ConventionTests
	{
		[Fact]
		public async Task Adds_convention_to_all_versions_of_endpoint()
		{
			var set = new RouteVersionSetBuilder<int>().Version(1).Version(2).Build();
			var ids = new NamedIds();
			var app = await TestApp.StartAsync(configureApp: (app) =>
			{
				var v = app.WithVersions(set);

				v.From(1).MapGet("a", () => { }).AddEndpointFilter(new AddHeaderFilter("X-IdA", ids["a"]));
				v.From(1).MapGet("b", () => { });
			});

			Assert.Equal(ids["a"], await app.GetSingleHeaderAsync("v1/a", "X-IdA"));
			Assert.Equal(ids["a"], await app.GetSingleHeaderAsync("v2/a", "X-IdA"));

			Assert.Null(await app.GetSingleHeaderAsync("v1/b", "X-IdA"));
			Assert.Null(await app.GetSingleHeaderAsync("v1/b", "X-IdA"));
		}

		[Fact]
		public async Task Adds_convention_to_all_endpoints_of_version()
		{
			var ids = new NamedIds();

			var set = new RouteVersionSetBuilder<int>()
				.Version(1, (v) => v.AddEndpointFilter(new AddHeaderFilter("X-Id1", ids["1"])))
				.Version(2)
				.Build();

			var app = await TestApp.StartAsync(configureApp: (app) =>
			{
				var v = app.WithVersions(set);

				v.From(1).MapGet("a", () => { });
				v.From(1).MapGet("b", () => { });
			});

			Assert.Equal(ids["1"], await app.GetSingleHeaderAsync("v1/a", "X-Id1"));
			Assert.Equal(ids["1"], await app.GetSingleHeaderAsync("v1/b", "X-Id1"));

			Assert.Null(await app.GetSingleHeaderAsync("v2/a", "X-Id1"));
			Assert.Null(await app.GetSingleHeaderAsync("v2/b", "X-Id1"));
		}
	}

	public class SlugTests
	{
		[Fact]
		public async Task Uses_custom_slug_function()
		{
			var slugId = $"{Guid.NewGuid()}";
			var set = new RouteVersionSetBuilder<int>()
				.Version(1)
				.Version(2)
				.WithSlug((version) => $"version{version}-{slugId}")
				.Build();

			var ids = new NamedIds();
			var app = await TestApp.StartAsync(configureApp: (app) =>
			{
				var v = app.WithVersions(set);
				v.From(1).MapGet("a", () => ids["a"]);
			});

			Assert.Equal(ids["a"], await app.GetStringAsync($"version1-{slugId}/a"));
			Assert.Equal(ids["a"], await app.GetStringAsync($"version2-{slugId}/a"));
		}

		[Fact]
		public async Task Uses_custom_slug_pattern()
		{
			var slugId = $"{Guid.NewGuid()}";
			var set = new RouteVersionSetBuilder<int>()
				.Version(1)
				.Version(2)
				.WithSlug($"version{{0}}-{slugId}")
				.Build();

			var ids = new NamedIds();
			var app = await TestApp.StartAsync(configureApp: (app) =>
			{
				var v = app.WithVersions(set);
				v.From(1).MapGet("a", () => ids["a"]);
			});

			Assert.Equal(ids["a"], await app.GetStringAsync($"version1-{slugId}/a"));
			Assert.Equal(ids["a"], await app.GetStringAsync($"version2-{slugId}/a"));
		}
	}
}
