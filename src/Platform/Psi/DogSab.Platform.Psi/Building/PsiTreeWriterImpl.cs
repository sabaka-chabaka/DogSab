using DogSab.Platform.Psi.Abstractions.Parsing;
using DogSab.Platform.Psi.Abstractions.Tree;
using DogSab.Platform.Psi.Tree;

namespace DogSab.Platform.Psi.Building;

/// <summary>
/// Default implementation of <see cref="IPsiTreeWriter"/>, wrapping a
/// concrete <see cref="PsiElementImpl"/> node without exposing that concrete
/// type to language plugins — plugins only ever see the narrow
/// <see cref="IPsiTreeWriter"/> contract.
/// </summary>
internal sealed class PsiTreeWriterImpl : IPsiTreeWriter
{
    private readonly PsiElementImpl _node;
    private readonly IPsiFile _containingFile;
    private readonly string _sourceText;

    public PsiTreeWriterImpl(PsiElementImpl node, IPsiFile containingFile, string sourceText)
    {
        _node = node;
        _containingFile = containingFile;
        _sourceText = sourceText;
    }

    /// <inheritdoc />
    public IPsiTreeWriter AppendChild(PsiElementType type, int startOffset, int length)
    {
        var text = _sourceText.Substring(startOffset, length);
        var child = new PsiElementImpl(type, startOffset, length, text, _containingFile);

        _node.AddChild(child);

        return new PsiTreeWriterImpl(child, _containingFile, _sourceText);
    }
}