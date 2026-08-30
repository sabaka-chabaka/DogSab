using Avalonia.Controls;
using Avalonia.Input;
using DogSab.Platform.Vfs.Abstractions.VirtualFile;

namespace DogSab.Platform.Ui.Shell.ProjectView;

/// <summary>
/// A real, working implementation of a Project View tool window: displays
/// the file tree rooted at a given directory and raises
/// <see cref="FileActivated"/> when the user double-clicks a file leaf.
/// The first genuine tool window content the platform has, as opposed to
/// the purely infrastructural <c>IToolWindow</c>/<c>IToolWindowFactory</c>
/// contracts that existed without any real implementation until now.
/// </summary>
public partial class ProjectViewToolWindow : UserControl
{
    /// <summary>
    /// Raised when the user double-clicks a file (not a directory) node in
    /// the tree, signaling intent to open it for editing.
    /// </summary>
    public event Action<IVirtualFile>? FileActivated;

    public ProjectViewToolWindow()
    {
        InitializeComponent();
        FileTree.DoubleTapped += OnDoubleTapped;
    }

    /// <summary>
    /// Populates the tree with the contents of a root directory.
    /// </summary>
    /// <param name="rootDirectory">
    /// The directory to display as the tree's root.
    /// </param>
    public void SetRoot(IVirtualFile rootDirectory)
    {
        var rootNode = BuildNode(rootDirectory);
        FileTree.ItemsSource = new List<FileTreeNode> { rootNode };
    }

    /// <summary>
    /// Recursively builds a tree node for a virtual file, including its
    /// children if it is a directory.
    /// </summary>
    /// <param name="file">
    /// The file or directory to build a node for.
    /// </param>
    /// <returns>
    /// The built tree node.
    /// </returns>
    private FileTreeNode BuildNode(IVirtualFile file)
    {
        var node = new FileTreeNode(file);

        if (file.Type == VirtualFileType.Directory)
        {
            foreach (var child in file.GetChildren())
            {
                node.Children.Add(BuildNode(child));
            }
        }

        return node;
    }

    /// <summary>
    /// Raises <see cref="FileActivated"/> for the double-clicked node, if
    /// it represents a file rather than a directory.
    /// </summary>
    /// <param name="sender">
    /// The event source, unused.
    /// </param>
    /// <param name="e">
    /// The double-tap event, unused beyond confirming the interaction occurred.
    /// </param>
    private void OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (FileTree.SelectedItem is FileTreeNode { File.Type: VirtualFileType.File } node)
        {
            FileActivated?.Invoke(node.File);
        }
    }

    /// <summary>
    /// A single node in the displayed file tree, wrapping a virtual file
    /// with its display name and children for tree-control binding.
    /// </summary>
    public sealed class FileTreeNode
    {
        /// <summary>
        /// The virtual file this node represents.
        /// </summary>
        public IVirtualFile File { get; }

        /// <summary>
        /// The text shown for this node in the tree.
        /// </summary>
        public string Name => File.Name;

        /// <summary>
        /// This node's child nodes, populated only for directories.
        /// </summary>
        public List<FileTreeNode> Children { get; } = new();

        /// <summary>
        /// Creates a new tree node wrapping a virtual file.
        /// </summary>
        /// <param name="file">
        /// The virtual file this node represents.
        /// </param>
        public FileTreeNode(IVirtualFile file)
        {
            File = file;
        }
    }
}