namespace RouteVersioning.Tests;

using Microsoft.AspNetCore.Builder;
using System;
using Xunit;

public class RouteVersionMetadataTests
{
	public class VersionSetTests
	{
		[Fact]
		public void References_version_set()
		{
			var set = new RouteVersionSetBuilder<int>().Build();
			var meta = new RouteVersionMetadataBuilder<int>(1).Build(set);

			Assert.Equal(1, meta.Version);
			Assert.Equal(set, meta.Set);
		}

		[Fact]
		public void References_version_set_when_added_via_version_set()
		{
			var set = new RouteVersionSetBuilder<int>().Version(1).Version(2).Build();

			var meta1 = set.GetMetadata(1);
			var meta2 = set.GetMetadata(2);

			Assert.Equal(1, meta1.Version);
			Assert.Equal(set, meta1.Set);

			Assert.Equal(2, meta2.Version);
			Assert.Equal(set, meta2.Set);
		}
	}

	public class FeaturesTests
	{
		[Fact]
		public void Includes_added_features()
		{
			var set = new RouteVersionSetBuilder<int>().Build();
			var f11 = new Feature1();
			var f12 = new Feature1();
			var f21 = new Feature2();
			var f22 = new Feature2();

			var meta = new RouteVersionMetadataBuilder<int>(1)
				.WithFeature(f11)
				.WithFeature(f12)
				.WithFeature(f21)
				.WithFeature(f22)
				.Build(set);

			var builtF1s = meta.GetFeatures<Feature1>();
			var builtF2s = meta.GetFeatures<Feature2>();

			Assert.Contains(f11, builtF1s);
			Assert.Contains(f12, builtF1s);
			Assert.Contains(f21, builtF2s);
			Assert.Contains(f22, builtF2s);
		}

		public class Feature1
		{
		}

		public class Feature2
		{
		}
	}

	public class EndpointConventionBuilderTests
	{
		[Fact]
		public void Includes_added_conventions()
		{
			var set = new RouteVersionSetBuilder<int>().Build();
			Action<EndpointBuilder> c1 = (b) => { };
			Action<EndpointBuilder> c2 = (b) => { };
			Action<EndpointBuilder> fc1 = (b) => { };
			Action<EndpointBuilder> fc2 = (b) => { };

			var builder = new RouteVersionMetadataBuilder<int>(1);
			var conventionBuilder = (IEndpointConventionBuilder)builder;

			conventionBuilder.Add(c1);
			conventionBuilder.Add(c2);
			conventionBuilder.Finally(fc1);
			conventionBuilder.Finally(fc2);

			var meta = builder.Build(set);

			Assert.Contains(c1, meta.conventions);
			Assert.Contains(c2, meta.conventions);
			Assert.Contains(fc1, meta.finallyConventions);
			Assert.Contains(fc2, meta.finallyConventions);
		}
	}
}
