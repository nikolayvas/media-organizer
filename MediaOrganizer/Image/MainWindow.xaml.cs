using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Image
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();

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

        private void DriveS_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;

            string drive = (string)menuItem.Header;

            FillTree(leftTree, drive);
        }

        private void DriveD_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;

            string drive = (string)menuItem.Header;

            FillTree(rightTree, drive);
        }

        private void FillTree(TreeView root, string drive)
        {
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
        }

        void Folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == null)
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
                            //Foreground = new SolidColorBrush(Colors.Orange)
                        };

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
                            //Foreground = new SolidColorBrush(Colors.Orange)
                        };
                        item.Items.Add(subitem);
                    }
                }
                catch (Exception) { }
            }
        }

        private void CloseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void LeftTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem SelectedItem = leftTree.SelectedItem as TreeViewItem;
            switch (SelectedItem.Tag.ToString())
            {
                case "Solution":
                    leftTree.ContextMenu = leftTree.Resources["SolutionContext"] as System.Windows.Controls.ContextMenu;
                    break;
                default:
                    leftTree.ContextMenu = leftTree.Resources["FolderContext"] as System.Windows.Controls.ContextMenu;
                    break;
            }
        }

        private void AddFilesToFolder_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LeftTree_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
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
    }
}
