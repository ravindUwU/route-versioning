namespace RouteVersioning.OpenApi.Transformers;

using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

internal static class Helpers
{
	public class OpenApiDocumentActions : Dictionary<string, ActionDescriptor>
	{
		public OpenApiDocumentActions(OpenApiDocumentTransformerContext ctx)
		{
			foreach (var a in ctx.DescriptionGroups
				.SelectMany((g) => g.Items)
				.Select((d) => d.ActionDescriptor)
			)
			{
				this[a.Id] = a;
			}
		}

		public bool TryGetAction(OpenApiOperation op, [NotNullWhen(true)] out ActionDescriptor? action)
		{
			if (op.Annotations.TryGetValue("x-aspnetcore-id", out var _actionId)
				&& _actionId is string actionId
				&& TryGetValue(actionId, out action))
			{
				return true;
			}

			action = null;
			return false;
		}
	}
}
