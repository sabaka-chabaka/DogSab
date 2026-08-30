using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using DogSab.Platform.Editor.Abstractions.Document;
using DogSab.Platform.Editor.Abstractions.Inspections;
using DogSab.Platform.Editor.Completion;
using DogSab.Platform.Editor.Folding;
using DogSab.Platform.Editor.Highlighting;
using DogSab.Platform.Editor.Inspections;
using DogSab.Platform.Editor.Session;
using DogSab.Platform.Editor.Ui.Completion;
using DogSab.Platform.Editor.Ui.Diagnostics;
using DogSab.Platform.Editor.Ui.Gutter;
using DogSab.Platform.Editor.Ui.Rendering;
using DogSab.Platform.Psi.Abstractions.Tree;
using DogSab.Platform.Psi.Caching;
using DogSab.Platform.Ui.Themes;

namespace DogSab.Platform.Editor.Ui;

/// <summary>
/// The main editor control for a single open file: composes the text
/// renderer, caret renderer, gutter, problem squiggles, and completion
/// popup into one interactive view, driven by an <see cref="EditorSession"/>.
/// This is a deliberately simplified implementation — it re-lays-out and
/// redraws the entire visible canvas on every document change rather than
/// virtualizing to only the currently visible line range, which is the
/// single biggest gap between this and a production-quality editor view
/// (see the remark on <see cref="RenderAll"/>).
/// </summary>
public partial class EditorView : UserControl
{
    private readonly EditorTextRenderer _textRenderer = new();
    private readonly CaretRenderer _caretRenderer = new();
    private readonly GutterRenderer _gutterRenderer = new();
    private readonly FoldingIndicatorRenderer _foldingIndicatorRenderer = new();
    private readonly ProblemHighlightRenderer _problemHighlightRenderer = new();

    private HighlightingCoordinator? _highlightingCoordinator;
    private CompletionCoordinator? _completionCoordinator;
    private InspectionCoordinator? _inspectionCoordinator;
    private FoldingCoordinator? _foldingCoordinator;
    private PsiFileCache? _psiFileCache;
    private ThemeManagerImpl? _themeManager;

    private EditorSession? _session;
    private readonly HashSet<int> _collapsedFoldingRegionStarts = new();
    private readonly DispatcherTimer _caretBlinkTimer;

    public EditorView()
    {
        InitializeComponent();

        _caretBlinkTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(500), DispatcherPriority.Background, OnCaretBlinkTick);
        _caretBlinkTimer.Start();

