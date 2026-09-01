namespace DogSab.Platform.Debugger.Abstractions;

/// <summary>
/// A single named value visible within a stack frame's current scope,
/// shown in a future Variables panel while a debug session is
/// <see cref="DebugSessionState.Paused"/>.
/// </summary>
public readonly struct Variable
{
    /// <summary>
    /// The variable's name as declared in source code.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The variable's current value, formatted as a display string by the
    /// debug adapter (e.g. <c>"42"</c>, <c>"\"hello\""</c>,
    /// <c>"MyApp.Foo {Bar = 1}"</c>). Left as a plain string rather than
    /// a structured value, since debug adapters vary widely in how much
    /// structural detail they expose for complex types, and the platform
    /// itself has no way to meaningfully interpret an arbitrary runtime's
    /// object representation.
    /// </summary>
    public string DisplayValue { get; }

    /// <summary>
    /// The variable's declared or runtime type name, as reported by the
    /// debug adapter (e.g. <c>"int"</c>, <c>"string"</c>).
    /// </summary>
    public string TypeName { get; }

    /// <summary>
    /// Whether this variable's value can be further expanded to show
    /// nested fields/properties (e.g. an object or collection), as opposed
    /// to being a terminal, already-fully-shown value like a primitive.
    /// </summary>
    public bool IsExpandable { get; }

    /// <summary>
    /// Creates a new variable.
    /// </summary>
    /// <param name="name">
    /// The variable's name.
    /// </param>
    /// <param name="displayValue">
    /// The variable's current value, as a display string.
    /// </param>
    /// <param name="typeName">
    /// The variable's type name.
    /// </param>
    /// <param name="isExpandable">
    /// Whether this variable can be expanded to show nested fields.
    /// </param>
    public Variable(string name, string displayValue, string typeName, bool isExpandable)
    {
        Name = name;
        DisplayValue = displayValue;
        TypeName = typeName;
        IsExpandable = isExpandable;
    }
}