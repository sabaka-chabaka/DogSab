using Avalonia.Controls;
using Avalonia.Input;
using DogSab.Platform.Editor.Abstractions.Completion;

namespace DogSab.Platform.Editor.Ui.Completion;

/// <summary>
/// The popup list shown while typing, presenting the
/// <see cref="CompletionItem"/> results computed by
/// <see cref="Editor.Completion.CompletionCoordinator"/> and letting the
/// user pick one via mouse click or Enter key.
/// Kept as a plain <see cref="UserControl"/> hosted inside a positioned
/// <see cref="Avalonia.Controls.Primitives.Popup"/> by <see cref="EditorView"/>,
/// rather than being a top-level window itself, so its position stays
/// pinned relative to the caret as the editor scrolls.
/// </summary>
public partial class CompletionPopup : UserControl
{
    /// <summary>
    /// Raised when the user confirms a selection, either by double-clicking
    /// an item or pressing Enter while it is highlighted.
    /// </summary>
    public event Action<CompletionItem>? ItemChosen;

    public CompletionPopup()
    {
        InitializeComponent();

        ItemsList.DoubleTapped += (_, _) => ConfirmSelection();
        ItemsList.KeyDown += OnKeyDown;
    }

    /// <summary>
    /// Populates the popup with a new list of completion items, replacing
    /// any previously shown items, and selects the first item by default.
    /// </summary>
    /// <param name="items">
    /// The completion items to display, expected to already be sorted by
    /// the caller (see <see cref="Editor.Completion.CompletionCoordinator"/>,
    /// which sorts by descending priority before returning).
    /// </param>
    public void SetItems(IReadOnlyList<CompletionItem> items)
    {
        ItemsList.ItemsSource = items;

        if (items.Count > 0)
        {
            ItemsList.SelectedIndex = 0;
        }
    }

    /// <summary>
    /// Handles keyboard navigation within the popup: Enter confirms the
    /// current selection, Escape is left unhandled here so the owning
    /// <see cref="EditorView"/> can close the popup on its own key handler.
    /// </summary>
    /// <param name="sender">
    /// The event source, unused.
    /// </param>
    /// <param name="e">
    /// The key event arguments.
    /// </param>
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ConfirmSelection();
            e.Handled = true;
        }
    }

    /// <summary>
    /// Raises <see cref="ItemChosen"/> for the currently highlighted item,
    /// if any is selected.
    /// </summary>
    private void ConfirmSelection()
    {
        if (ItemsList.SelectedItem is CompletionItem selected)
        {
            ItemChosen?.Invoke(selected);
        }
    }
}