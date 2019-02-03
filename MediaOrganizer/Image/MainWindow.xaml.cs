using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Image.Engine;
using System.Threading.Tasks;

namespace Image
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _cancelSource;
        private string _destinationDrive;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void DriveS_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;

            string drive = (string)menuItem.Header;

            FillTree(leftTree, drive, true);
        }

        private void DriveD_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;

            string drive = (string)menuItem.Header;

            _destinationDrive = drive;
            FillTree(rightTree, drive, false);
        }

        private void FillTree(TreeView root, string drive, bool expandRoot)
        {
            root.Items.Clear();
            TreeViewItem item = new TreeViewItem
            {
                Header = drive,
                Tag = new TreeNode()
                {
                    Type = NodeDataType.Drive,
                    Path = drive
                },
                FontWeight = FontWeights.Normal
            };

            item.Items.Add(null);
            item.Expanded += new RoutedEventHandler(Folder_Expanded);
            root.Items.Add(item);

            item.IsExpanded = expandRoot;
        }

        void Folder_Expanded(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            TreeViewItem item = (TreeViewItem)sender;
            ExpandFolder(item);
        }

        private void ExpandFolder(TreeViewItem item)
        {
            item.Items.Clear();
            try
            {
                foreach (string s in Directory.GetDirectories(((TreeNode)item.Tag).Path))
                {
                    TreeViewItem subitem = new TreeViewItem
                    {
                        Header = s.Substring(s.LastIndexOf("\\") + 1),
                        Tag = new TreeNode()
                        {
                            Type = NodeDataType.Folder,
                            Path = s
                        },
                        FontWeight = FontWeights.Normal,
                    };

                    if (ThumbMarker.HasThumpMarker(s))
                    {
                        subitem.Foreground = new SolidColorBrush(Colors.Orange);
                    }

                    subitem.Items.Add(null);
                    subitem.Expanded += new RoutedEventHandler(Folder_Expanded);
                    item.Items.Add(subitem);
                }
                foreach (string f in Directory.GetFiles(((TreeNode)item.Tag).Path))
                {
                    TreeViewItem subitem = new TreeViewItem
                    {
                        Header = Path.GetFileName(f),
                        Tag = new TreeNode()
                        {
                            Type = NodeDataType.File,
                            Path = f
                        },
                        FontWeight = FontWeights.Normal,
                    };

                    item.Items.Add(subitem);
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Error(ex);
            }
        }

        private void CloseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void LeftTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(sender is TreeView tv)
            {
                if(tv.SelectedItem is TreeViewItem selectedItem)
                {
                    if (selectedItem.Tag is TreeNode tag)
                    {
                        switch (tag.Type)
                        {
                            case NodeDataType.Folder:
                                var contextMenu = leftTree.Resources["sourceTreeContext"] as ContextMenu;

                                if(ThumbMarker.HasThumpMarker(tag.Path))
                                {
                                    (contextMenu.Items[2] as MenuItem).Visibility = Visibility.Collapsed;
                                    (contextMenu.Items[3] as MenuItem).Visibility = Visibility.Visible;
                                }
                                else
                                {
                                    (contextMenu.Items[2] as MenuItem).Visibility = Visibility.Visible;
                                    (contextMenu.Items[3] as MenuItem).Visibility = Visibility.Collapsed;
                                }

                                leftTree.ContextMenu = contextMenu;

                                break;
                            default:
                                leftTree.ContextMenu = null;
                                break;
                        }
                    }
                }
            }
        }

        private void rightTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (sender is TreeView tv)
            {
                if (tv.SelectedItem is TreeViewItem selectedItem)
                {
                    if (selectedItem.Tag is TreeNode tag)
                    {
                        switch (tag.Type)
                        {
                            case NodeDataType.Folder:
                            case NodeDataType.Drive:
                                var contextMenu = rightTree.Resources["targetTreeContext"] as ContextMenu;

                                rightTree.ContextMenu = contextMenu;

                                break;
                            default:
                                rightTree.ContextMenu = null;
                                break;
                        }
                    }
                }
            }
        }

        private void Tree_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        private static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }

        private async void Copy_Click(object sender, RoutedEventArgs e)
        {
            if(rightTree.Items.Count == 0)
            {
                MessageBox.Show("You need first to select the destination drive!",
                                "Info",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var item = GetSelectedTreeViewItem(sender);
            if (item != null)
            {
                if (item.Tag is TreeNode tag)
                {
                    Log.Instance.Info($"Folder, {tag.Path} was clicked!");

                    try
                    {
                        BlockUI(true);
                        _cancelSource = new CancellationTokenSource();
                        var taskRes = FolderProcessor.ProcessFolder(tag.Path, _destinationDrive, SetProgress, _cancelSource.Token);

                        await taskRes.ContinueWith(_ => _cancelSource.Dispose());
                        if (taskRes.Result.Errors > 0)
                        {
                            MessageBox.Show($"Operation completed with {taskRes.Result} errors! For details check the log file!",
                                            "Warning",
                                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            if(!taskRes.IsCanceled)
                            {
                                ThumbMarker.PutThumbMarker(tag.Path, true);
                                ExpandFolder(item.Parent as TreeViewItem);
                            }
                        }
                    }
                    finally
                    {
                        BlockUI(false);
                        _cancelSource = null;
                    }
                }
            }
        }

        private void Mark_Click(object sender, RoutedEventArgs e)
        {
            var item = GetSelectedTreeViewItem(sender);
            if (item != null)
            {
                if (item.Tag is TreeNode tag)
                {
                    ThumbMarker.PutThumbMarker(tag.Path, true);
                    ExpandFolder(item.Parent as TreeViewItem);
                }
            }
        }

        private void UnMark_Click(object sender, RoutedEventArgs e)
        {
            var item = GetSelectedTreeViewItem(sender);
            if (item != null)
            {
                if (item.Tag is TreeNode tag)
                {
                    ThumbMarker.DelThumbMarker(tag.Path, true);
                    ExpandFolder(item.Parent as TreeViewItem);
                }
            }
        }

        private TreeViewItem GetSelectedTreeViewItem(object sender)
        {
            if(sender is MenuItem mi)
            {
                if (mi.CommandParameter is ContextMenu cm)
                {
                    var tv = cm.PlacementTarget as TreeView;

                    if (tv.SelectedItem is TreeViewItem item)
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        private void BlockUI(bool yes)
        {
            sourceDriveItem.IsEnabled = !yes;
            destinationDriveItem.IsEnabled = !yes;
            (leftTree.Resources["sourceTreeContext"] as ContextMenu).Visibility = yes ? Visibility.Hidden : Visibility.Visible;
            (rightTree.Resources["targetTreeContext"] as ContextMenu).Visibility = yes ? Visibility.Hidden : Visibility.Visible;

            if (yes)
            {
                progressBar.Value = 0;
                activityGif.Visibility = Visibility.Visible;
                cancelCopy.Visibility = Visibility.Visible;
            }
            else
            {
                cancelCopy.Visibility = Visibility.Hidden;
                activityGif.Visibility = Visibility.Hidden;
            }
        }

        private void SetProgress(string inPorgressFile, int progress)
        {
            progressBar.Value = progress;
            inProgress.Text = inPorgressFile;
        }

        private void MenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            sourceDriveItem.Items.Clear();
            destinationDriveItem.Items.Clear();

            foreach (DriveInfo d in allDrives)
            {
                MenuItem driveS = new MenuItem()
                {
                    Header = d.Name,
                };
                driveS.Click += DriveS_Click;

                sourceDriveItem.Items.Add(driveS);

                MenuItem driveD = new MenuItem()
                {
                    Header = d.Name,
                };
                driveD.Click += DriveD_Click;

                destinationDriveItem.Items.Add(driveD);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (CloseConfirm())
            {
                _cancelSource.Cancel();
            }
        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (CloseConfirm())
            {
                Close();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!CloseConfirm())
            {
                e.Cancel = true;
            }
        }

        private bool CloseConfirm()
        {
            if (_cancelSource != null && !_cancelSource.IsCancellationRequested)
            {
                var res = MessageBox.Show("Do you really want to cancel running process?",
                                "Warning",
                                MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (res != MessageBoxResult.Yes)
                {
                    return false;
                }
            }

            return true;
        }

        private async void RemoveDuplicated_Click(object sender, RoutedEventArgs e)
        {
            var item = GetSelectedTreeViewItem(sender);
            if (item != null)
            {
                if (item.Tag is TreeNode tag)
                {
                    Log.Instance.Info($"Remove duplications for folder {tag.Path} started!");

                    try
                    {
                        BlockUI(true);
                        _cancelSource = new CancellationTokenSource();

                        await Task.Run(() => DuplicatedFilesRemoval.RemoveDuplicatedFilesPerFolder(tag.Path, true, SetProgress, _cancelSource.Token));

                        ExpandFolder(item.Parent as TreeViewItem);
                    }
                    finally
                    {
                        BlockUI(false);
                        _cancelSource.Dispose();
                        _cancelSource = null;
                    }
                }
            }
        }
    }
}
