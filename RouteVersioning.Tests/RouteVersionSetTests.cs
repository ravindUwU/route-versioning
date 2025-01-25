namespace RouteVersioning.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class RouteVersionSetTests
{
	public class VersionRegistrationTests
	{
		[Fact]
		public void Disallows_repeatedly_adding_the_same_version()
		{
			var builder = new RouteVersionSetBuilder<int>().Version(1);

			Assert.Throws<InvalidOperationException>(() => builder.Version(1));
		}

		[Fact]
		public void Retrieves_metadata_for_defined_version()
		{
			var set = new RouteVersionSetBuilder<int>().Version(1).Version(2).Build();

			var m1 = set.GetMetadata(1);
			var m2 = set.GetMetadata(2);

			Assert.Equal(1, m1.Version);
			Assert.Equal(2, m2.Version);
		}

		[Fact]
		public void Throws_when_retrieving_metadata_for_undefined_version()
		{
			var set = new RouteVersionSetBuilder<int>().Version(1).Build();

			Assert.Throws<ArgumentException>(
				() => set.GetMetadata(2)
			);
		}

		[Fact]
		public void Allows_checking_whether_version_is_defined()
		{
			var set = new RouteVersionSetBuilder<int>().Version(1).Build();

			Assert.True(set.Contains(1));
			Assert.False(set.Contains(2));
		}

		[Fact]
		public void Enumerates_added_versions()
		{
			var set = new RouteVersionSetBuilder<int>().Version(1).Version(2).Version(3).Build();

			var versions = set.ToList();

			Assert.Contains(1, versions);
			Assert.Contains(2, versions);
			Assert.Contains(3, versions);
		}
	}

	public class SlugTests
	{
		[Fact]
		public void Defaults_to_slug_of_format_vN()
		{
			var set = new RouteVersionSetBuilder<int>().Build();

			Assert.Equal("v1", set.GetSlug(1));
			Assert.Equal("v2", set.GetSlug(2));
		}

		[Fact]
		public void Uses_custom_slug_function()
		{
			var id = Guid.NewGuid();
			Func<int, string> slug = (v) => $"v{v}-{id}";

			var set = new RouteVersionSetBuilder<int>().WithSlug(slug).Build();

			Assert.Equal(slug(1), set.GetSlug(1));
		}

		[Fact]
		public void Uses_custom_slug_pattern()
		{
			var id = Guid.NewGuid();
			var slug = $"{{0}}-{id}";

			var set = new RouteVersionSetBuilder<int>().WithSlug(slug).Build();

			Assert.Equal($"1-{id}", set.GetSlug(1));
		}

		[Fact]
		public void Throws_when_slug_pattern_doesnt_include_placeholder()
		{
			var ex = Assert.Throws<ArgumentException>(
				() => new RouteVersionSetBuilder<int>().WithSlug("v")
			);
			Assert.Equal("pattern", ex.ParamName);
		}

		[Fact]
		public void Throws_when_slug_pattern_includes_more_than_1_placeholder()
		{
			var ex = Assert.Throws<ArgumentException>(
				() => new RouteVersionSetBuilder<int>().WithSlug("v{0}v{0}")
			);
			Assert.Equal("pattern", ex.ParamName);
		}
	}

	public class NamedSlugTests
	{
		[Fact]
		public void Makes_same_slug_regardless_of_name()
		{
			var name = $"{Guid.NewGuid()}";

			var set1 = new RouteVersionSetBuilder<int>().Build();
			var set2 = new RouteVersionSetBuilder<int>(name).Build();

			Assert.Null(set1.Name);
			Assert.Equal($"v1", set1.GetSlug(1));
			Assert.Equal($"v1", set2.GetSlug(1));
		}

		[Fact]
		public void Makes_named_slug_identical_to_slug_when_unnamed()
		{
			var set = new RouteVersionSetBuilder<int>().Build();

			Assert.Null(set.Name);
			Assert.Equal(set.GetSlug(1), set.GetNamedSlug(1));
			Assert.Equal(set.GetSlug(2), set.GetNamedSlug(2));
		}

		[Fact]
		public void Makes_named_slug_by_prefixing_name_to_slug()
		{
			var name = $"{Guid.NewGuid()}";
			var set = new RouteVersionSetBuilder<int>(name).Build();

			Assert.Equal(name, set.Name);
			Assert.Equal($"{name}-v1", set.GetNamedSlug(1));
			Assert.Equal($"{name}-v2", set.GetNamedSlug(2));
		}
	}

	public class ComparerTests
	{
		[Fact]
		public void Uses_custom_comparer()
		{
			var comparer = new CustomComparer();
			var set = new RouteVersionSetBuilder<int>().WithComparer(comparer).Build();

			Assert.Equal(0, comparer.Count);

			set.Compare(1, 2);

			Assert.Equal(1, comparer.Count);
		}

		public class CustomComparer : IComparer<int>
		{
			public int Count { get; private set; }

			public int Compare(int x, int y)
			{
				++Count;
				return Comparer<int>.Default.Compare(x, y);
			}
		}
	}
}
