namespace RouteVersioning.OpenApi.Tests;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using RouteVersioning.Tests.Common;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class VersionedOpenApiTests
{
	public class PathTests
	{
		[Fact]
		public async Task Default_doc_includes_versioned_and_unversioned_endpoints()
		{
			var set = new RouteVersionSetBuilder<int>().Version(1).Version(2).Build();

			var app = await OpenApiTestApp.StartAsync(
				configureServices: (services, _) =>
				{
					services.AddOpenApi("current");
				},
				configureApp: (app) =>
				{
					app.MapGet("a", () => { });

					var v = app.WithVersions(set);
					v.From(1).MapGet("b", () => { });
					v.From(2).MapGet("c", () => { });

					app.MapOpenApi();
				}
			);

			var doc = await app.GetOpenApiDocumentAsync("openapi/current.json");
			Assert.True(doc.Paths.ContainsKey("/a"));
			Assert.True(doc.Paths.ContainsKey("/v1/b"));
			Assert.True(doc.Paths.ContainsKey("/v2/b"));
			Assert.True(doc.Paths.ContainsKey("/v2/c"));
		}

		[Fact]
		public async Task Default_doc_excludes_versioned_endpoints_if_configured()
		{
			var set = new RouteVersionSetBuilder<int>().Version(1).Build();

			var app = await OpenApiTestApp.StartAsync(
				configureServices: (services, _) =>
				{
					services.AddOpenApi("current", (o) => o.ExcludeVersionedOperations());
				},
				configureApp: (app) =>
				{
					app.MapGet("a", () => { });

					var v = app.WithVersions(set);
					v.From(1).MapGet("b", () => { });

					app.MapOpenApi();
				}
			);

			var doc = await app.GetOpenApiDocumentAsync("openapi/current.json");
			Assert.True(doc.Paths.ContainsKey("/a"));
			Assert.False(doc.Paths.ContainsKey("/v1/b"));
		}

		[Fact]
		public async Task Versioned_doc_includes_endpoints_of_version()
		{
			var set = new RouteVersionSetBuilder<int>().Version(1).Version(2).Build();

			var app = await OpenApiTestApp.StartAsync(
				configureServices: (services, _) =>
				{
					services.AddVersionedOpenApi(set);
				},
				configureApp: (app) =>
				{
					var v = app.WithVersions(set);
					v.From(1).MapGet("a", () => { });
					v.From(2).MapGet("b", () => { });

					app.MapOpenApi();
				}
			);

			{
				var doc = await app.GetOpenApiDocumentAsync("openapi/v1.json");
				Assert.True(doc.Paths.ContainsKey("/v1/a"));
				Assert.Single(doc.Paths);
			}

			{
				var doc = await app.GetOpenApiDocumentAsync("openapi/v2.json");
				Assert.True(doc.Paths.ContainsKey("/v2/a"));
				Assert.True(doc.Paths.ContainsKey("/v2/b"));
				Assert.Equal(2, doc.Paths.Count);
			}
		}

		[Fact]
		public async Task Versioned_doc_includes_unversioned_endpoints_by_default()
		{
			var set = new RouteVersionSetBuilder<int>().Version(1).Build();

			var app = await OpenApiTestApp.StartAsync(
				configureServices: (services, _) =>
				{
					services.AddVersionedOpenApi(set);
				},
				configureApp: (app) =>
				{
					app.MapGet("a", () => { });

					var v = app.WithVersions(set);
					v.From(1).MapGet("b", () => { });

					app.MapOpenApi();
				}
			);

			var doc = await app.GetOpenApiDocumentAsync("openapi/v1.json");
			Assert.True(doc.Paths.ContainsKey("/a"));
			Assert.True(doc.Paths.ContainsKey("/v1/b"));
		}

		[Fact]
		public async Task Versioned_doc_excludes_unversioned_endpoints_if_configured()
		{
			var set = new RouteVersionSetBuilder<int>().Version(1).Build();

			var app = await OpenApiTestApp.StartAsync(
				configureServices: (services, _) =>
				{
					services.AddVersionedOpenApi(set, includeUnversionedEndpoints: false);
				},
				configureApp: (app) =>
				{
					app.MapGet("a", () => { });

					var v = app.WithVersions(set);
					v.From(1).MapGet("b", () => { });

					app.MapOpenApi();
				}
			);

			var doc = await app.GetOpenApiDocumentAsync("openapi/v1.json");
			Assert.False(doc.Paths.ContainsKey("/a"));
			Assert.True(doc.Paths.ContainsKey("/v1/b"));
			Assert.Single(doc.Paths);
		}
	}

