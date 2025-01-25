namespace RouteVersioning.OpenApi.Tests.Transformers;

using Microsoft.OpenApi.Models;
using RouteVersioning.OpenApi.Transformers;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class MarkSunsettedOperationsTransformerTests
{
	[Fact]
	public async Task Marks_sunsetted_operations()
	{
		var set = new RouteVersionSetBuilder<int>()
			.Version(1, (v) => v
				.Sunset(DateTime.Now)
			)
			.Version(2)
			.Build();

		var sunsettedActionDesc = TestHelpers.MakeApiDescription(metadata: [set.GetMetadata(1)]);
		var otherActionDesc = TestHelpers.MakeApiDescription();

		var doc = new OpenApiDocument();
		doc.AddOperation(OperationType.Get, "/sunsetted", sunsettedActionDesc);
		doc.AddOperation(OperationType.Get, "/other", otherActionDesc);

		var transformer = new MarkSunsettedOperationsTransformer();

		await transformer.TransformAsync(
			doc,
			TestHelpers.TransformerContext(descriptions: [sunsettedActionDesc, otherActionDesc]),
			CancellationToken.None
		);

		Assert.True(doc.Paths["/sunsetted"].Operations[OperationType.Get].Deprecated);
		Assert.False(doc.Paths["/other"].Operations[OperationType.Get].Deprecated);
	}
}
