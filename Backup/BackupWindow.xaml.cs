using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Media;
using Quicker.Interface;
using Quicker.Managers;
using System.Windows;
using System.IO;
using Backup;

namespace Backup
{
    public partial class BackupWindow : Window, IExtensionModule
    {
        private readonly List<FilesDatabase.FileData> selectedFiles = []; // 选中的文件
        private readonly FilesDatabase db = new(); // 文件数据库

        // 使用 LibraryImport 特性来声明外部函数，编译时生成更高效的封送代码
        [LibraryImport("FileCopy/FileCopy.dll")]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        private static partial void CopyFiles(
            [MarshalAs(UnmanagedType.LPStr)] string sourcePaths,
            [MarshalAs(UnmanagedType.LPStr)] string targetPath,
            [MarshalAs(UnmanagedType.LPStr)] string style,
            [MarshalAs(UnmanagedType.Bool)] bool cleanTargetFolder); // 外部函数声明

        // 实现 IExtensionModule 接口的 Initialize 方法
        public void Initialize()
        {
            this.ShowWindow(); // 在初始化时直接显示窗口
        }

        // 实现 IExtensionModule 接口的 ShowWindow 方法
        public void ShowWindow()
        {
            this.Show(); // 显示窗口
            this.Activate(); // 激活窗口
        }

        public BackupWindow()
        {
            InitializeComponent();
        }

        // 加载要备份的文件列表
        private void BackupWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Refresh(); // 刷新文件列表
        }

        // 刷新文件列表
        public void Refresh()
        {
            MainStackPanel.Children.Clear(); // 清空窗口内容
            var files = db.GetAllFileData(); // 获取所有文件数据
            foreach (var file in files)
            {
                CreateFileItem(file); // 创建文件项
            }
        }