	public class NamedVersionSetTests
	{
		[Fact]
		public async Task Default_doc_includes_versioned_endpoints_of_all_named_sets()
		{
			var set1 = new RouteVersionSetBuilder<int>("set1").Version(1).Version(2).Build();
			var set2 = new RouteVersionSetBuilder<int>("set2").Version(1).Version(2).Build();

			var app = await OpenApiTestApp.StartAsync(
				configureServices: (services, _) =>
				{
					services.AddOpenApi("current");
					services.AddVersionedOpenApi(set1);
					services.AddVersionedOpenApi(set2);
				},
				configureApp: (app) =>
				{
					var s1 = app.MapGroup("set1").WithVersions(set1);
					s1.From(1).MapGet("a", () => { });

					var s2 = app.MapGroup("set2").WithVersions(set2);
					s2.From(1).MapGet("a", () => { });

					app.MapOpenApi();
				}
			);

			var doc = await app.GetOpenApiDocumentAsync("openapi/current.json");
			Assert.True(doc.Paths.ContainsKey("/set1/v1/a"));
			Assert.True(doc.Paths.ContainsKey("/set1/v2/a"));
			Assert.True(doc.Paths.ContainsKey("/set2/v1/a"));
			Assert.True(doc.Paths.ContainsKey("/set2/v2/a"));
		}

		[Fact]
		public async Task Versioned_doc_of_named_set_mapped_with_named_slug()
		{
			var set1 = new RouteVersionSetBuilder<int>("set1").Version(1).Version(2).Build();
			var set2 = new RouteVersionSetBuilder<int>("set2").Version(1).Version(2).Build();

			var app = await OpenApiTestApp.StartAsync(
				configureServices: (services, _) =>
				{
					services.AddVersionedOpenApi(set1);
					services.AddVersionedOpenApi(set2);
				},
				configureApp: (app) =>
				{
					var s1 = app.MapGroup("set1").WithVersions(set1);
					s1.From(1).MapGet("a", () => { });

					var s2 = app.MapGroup("set2").WithVersions(set2);
					s2.From(1).MapGet("a", () => { });

					app.MapOpenApi();
				}
			);

			{
				var doc = await app.GetOpenApiDocumentAsync("openapi/set1-v1.json");
				Assert.Single(doc.Paths);
				Assert.True(doc.Paths.ContainsKey("/set1/v1/a"));
			}

			{
				var doc = await app.GetOpenApiDocumentAsync("openapi/set1-v2.json");
				Assert.Single(doc.Paths);
				Assert.True(doc.Paths.ContainsKey("/set1/v2/a"));
			}

			{
				var doc = await app.GetOpenApiDocumentAsync("openapi/set2-v1.json");
				Assert.Single(doc.Paths);
				Assert.True(doc.Paths.ContainsKey("/set2/v1/a"));
			}

			{
				var doc = await app.GetOpenApiDocumentAsync("openapi/set2-v2.json");
				Assert.Single(doc.Paths);
				Assert.True(doc.Paths.ContainsKey("/set2/v2/a"));
			}
		}
	}

	public class ConfigurationTests
	{
		[Fact]
		public async Task Allows_configuring_all_versions()
		{
			var set = new RouteVersionSetBuilder<int>().Version(1).Version(2).Build();
			var transformer = new Transformer();

			var app = await OpenApiTestApp.StartAsync(
				configureServices: (services, _) =>
				{
					services.AddVersionedOpenApi(set, (o) => o.AddDocumentTransformer(transformer));
				},
				configureApp: (app) =>
				{
					app.MapOpenApi();
				}
			);

			Assert.Equal(0, transformer.Count);

			await app.GetOpenApiDocumentAsync("openapi/v1.json");

			Assert.Equal(1, transformer.Count);

			await app.GetOpenApiDocumentAsync("openapi/v2.json");

			Assert.Equal(2, transformer.Count);
		}

		[Fact]
		public async Task Allows_configuring_specific_versions()
		{
			var transformer1 = new Transformer();
			var transformer2 = new Transformer();

			var set = new RouteVersionSetBuilder<int>()
				.Version(1, (v) => v
					.ConfigureOpenApiOptions((o) => o.AddDocumentTransformer(transformer1))
				)
				.Version(2, (v) => v
					.ConfigureOpenApiOptions((o) => o.AddDocumentTransformer(transformer2))
				)
				.Version(3)
				.Build();

			var app = await OpenApiTestApp.StartAsync(
				configureServices: (services, _) =>
				{
					services.AddVersionedOpenApi(set);
				},
				configureApp: (app) =>
				{
					app.MapOpenApi();
				}
			);

			Assert.Equal(0, transformer1.Count);
			Assert.Equal(0, transformer2.Count);

			await app.GetOpenApiDocumentAsync("openapi/v1.json");

			Assert.Equal(1, transformer1.Count);
			Assert.Equal(0, transformer2.Count);

			await app.GetOpenApiDocumentAsync("openapi/v2.json");

			Assert.Equal(1, transformer1.Count);
			Assert.Equal(1, transformer2.Count);

			await app.GetOpenApiDocumentAsync("openapi/v3.json");

			Assert.Equal(1, transformer1.Count);
			Assert.Equal(1, transformer2.Count);
		}

