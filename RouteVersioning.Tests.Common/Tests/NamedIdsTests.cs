namespace RouteVersioning.Tests.Common.Tests;

using Xunit;

public class NamedIdsTests
{
	[Fact]
	public void Makes_same_id_for_same_name()
	{
		var ids = new NamedIds();
		Assert.Equal(ids["a"], ids["a"]);
	}

	[Fact]
	public void Makes_different_ids_for_different_names()
	{
		var ids = new NamedIds();
		Assert.NotEqual(ids["a"], ids["b"]);
	}
}
