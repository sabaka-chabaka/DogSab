using DogSab.Platform.Psi.Abstractions.Tree;

namespace DogSab.Platform.Psi.Tree;

/// <summary>
/// Default, mutable-during-construction implementation of <see cref="IPsiElement"/>.
/// Children are added via <see cref="AddChild"/> while
/// <see cref="Building.PsiTreeBuilder"/> assembles a tree from a language's
/// <see cref="Abstractions.Parsing.IParser"/> output; once construction is
/// complete, the tree is treated as immutable by the rest of the platform —
/// there is no public mutation API on <see cref="Abstractions.Tree.IPsiElement"/> itself.
/// </summary>
public class PsiElementImpl : IPsiElement
{
    private readonly List<IPsiElement> _children = new();
    private readonly IPsiFile _containingFile;

    /// <inheritdoc />
    public PsiElementType Type { get; }

    /// <inheritdoc />
    public int StartOffset { get; }

    /// <inheritdoc />
    public int Length { get; }

    /// <inheritdoc />
    public string Text { get; }

    /// <inheritdoc />
    public IPsiElement? Parent { get; internal set; }

    /// <inheritdoc />
    public IReadOnlyList<IPsiElement> Children => _children;

    /// <inheritdoc />
    public IPsiFile ContainingFile => _containingFile;

    /// <summary>
    /// Creates a new PSI element.
    /// </summary>
    /// <param name="type">The kind of node this is.</param>
    /// <param name="startOffset">The zero-based character offset where this node starts.</param>
    /// <param name="length">The length, in characters, of this node's span.</param>
    /// <param name="text">This node's exact source text.</param>
    /// <param name="containingFile">The file this node belongs to.</param>
    public PsiElementImpl(PsiElementType type, int startOffset, int length, string text, IPsiFile containingFile)
    {
        Type = type;
        StartOffset = startOffset;
        Length = length;
        Text = text;
        _containingFile = containingFile;
    }

    /// <summary>
    /// Adds a child node during tree construction, setting the child's
    /// <see cref="Parent"/> back-reference. Called only by
    /// <see cref="Building.PsiTreeBuilder"/> while assembling a tree; not
    /// part of the public <see cref="IPsiElement"/> contract.
    /// </summary>
    /// <param name="child">The child node to add.</param>
    internal void AddChild(PsiElementImpl child)
    {
        child.Parent = this;
        _children.Add(child);
    }
}