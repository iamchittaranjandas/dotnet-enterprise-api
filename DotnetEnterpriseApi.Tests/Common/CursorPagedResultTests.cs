using DotnetEnterpriseApi.Application.Common.Models;
using FluentAssertions;

namespace DotnetEnterpriseApi.Tests.Common
{
    public class CursorPagedResultTests
    {
        [Fact]
        public void CursorPagedResult_DefaultValues()
        {
            var result = new CursorPagedResult<string>();

            result.Items.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.NextCursor.Should().BeNull();
            result.HasNextPage.Should().BeFalse();
        }

        [Fact]
        public void CursorPagedResult_WithData()
        {
            var result = new CursorPagedResult<string>
            {
                Items = new List<string> { "a", "b" },
                NextCursor = 5,
                HasNextPage = true
            };

            result.Items.Should().HaveCount(2);
            result.NextCursor.Should().Be(5);
            result.HasNextPage.Should().BeTrue();
        }
    }
}