        /// <summary>
        /// 创建文件项
        /// </summary>
        /// <param name="file"> 文件数据 </param>
        private void CreateFileItem(FilesDatabase.FileData file)
        {
            Grid grid = new()
            {
                Height = 24,
                Margin = new Thickness(0, 5, 0, 0)
            };
            MainStackPanel.Children.Add(grid); // 添加文件项

            CheckBox checkbox = new()
            {
                Tag = file.FileID,
                Content = file.FileName,
                Margin = new Thickness(10, 0, 158, 0)
            };
            grid.Children.Add(checkbox); // 添加复选框

            Button deleteButton = new()
            {
                Width = 64,
                Content = "删除",
                Tag = file.FileID,
                Margin = new Thickness(497, 0, 0, 0),
                Style = (Style)FindResource("WhiteButton"),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            deleteButton.Click += DeleteButton_Click; // 绑定删除按钮事件
            grid.Children.Add(deleteButton); // 添加删除按钮

            Button editButton = new()
            {
                Width = 64,
                Content = "编辑",
                Tag = file.FileID,
                Margin = new Thickness(428, 0, 0, 0),
                Style = (Style)FindResource("WhiteButton"),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            editButton.Click += EditButton_Click; // 绑定编辑按钮事件
            grid.Children.Add(editButton); // 添加编辑按钮
        }

        // 备份选中的文件
        private async void BackupButton_Click(object sender, RoutedEventArgs e)
        {
            var checkboxes = FindVisualChildren<CheckBox>(scrollViewer); // 获取所有复选框
            selectedFiles.Clear(); // 清空选中的文件
            foreach (var checkbox in checkboxes) // 遍历所有复选框
            {
                if (checkbox.IsChecked != true) continue; // 跳过未选中的文件
                if (int.TryParse(checkbox.Tag.ToString(), out int fileID)) // 尝试获取文件ID
                {
                    var fileData = db.GetFileData(fileID); // 获取文件数据
                    if (fileData == null) return; // 文件不存在
                    selectedFiles.Add(fileData); // 添加选中的文件
                }
            }

            if (selectedFiles.Count == 0)
            {
                using var toast = new ToastManager(); // 创建 Toast 管理器
                toast.Show("没有选中任何文件！", "Error"); // 显示 Toast 通知
                return; // 退出
            }

            TipLabel.Content = "备份中，请勿关闭窗口"; // 显示备份提示
            BackupButton.IsEnabled = false; // 禁用备份按钮
            WindowState = WindowState.Minimized; // 窗口最小化

            try
            {
                await BackupFilesAsync(); // 备份文件
            }
            catch (Exception ex)
            {
                using var toast = new ToastManager(); // 创建 Toast 管理器
                toast.Show($"备份失败: {ex.Message}", "Error"); // 显示 Toast 通知
            }
            finally
            {
                using var toast = new ToastManager(); // 创建 Toast 管理器
                toast.Show("备份完成！", "Success"); // 显示 Toast 通知
                TipLabel.Content = "备份完成！"; // 显示备份完成信息
                BackupButton.IsEnabled = true; // 启用备份按钮
                if (ExitAfterBackupCheckBox.IsChecked == true) this.Close(); // 关闭窗口
                WindowState = WindowState.Normal; // 窗口恢复
            }
        }

        // 备份文件
        private async Task BackupFilesAsync()
        {
            foreach (var file in selectedFiles)
            {
                try
                {
                    Directory.CreateDirectory(file.TargetPath); // 创建目标文件夹
                    string sourcePaths = string.Join("\n", file.SourcePath.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries));
                    await Task.Run(() => CopyFiles(sourcePaths, file.TargetPath, file.Style, file.CleanTargetFloder)); // 调用文件复制库
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        using var toast = new ToastManager(); // 创建 Toast 管理器
                        toast.Show($"备份文件 {file.FileName} 失败: {ex.Message}", "Error"); // 显示 Toast 通知
                    });
                }
            }
        }

        // 删除文件
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender; // 获取按钮
            int fileID = (int)button.Tag; // 获取文件ID
            db.DeleteFileData(fileID); // 删除文件数据
            MainStackPanel.Children.Remove(button.Parent as Grid); // 删除文件项
        }

        // 编辑文件
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender; // 获取按钮
            int fileID = (int)button.Tag; // 获取文件ID
            AddBackupItemWindow addwindow = new(fileID.ToString()); // 创建编辑窗口
            addwindow.ShowDialog(); // 显示窗口
        }

        // 关闭窗口时释放数据库资源
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e); // 关闭窗口时释放数据库资源
            MainGrid.Children.Clear(); // 清空窗口内容
            GC.Collect(); // 回收内存
        }

        // 递归查找子控件
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) yield break; // 终止条件
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i); // 枚举子节点
                if (child is T t)
                    yield return t; // 找到符合条件的子节点
                foreach (var descendant in FindVisualChildren<T>(child))
                {
                    yield return descendant; // 递归枚举子孙节点
                }
            }
        }

        // 打开添加窗口
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            Popup.IsOpen = true; // 打开弹出窗口
        }

        // 关闭添加窗口
        private void AddFileButton_Click(object sender, RoutedEventArgs e)
        {
            AddBackupItemWindow addwindow = new("File"); // 创建添加文件窗口
            addwindow.ShowDialog(); // 显示窗口
        }

        // 打开添加文件夹窗口
        private void AddFolderButton_Click(object sender, RoutedEventArgs e)
        {
            AddBackupItemWindow addwindow = new("Folder"); // 创建添加文件夹窗口
            addwindow.ShowDialog(); // 显示窗口
        }

        // 同步滚动条
        private void VerticalScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            scrollViewer.ScrollToVerticalOffset(VerticalScrollBar.Value); // 同步滚动条
        }
        private void ListViewScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (scrollViewer != null)
            {
                VerticalScrollBar.Maximum = scrollViewer.ScrollableHeight; // 设置滚动条最大值
                VerticalScrollBar.ViewportSize = scrollViewer.ViewportHeight; // 设置滚动条可视大小
                VerticalScrollBar.Value = scrollViewer.VerticalOffset; // 设置滚动条当前值
            }
        }
    }
}