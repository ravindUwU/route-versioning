namespace RouteVersioning.OpenApi.Tests.Transformers;

using Microsoft.OpenApi.Models;
using RouteVersioning.OpenApi.Transformers;
using RouteVersioning.Tests.Common;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class DocumentInfoTransformerTests
{
	[Fact]
	public async Task Sets_info_to_version_set_slug()
	{
		var set = new RouteVersionSetBuilder<int>().Version(1).Build();
		var transformer = new DocumentInfoTransformer<int>(set.GetMetadata(1));

		var doc = new OpenApiDocument();
		await transformer.TransformAsync(doc, TestHelpers.TransformerContext(), CancellationToken.None);

		Assert.Equal("v1", doc.Info.Version);
	}

	[Fact]
	public async Task Sets_info_to_named_version_set_slug()
	{
		var set = new RouteVersionSetBuilder<int>("name").Version(1).Build();
		var transformer = new DocumentInfoTransformer<int>(set.GetMetadata(1));

		var doc = new OpenApiDocument();
		await transformer.TransformAsync(doc, TestHelpers.TransformerContext(), CancellationToken.None);

		Assert.Equal("v1", doc.Info.Version);
	}

	[Fact]
	public async Task Runs_info_configuration_delegates()
	{
		var ids = new NamedIds();
		var set = new RouteVersionSetBuilder<int>("name")
			.Version(1, (v) => v
				.ConfigureOpenApiInfo((i) => i.Description = ids["info"])
			)
			.Build();

		var transformer = new DocumentInfoTransformer<int>(set.GetMetadata(1));

		var doc = new OpenApiDocument();
		await transformer.TransformAsync(doc, TestHelpers.TransformerContext(), CancellationToken.None);

		Assert.Equal(ids["info"], doc.Info.Description);
	}
}
