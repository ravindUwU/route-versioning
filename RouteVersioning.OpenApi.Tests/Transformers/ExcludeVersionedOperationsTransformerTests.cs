namespace RouteVersioning.OpenApi.Tests.Transformers;

using Microsoft.OpenApi.Models;
using RouteVersioning.OpenApi.Transformers;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class ExcludeVersionedOperationsTransformerTests
{
	[Fact]
	public async Task Removes_operations_with_version_metadata()
	{
		var set = new RouteVersionSetBuilder<int>().Version(1).Build();
		var versionedActionDesc = TestHelpers.MakeApiDescription(metadata: [set.GetMetadata(1)]);
		var unversionedActionDesc = TestHelpers.MakeApiDescription();

		var doc = new OpenApiDocument();
		doc.AddOperation(OperationType.Get, "/versioned", versionedActionDesc);
		doc.AddOperation(OperationType.Get, "/unversioned", unversionedActionDesc);

		var transformer = new ExcludeVersionedOperationsTransformer();

		await transformer.TransformAsync(
			doc,
			TestHelpers.TransformerContext(descriptions: [versionedActionDesc, unversionedActionDesc]),
			CancellationToken.None
		);

		Assert.False(doc.Paths.ContainsKey("/versioned"));
		Assert.True(doc.Paths.ContainsKey("/unversioned"));
	}
}
