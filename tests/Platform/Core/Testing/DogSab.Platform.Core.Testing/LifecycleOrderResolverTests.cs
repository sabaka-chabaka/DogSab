using DogSab.Platform.Core.Impl.Lifecycle;
using FluentAssertions;

namespace DogSab.Platform.Core.Testing;

public class LifecycleOrderResolverTests
{
    private readonly LifecycleOrderResolver _resolver;

    public LifecycleOrderResolverTests()
    {
        _resolver = new LifecycleOrderResolver();
    }

    private class TestItem
    {
        public string Name { get; init; } = "";
        public int Order { get; init; }
    }

    [Fact]
    public void Resolve_ShouldOrderItemsByAscendingOrder()
    {
        // Arrange
        var items = new[]
        {
            new TestItem { Name = "C", Order = 10 },
            new TestItem { Name = "A", Order = 1 },
            new TestItem { Name = "B", Order = 5 }
        };

        // Act
        var result = _resolver.Resolve(items, x => x.Order);

        // Assert
        result.Select(x => x.Name).Should().Equal("A", "B", "C");
    }

    [Fact]
    public void Resolve_ShouldBeStable_WhenOrdersAreEqual()
    {
        // Arrange
        var items = new[]
        {
            new TestItem { Name = "A1", Order = 1 },
            new TestItem { Name = "B", Order = 5 },
            new TestItem { Name = "A2", Order = 1 }
        };

        // Act
        var result = _resolver.Resolve(items, x => x.Order);

        // Assert
        result.Select(x => x.Name).Should().Equal("A1", "A2", "B");
    }

    [Fact]
    public void Resolve_ShouldThrow_WhenItemsContainsNull()
    {
        // Arrange
        var items = new TestItem?[] { new TestItem(), null };

        // Act
        var act = () => _resolver.Resolve(items!, x => x.Order);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*Sequence must not contain null elements*");
    }

    [Fact]
    public void Resolve_ShouldThrow_WhenItemsIsNull()
    {
        // Act
        var act = () => _resolver.Resolve<TestItem>(null!, x => x.Order);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
