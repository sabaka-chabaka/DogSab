namespace DogSab.Platform.Core.Impl.Lifecycle;

/// <summary>
/// Sorts a sequence of items by an integer order key, preserving the relative
/// order of items that share the same key (stable sort). Used wherever the
/// platform needs to order plugin-contributed items by a simple declared
/// priority — e.g. startup activities, actions, or inspections — as opposed
/// to <see cref="Components.ComponentDependencyResolver"/>, which resolves
/// order from an explicit dependency graph rather than a flat priority value.
/// </summary>
public sealed class LifecycleOrderResolver
{
    /// <summary>
    /// Returns the items in ascending order of the value produced by <paramref name="orderSelector"/>.
    /// Items with an equal order value retain their original relative position.
    /// </summary>
    /// <typeparam name="T">The type of item being ordered.</typeparam>
    /// <param name="items">The items to order.</param>
    /// <param name="orderSelector">A function returning the order key for a given item (lower runs/appears first).</param>
    /// <returns>A new list containing the items in resolved order.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="items"/> or <paramref name="orderSelector"/> is <c>null</c>,
    /// or if <paramref name="items"/> contains a <c>null</c> element and <typeparamref name="T"/> is a reference type.
    /// </exception>
    public IReadOnlyList<T> Resolve<T>(IEnumerable<T> items, Func<T, int> orderSelector)
    {
        if (items is null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        if (orderSelector is null)
        {
            throw new ArgumentNullException(nameof(orderSelector));
        }

        return items
            .Select((item, index) =>
            {
                if (item is null)
                {
                    throw new ArgumentNullException(nameof(items), "Sequence must not contain null elements.");
                }

                return (item, index);
            })
            .OrderBy(pair => orderSelector(pair.item))
            .ThenBy(pair => pair.index) // explicit stability guard, independent of the underlying sort implementation
            .Select(pair => pair.item)
            .ToArray();
    }
}