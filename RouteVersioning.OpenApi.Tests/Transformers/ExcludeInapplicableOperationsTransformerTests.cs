namespace RouteVersioning.OpenApi.Tests.Transformers;

using Microsoft.OpenApi.Models;
using RouteVersioning.OpenApi.Transformers;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class ExcludeInapplicableOperationsTransformerTests
{
	[Fact]
	public async Task Retains_unversioned_operations_if_configured()
	{
		var set = new RouteVersionSetBuilder<int>().Version(1).Build();
		var versionedActionDesc = TestHelpers.MakeApiDescription(metadata: [set.GetMetadata(1)]);
		var unversionedActionDesc = TestHelpers.MakeApiDescription();

		var doc = new OpenApiDocument();
		doc.AddOperation(OperationType.Get, "/versioned", versionedActionDesc);
		doc.AddOperation(OperationType.Get, "/unversioned", unversionedActionDesc);

		var transformer = new ExcludeInapplicableOperationsTransformer<int>(
			set.GetMetadata(1),
			includeUnversionedEndpoints: true
		);

		await transformer.TransformAsync(
			doc,
			TestHelpers.TransformerContext(descriptions: [versionedActionDesc, unversionedActionDesc]),
			CancellationToken.None
		);

		Assert.True(doc.Paths.ContainsKey("/versioned"));
		Assert.True(doc.Paths.ContainsKey("/unversioned"));
	}

	[Fact]
	public async Task Removes_unversioned_operations_if_configured()
	{
		var set = new RouteVersionSetBuilder<int>().Version(1).Build();
		var versionedActionDesc = TestHelpers.MakeApiDescription(metadata: [set.GetMetadata(1)]);
		var unversionedActionDesc = TestHelpers.MakeApiDescription();

		var doc = new OpenApiDocument();
		doc.AddOperation(OperationType.Get, "/versioned", versionedActionDesc);
		doc.AddOperation(OperationType.Get, "/unversioned", unversionedActionDesc);

		var transformer = new ExcludeInapplicableOperationsTransformer<int>(
			set.GetMetadata(1),
			includeUnversionedEndpoints: false
		);

		await transformer.TransformAsync(
			doc,
			TestHelpers.TransformerContext(descriptions: [versionedActionDesc, unversionedActionDesc]),
			CancellationToken.None
		);

		Assert.True(doc.Paths.ContainsKey("/versioned"));
		Assert.False(doc.Paths.ContainsKey("/unversioned"));
	}

	[Fact]
	public async Task Removes_operations_of_other_versions()
	{
		var set = new RouteVersionSetBuilder<int>().Version(1).Version(2).Build();
		var v1ActionDesc = TestHelpers.MakeApiDescription(metadata: [set.GetMetadata(1)]);
		var v2ActionDesc = TestHelpers.MakeApiDescription(metadata: [set.GetMetadata(2)]);

		var doc = new OpenApiDocument();
		doc.AddOperation(OperationType.Get, "/v1", v1ActionDesc);
		doc.AddOperation(OperationType.Get, "/v2", v2ActionDesc);

		var transformer = new ExcludeInapplicableOperationsTransformer<int>(
			set.GetMetadata(1),
			includeUnversionedEndpoints: false
		);

		await transformer.TransformAsync(
			doc,
			TestHelpers.TransformerContext(descriptions: [v1ActionDesc, v2ActionDesc]),
			CancellationToken.None
		);

		Assert.True(doc.Paths.ContainsKey("/v1"));
		Assert.False(doc.Paths.ContainsKey("/v2"));
	}

	[Fact]
	public async Task Removes_operations_of_other_sets()
	{
		var set1 = new RouteVersionSetBuilder<int>("set1").Version(1).Build();
		var s1v1ActionDesc = TestHelpers.MakeApiDescription(metadata: [set1.GetMetadata(1)]);

		var set2 = new RouteVersionSetBuilder<int>("set2").Version(1).Version(2).Build();
		var s2v1ActionDesc = TestHelpers.MakeApiDescription(metadata: [set2.GetMetadata(1)]);

		var doc = new OpenApiDocument();
		doc.AddOperation(OperationType.Get, "/set1-v1", s1v1ActionDesc);
		doc.AddOperation(OperationType.Get, "/set2-v1", s2v1ActionDesc);

		var transformer = new ExcludeInapplicableOperationsTransformer<int>(
			set1.GetMetadata(1),
			includeUnversionedEndpoints: false
		);

		await transformer.TransformAsync(
			doc,
			TestHelpers.TransformerContext(descriptions: [s1v1ActionDesc, s2v1ActionDesc]),
			CancellationToken.None
		);

		Assert.True(doc.Paths.ContainsKey("/set1-v1"));
		Assert.False(doc.Paths.ContainsKey("/set2-v1"));
	}
}
