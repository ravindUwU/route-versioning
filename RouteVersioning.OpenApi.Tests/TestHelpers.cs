namespace RouteVersioning.OpenApi.Tests;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;

internal static class TestHelpers
{
	public static OpenApiDocumentTransformerContext TransformerContext(
		Action<IServiceCollection>? configureServices = null,
		IReadOnlyList<ApiDescription>? descriptions = null
	)
	{
		var services = new ServiceCollection();
		configureServices?.Invoke(services);

		return new OpenApiDocumentTransformerContext
		{
			ApplicationServices = services.BuildServiceProvider(),
			DescriptionGroups = [
				new ApiDescriptionGroup(groupName: null, items: descriptions ?? [])
			],
			DocumentName = "doc",
		};
	}

	public static void AddOperation(
		this OpenApiDocument doc,
		OperationType op,
		PathString path,
		ApiDescription desc
	)
	{
		doc.Paths ??= [];

		if (!doc.Paths.ContainsKey(path))
		{
			doc.Paths[path] = new OpenApiPathItem
			{
				Operations = new Dictionary<OperationType, OpenApiOperation>(),
			};
		}

		var docPath = doc.Paths[path];

		if (docPath.Operations.ContainsKey(op))
		{
			throw new InvalidOperationException($"The path {path} already contains a {op} operation.");
		}

		docPath.Operations[op] = new OpenApiOperation
		{
			Annotations = new Dictionary<string, object>
			{
				["x-aspnetcore-id"] = desc.ActionDescriptor.Id,
			},
		};
	}

	public static ApiDescription MakeApiDescription(
		IList<object>? metadata = null
	)
	{
		return new ApiDescription
		{
			ActionDescriptor = new ActionDescriptor
			{
				EndpointMetadata = metadata ?? [],
			},
		};
	}
}
