using Opengate.Modules.Shared.Domain.ValueObjects.Slugs;

namespace Opengate.Tests.Unit.Modules.Shared.Domain.ValueObjects.Slugs;

public class CreateSlugTest
{
    [Fact]
    public void CreateSlugTest_ShouldCreateUrlSafeSlug_WhenDirtyStringIsPassed()
    {
        var dirtyString = "this is a dirty string";

        var slug = Slug.Parse(dirtyString);

        Assert.Equal("this-is-a-dirty-string", slug.ToString());
    }

    [Fact]
    public void CreateSlugTest_ShouldCreateUrlSafeSlug_WithDiacritics()
    {
        var dirtyString = "Abafé e esqueça";

        var slug = Slug.Parse(dirtyString);

        slug.ToString().ShouldBe("abaf-e-esquea");
    }

    [Fact]
    public void CreateSlugTest_ShouldKeepName_WhenAlreadyUrlSafe()
    {
        var dirtyString = "my-system";

        var slug = Slug.Parse(dirtyString);

        slug.ToString().ShouldBe("my-system");
    }
}