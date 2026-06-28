using Opengate.Modules.Shared.Domain.ValueObjects.Pagination;

namespace Opengate.Tests.Unit.Modules.Shared.Domain.ValueObjects.Pagiantion;

public class OffsetPaginationQueryTest
{
    [Fact]
    public void OffsetPaginationQueryTest_DefualtPagination()
    {
        //Given
        var pagination = new OffsetPaginationQuery();

        //Then
        pagination.PageSize.ShouldBe(OffsetPaginationQuery.InitialPageSize);
        pagination.Page.ShouldBe(OffsetPaginationQuery.InitialPage);
    }

    [Fact]
    public void OffsetPaginationQueryTest_ShouldShowTheCorrectPageSize_WhenInitWithPageSize()
    {
        //Given
        var pagination = OffsetPaginationQuery.InitWithPageSize(500);

        //Then
        pagination.PageSize.ShouldBe(500);
        pagination.Page.ShouldBe(OffsetPaginationQuery.InitialPage);
    }

    [Fact]
    public void OffsetPaginationQueryTest_ShouldFail_WhenNegativePage()
    {
        //Given
        var paginationFunc = () => new OffsetPaginationQuery(10, -1);

        //Then
        paginationFunc.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void OffsetPaginationQueryTest_ShouldFail_WhenPageLowerThanInitialFirstPage()
    {
        //Given
        var paginationFunc = () => new OffsetPaginationQuery(10, OffsetPaginationQuery.InitialPage - 1);

        //Then
        paginationFunc.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(int.MinValue)]
    public void OffsetPaginationQueryTest_ShouldFail_WhenPageSizeLowerThanOrEqualZero(int pageSize)
    {
        //Given
        var paginationFunc = () => new OffsetPaginationQuery(pageSize, 1);

        //Then
        paginationFunc.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void OffsetPaginationQueryTest_ShouldInitWithZeroSkip_OnPage1()
    {
        //Given
        var pagination = OffsetPaginationQuery.InitWithPageSize(20);
        //Then
        pagination.GetSkip().ShouldBe(0);
    }

    [Fact]
    public void OffsetPaginationQueryTest_ShouldRenderSkip_WithPageSize()
    {
        //Given
        var pagination = new OffsetPaginationQuery(
            pageSize: 20,
            page: 3);

        //Then
        pagination.GetSkip().ShouldBe(40);
        pagination.GetLimit().ShouldBe(20);
        pagination.Page.ShouldBe(3);
        pagination.PageSize.ShouldBe(20);
    }

    [Fact]
    public void OffsetPaginationQueryTest_ShouldDefaultSize_WithDefaultObject()
    {
        //Given
        var pagination = OffsetPaginationQuery.Default();

        //Then
        pagination.GetSkip().ShouldBe(0);
        pagination.Page.ShouldBe(OffsetPaginationQuery.InitialPage);
    }
}