		[Fact]
		public async Task Allows_configuring_info()
		{
			var ids = new NamedIds();

			var set = new RouteVersionSetBuilder<int>()
				.Version(1, (v) => v
					.ConfigureOpenApiInfo((i) => i.Description = ids["1"])
				)
				.Version(2, (v) => v
					.ConfigureOpenApiInfo((i) => i.Description = ids["2"])
				)
				.Build();

			var app = await OpenApiTestApp.StartAsync(
				configureServices: (services, _) =>
				{
					services.AddVersionedOpenApi(set);
				},
				configureApp: (app) =>
				{
					app.MapOpenApi();
				}
			);

			{
				var doc = await app.GetOpenApiDocumentAsync("openapi/v1.json");
				Assert.Equal(ids["1"], doc.Info.Description);
			}

			{
				var doc = await app.GetOpenApiDocumentAsync("openapi/v2.json");
				Assert.Equal(ids["2"], doc.Info.Description);
			}
		}

		[Fact]
		public async Task Sets_info_version()
		{
			var set = new RouteVersionSetBuilder<int>().Version(1).Version(2).Build();

			var app = await OpenApiTestApp.StartAsync(
				configureServices: (services, _) =>
				{
					services.AddVersionedOpenApi(set);
				},
				configureApp: (app) =>
				{
					app.MapOpenApi();
				}
			);

			{
				var doc = await app.GetOpenApiDocumentAsync("openapi/v1.json");
				Assert.Equal("v1", doc.Info.Version);
			}

			{
				var doc = await app.GetOpenApiDocumentAsync("openapi/v2.json");
				Assert.Equal("v2", doc.Info.Version);
			}
		}

		private class Transformer : IOpenApiDocumentTransformer
		{
			public int Count { get; private set; }

			public Task TransformAsync(OpenApiDocument doc, OpenApiDocumentTransformerContext ctx, CancellationToken ct)
			{
				++Count;
				return Task.CompletedTask;
			}
		}
	}

	public class SunsetTests
	{
		[Fact]
		public async Task Marks_sunsetted_operations_in_versioned_docs()
		{
			var set = new RouteVersionSetBuilder<int>()
				.Version(1, (v) => v.Sunset(DateTime.Now))
				.Version(2)
				.Build();

			var app = await OpenApiTestApp.StartAsync(
				configureServices: (services, _) =>
				{
					services.AddVersionedOpenApi(set);
				},
				configureApp: (app) =>
				{
					var v = app.WithVersions(set);
					v.From(1).MapGet("a", () => { });

					app.MapOpenApi();
				}
			);

			{
				var doc = await app.GetOpenApiDocumentAsync("openapi/v1.json");
				Assert.True(doc.Paths["/v1/a"].Operations[OperationType.Get].Deprecated);
			}

			{
				var doc = await app.GetOpenApiDocumentAsync("openapi/v2.json");
				Assert.False(doc.Paths["/v2/a"].Operations[OperationType.Get].Deprecated);
			}
		}

		[Fact]
		public async Task Marks_sunsetted_operations_in_default_doc_if_configured()
		{
			var set = new RouteVersionSetBuilder<int>()
				.Version(1, (v) => v.Sunset(DateTime.Now))
				.Version(2)
				.Build();

			var app = await OpenApiTestApp.StartAsync(
				configureServices: (services, _) =>
				{
					services.AddOpenApi("current", (v) => v.MarkSunsettedOperations());
				},
				configureApp: (app) =>
				{
					var v = app.WithVersions(set);
					v.From(1).MapGet("a", () => { });

					app.MapOpenApi();
				}
			);

			var doc = await app.GetOpenApiDocumentAsync("openapi/current.json");
			Assert.True(doc.Paths["/v1/a"].Operations[OperationType.Get].Deprecated);
			Assert.False(doc.Paths["/v2/a"].Operations[OperationType.Get].Deprecated);
		}
	}
}