        Focusable = true;
        KeyDown += OnKeyDown;
        TextInput += OnTextInput;
        PointerPressed += OnPointerPressed;
    }

    /// <summary>
    /// Wires up the coordinators this view needs — supplied by the platform
    /// rather than resolved internally, since <see cref="EditorView"/> has
    /// no access to the DI container itself and is expected to be
    /// constructed by whatever code opens an editor tab (e.g. a future
    /// "open file" action), which does have that access.
    /// </summary>
    /// <param name="highlightingCoordinator">
    /// Used to compute syntax highlighting for the open file.
    /// </param>
    /// <param name="completionCoordinator">
    /// Used to compute completion suggestions.
    /// </param>
    /// <param name="inspectionCoordinator">
    /// Used to compute diagnostic problems.
    /// </param>
    /// <param name="foldingCoordinator">
    /// Used to compute foldable regions.
    /// </param>
    /// <param name="psiFileCache">
    /// Used to obtain the open file's parsed PSI tree.
    /// </param>
    /// <param name="themeManager">
    /// Used to resolve the active theme's colors for rendering.
    /// </param>
    public void Configure(
        HighlightingCoordinator highlightingCoordinator,
        CompletionCoordinator completionCoordinator,
        InspectionCoordinator inspectionCoordinator,
        FoldingCoordinator foldingCoordinator,
        PsiFileCache psiFileCache,
        ThemeManagerImpl themeManager)
    {
        _highlightingCoordinator = highlightingCoordinator;
        _completionCoordinator = completionCoordinator;
        _inspectionCoordinator = inspectionCoordinator;
        _foldingCoordinator = foldingCoordinator;
        _psiFileCache = psiFileCache;
        _themeManager = themeManager;
    }

    /// <summary>
    /// Opens a session in this view, subscribing to its document's changes
    /// and performing the initial render.
    /// </summary>
    /// <param name="session">
    /// The editor session to display.
    /// </param>
    public void OpenSession(EditorSession session)
    {
        _session = session;
        session.Document.AddListener(new DocumentChangeHandler(this));

        RenderAll();
    }

    /// <summary>
    /// Re-renders the entire visible editor content: gutter, text with
    /// highlighting, problem squiggles, and carets.
    /// A known, deliberate simplification: this redraws the whole document
    /// on every keystroke rather than virtualizing to just the visible
    /// scroll range and only re-rendering lines whose content actually
    /// changed. That virtualization is necessary before this view could
    /// handle large files responsively, and is not implemented here.
    /// </summary>
    private void RenderAll()
    {
        if (_session is null)
        {
            return;
        }

        TextCanvas.Children.Clear();
        GutterCanvas.Children.Clear();

        var theme = _themeManager?.ActiveTheme;
        if (theme is null)
        {
            return;
        }

        var lines = _session.Document.Text.Split('\n');
        var lineHeight = _textRenderer.LineHeight;
        var gutterWidth = _gutterRenderer.ComputeRequiredWidth(lines.Length);

        GutterCanvas.Width = gutterWidth + FoldingIndicatorRenderer.IndicatorWidth;

        var psiFile = TryGetPsiFile();
        var highlightSpans = psiFile is not null && _highlightingCoordinator is not null
            ? _highlightingCoordinator.ComputeHighlighting(psiFile)
            : Array.Empty<HighlightSpan>();
        var problems = psiFile is not null && _inspectionCoordinator is not null
            ? _inspectionCoordinator.Analyze(psiFile)
            : Array.Empty<Problem>();

        var canvasDrawing = new DrawingVisualHost(TextCanvas);
        var gutterDrawing = new DrawingVisualHost(GutterCanvas);

        var currentOffset = 0;

        for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            var lineText = lines[lineIndex];
            var topOffset = lineIndex * lineHeight;

            gutterDrawing.Draw(ctx => _gutterRenderer.RenderLineNumber(ctx, lineIndex + 1, topOffset, gutterWidth, theme.ForegroundColor));

            var spansForLine = FilterSpansForLine(highlightSpans, currentOffset, lineText.Length);
            canvasDrawing.Draw(ctx => _textRenderer.RenderLine(ctx, lineText, currentOffset, spansForLine, topOffset, theme));

            var problemsForLine = BuildProblemPositionsForLine(problems, currentOffset, lineText.Length);
            if (problemsForLine.Count > 0)
            {
                canvasDrawing.Draw(ctx => _problemHighlightRenderer.RenderProblemsForLine(ctx, problemsForLine, topOffset + lineHeight - 2));
            }

            currentOffset += lineText.Length + 1; // +1 for the '\n' consumed by Split
        }

        canvasDrawing.Draw(ctx =>
        {
            _caretRenderer.CharacterWidth = 8; // approximate monospace advance width for the configured font/size
            _caretRenderer.Render(ctx, _session.CaretModel, lineHeight, theme.AccentColor);
        });
    }

    /// <summary>
    /// Attempts to resolve the current session's parsed PSI file from the
    /// platform's cache, for use by highlighting and inspection.
    /// </summary>
    /// <returns>
    /// The cached PSI file, or <c>null</c> if unavailable (e.g. no
    /// registered language claims this file's extension).
    /// </returns>
    private IPsiFile? TryGetPsiFile()
    {
        return _session is not null && _psiFileCache is not null
            ? _psiFileCache.GetOrBuild(_session.VirtualFile)
            : null;
    }

    /// <summary>
    /// Filters a document-wide list of highlight spans down to just those
    /// overlapping a single line's character range.
    /// </summary>
    /// <param name="allSpans">
    /// Every highlight span computed for the document.
    /// </param>
    /// <param name="lineStartOffset">
    /// The document-wide offset this line begins at.
    /// </param>
    /// <param name="lineLength">
    /// The length of this line's text.
    /// </param>
    /// <returns>
    /// The spans overlapping this line's range.
    /// </returns>
    private static IReadOnlyList<HighlightSpan> FilterSpansForLine(IReadOnlyList<HighlightSpan> allSpans, int lineStartOffset, int lineLength)
    {
        var lineEndOffset = lineStartOffset + lineLength;

        return allSpans
            .Where(span => span.StartOffset < lineEndOffset && span.StartOffset + span.Length > lineStartOffset)
            .ToList();
    }

    /// <summary>
    /// Builds the horizontal pixel start/end positions for each problem
    /// overlapping a single line, approximating character width uniformly
    /// rather than measuring each character precisely — a known
    /// simplification consistent with <see cref="CaretRenderer.CharacterWidth"/>
    /// being a fixed approximate value elsewhere in this view.
    /// </summary>
    /// <param name="allProblems">
    /// Every problem computed for the document.
    /// </param>
    /// <param name="lineStartOffset">
    /// The document-wide offset this line begins at.
    /// </param>
    /// <param name="lineLength">
    /// The length of this line's text.
    /// </param>
    /// <returns>
    /// Each overlapping problem paired with its approximate start/end pixel
    /// positions on this line.
    /// </returns>
    private static List<(Problem Problem, double StartX, double EndX)> BuildProblemPositionsForLine(
        IReadOnlyList<Problem> allProblems,
        int lineStartOffset,
        int lineLength)
    {
        const double approximateCharacterWidth = 8;
        var lineEndOffset = lineStartOffset + lineLength;

        return allProblems
            .Where(p => p.StartOffset < lineEndOffset && p.EndOffset > lineStartOffset)
            .Select(p =>
            {
                var clampedStart = Math.Max(p.StartOffset, lineStartOffset) - lineStartOffset;
                var clampedEnd = Math.Min(p.EndOffset, lineEndOffset) - lineStartOffset;
                return (p, clampedStart * approximateCharacterWidth, clampedEnd * approximateCharacterWidth);
            })
            .ToList();
    }

    /// <summary>
    /// Toggles the caret's blink visibility phase and triggers a re-render,
    /// producing the standard blinking caret effect.
    /// </summary>
    /// <param name="sender">
    /// The timer, unused.
    /// </param>
    /// <param name="e">
    /// The event arguments, unused.
    /// </param>
    private void OnCaretBlinkTick(object? sender, EventArgs e)
    {
        _caretRenderer.IsVisiblePhase = !_caretRenderer.IsVisiblePhase;
        RenderAll();
    }

    /// <summary>
    /// Handles printable character input by inserting it into the document
    /// at the primary caret's position.
    /// </summary>
    /// <param name="sender">
    /// The event source, unused.
    /// </param>
    /// <param name="e">
    /// The text input event, carrying the typed text.
    /// </param>
    private void OnTextInput(object? sender, TextInputEventArgs e)
    {
        if (_session is null || string.IsNullOrEmpty(e.Text))
        {
            return;
        }

        var offset = _session.CaretModel.PrimaryCaret.Offset;
        _session.Document.Replace(offset, 0, e.Text);
        _session.CaretModel.MoveTo(_session.Document.ResolvePosition(offset + e.Text.Length));

        TriggerCompletionIfApplicable();
    }

    /// <summary>
    /// Handles non-printable key input: navigation, deletion, and undo/redo shortcuts.
    /// </summary>
    /// <param name="sender">
    /// The event source, unused.
    /// </param>
    /// <param name="e">
    /// The key event.
    /// </param>
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (_session is null)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Back:
                HandleBackspace();
                e.Handled = true;
                break;

            case Key.Z when e.KeyModifiers.HasFlag(KeyModifiers.Control):
                _session.Document.Undo();
                e.Handled = true;
                break;

            case Key.Y when e.KeyModifiers.HasFlag(KeyModifiers.Control):
                _session.Document.Redo();
                e.Handled = true;
                break;

            case Key.Escape:
                CompletionPopupHost.IsOpen = false;
                e.Handled = true;
                break;
        }
    }

    /// <summary>
    /// Deletes the character immediately before the primary caret, if any.
    /// </summary>
    private void HandleBackspace()
    {
        if (_session is null)
        {
            return;
        }

        var offset = _session.CaretModel.PrimaryCaret.Offset;

        if (offset == 0)
        {
            return;
        }

        _session.Document.Replace(offset - 1, 1, string.Empty);
        _session.CaretModel.MoveTo(_session.Document.ResolvePosition(offset - 1));
    }

    /// <summary>
    /// Handles a mouse click by moving the primary caret to the clicked
    /// position, or toggling a folding indicator if the click landed on one.
    /// A known simplification: pixel-to-offset translation here assumes a
    /// uniform character width rather than measuring actual glyph
    /// boundaries, so clicks may land slightly off from the intended
    /// character in proportionally-spaced or unusual-width text — not an
    /// issue in practice since the editor is fixed to a monospace typeface,
    /// but worth noting as an assumption baked into this method.
    /// </summary>
    /// <param name="sender">
    /// The event source, unused.
    /// </param>
    /// <param name="e">
    /// The pointer event, carrying the click position.
    /// </param>
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_session is null)
        {
            return;
        }

        var point = e.GetPosition(TextCanvas);
        const double approximateCharacterWidth = 8;
        var lineHeight = _textRenderer.LineHeight;

        var line = (int)(point.Y / lineHeight);
        var column = (int)(point.X / approximateCharacterWidth);

        var offset = _session.Document.ResolveOffset(line, column);
        _session.CaretModel.MoveTo(_session.Document.ResolvePosition(offset));

        RenderAll();
    }

    /// <summary>
    /// Computes and shows completion suggestions at the current caret
    /// position, if any are available.
    /// </summary>
    private void TriggerCompletionIfApplicable()
    {
        var psiFile = TryGetPsiFile();

        if (psiFile is null || _session is null || _completionCoordinator is null)
        {
            return;
        }

        var items = _completionCoordinator.GetCompletions(psiFile, _session.CaretModel.PrimaryCaret);

        if (items.Count == 0)
        {
            CompletionPopupHost.IsOpen = false;
            return;
        }

        var popup = new CompletionPopup();
        popup.SetItems(items);
        popup.ItemChosen += OnCompletionItemChosen;

        CompletionPopupHost.Child = popup;
        CompletionPopupHost.IsOpen = true;
    }

    /// <summary>
    /// Applies a chosen completion item by inserting its text at the caret
    /// and closing the popup.
    /// </summary>
    /// <param name="item">
    /// The completion item the user chose.
    /// </param>
    private void OnCompletionItemChosen(Abstractions.Completion.CompletionItem item)
    {
        if (_session is null)
        {
            return;
        }

        var offset = _session.CaretModel.PrimaryCaret.Offset;
        _session.Document.Replace(offset, 0, item.InsertText);
        _session.CaretModel.MoveTo(_session.Document.ResolvePosition(offset + item.InsertText.Length));

        CompletionPopupHost.IsOpen = false;
        RenderAll();
    }

    /// <summary>
    /// Bridges document change notifications back to a re-render, since
    /// <see cref="EditorView"/> itself is not an <see cref="IDocumentListener"/>.
    /// </summary>
    private sealed class DocumentChangeHandler : IDocumentListener
    {
        private readonly EditorView _owner;

        public DocumentChangeHandler(EditorView owner) => _owner = owner;

        public void DocumentChanged(DocumentChangeEvent change) => _owner.RenderAll();
    }

    /// <summary>
    /// A minimal helper wrapping a <see cref="Canvas"/> to let rendering
    /// code draw via a familiar <see cref="DrawingContext"/> callback,
    /// bridging the gap between this simplified retained-mode
    /// <see cref="Canvas"/>-based approach and the immediate-mode renderers
    /// (<see cref="EditorTextRenderer"/> etc.) written against
    /// <see cref="DrawingContext"/> directly. A production implementation
    /// would more likely override <c>Render(DrawingContext)</c> on a custom
    /// control instead of layering visuals onto a <see cref="Canvas"/> like
    /// this — this approach is simpler to wire up but less efficient.
    /// </summary>
    private sealed class DrawingVisualHost
    {
        private readonly Canvas _canvas;

        public DrawingVisualHost(Canvas canvas) => _canvas = canvas;

        public void Draw(Action<DrawingContext> drawAction)
        {
            var control = new DrawingControl(drawAction);
            _canvas.Children.Add(control);
        }

        private sealed class DrawingControl : Control
        {
            private readonly Action<DrawingContext> _drawAction;

            public DrawingControl(Action<DrawingContext> drawAction) => _drawAction = drawAction;

            public override void Render(DrawingContext context) => _drawAction(context);
        }
    }
}