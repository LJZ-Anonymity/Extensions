using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Windows.Controls;
using System.Windows.Media;
using System.Reflection;
using Quicker.Managers;
using Quicker.Extend;
using System.Windows;
using System.IO;

namespace Backup
{
    public partial class BackupWindow : Window, IExtensionModule
    {
        public new string Name => "Backup"; // 名称
        public string Version => "1.0.0"; // 版本
        public string Author => "Anonymity"; // 作者
        public string Description => "备份文件的扩展模块"; // 描述信息
        public byte[] IconData => GetIconData(); // 扩展图标
        public bool HasUI => true; // 是否有UI
        public bool HasContextMenu => false; // 是否具有右键菜单
        public string[] Dependencies => []; // 依赖模块

        private readonly List<FilesDatabase.FileData> selectedFiles = []; // 选中的文件
        private readonly FilesDatabase db = new(); // 文件数据库

        // 使用 LibraryImport 特性来声明外部函数，编译时生成更高效的封送代码
        [LibraryImport("FileCopy.dll", EntryPoint = "CopyFiles")]
        public static partial void CopyFiles(
            [MarshalAs(UnmanagedType.LPStr)] string sourcePaths,
            [MarshalAs(UnmanagedType.LPStr)] string targetPath,
            [MarshalAs(UnmanagedType.LPStr)] string style,
            [MarshalAs(UnmanagedType.Bool)] bool cleanTargetFolder); // 文件复制函数

        [LibraryImport("kernel32.dll", SetLastError = true, EntryPoint = "SetDllDirectoryW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool SetDllDirectory([MarshalAs(UnmanagedType.LPWStr)] string lpPathName); // 设置 DLL 搜索目录

        public void Start()
        {
            Refresh(); // 加载初始数据
            if (Visibility != Visibility.Visible)
            {
                ShowWindow(); // 显示窗口
            }
        }

        // 实现 IExtensionModule 接口的 Initialize 方法
        public void Initialize()
        {
            ShowWindow(); // 显示窗口
        }

        // 实现 IExtensionModule 接口的 ShowWindow 方法
        public void ShowWindow()
        {
            Show(); // 显示窗口
            Activate(); // 激活窗口
        }

        /// <summary>
        /// 获取图标字节数组
        /// </summary>
        /// <returns>图标数据</returns>
        private static byte[] GetIconData()
        {
            // 从嵌入资源中读取图标
            Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Backup.icon.ico");
            if (stream != null)
            {
                using (stream)
                {
                    byte[] iconData = new byte[stream.Length];
                    stream.Read(iconData, 0, iconData.Length);
                    return iconData;
                }
            }
            return []; // 如果找不到资源，返回空数组
        }

        public BackupWindow()
        {
            InitializeComponent(); // 初始化组件
            try
            {
                string? extDir = Path.GetDirectoryName(typeof(BackupWindow).Assembly.Location);
                System.Diagnostics.Debug.WriteLine($"扩展目录: {extDir}");
                if (!string.IsNullOrEmpty(extDir))
                {
                    string fileCopyPath = Path.Combine(extDir, "FileCopy.dll"); // 检查FileCopy.dll是否存在
                    if (!File.Exists(fileCopyPath))
                    {
                        ShowToast($"找不到FileCopy.dll文件: {fileCopyPath}", ToastType.Error);
                        return;
                    }
                    bool result = SetDllDirectory(extDir); // 设置 DLL 搜索目录
                }
            }
            catch (Exception ex)
            {
                ShowToast($"初始化DLL时出错: {ex.Message}", ToastType.Error);
            }
        }

        // 加载要备份的文件列表
        private void BackupWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Refresh(); // 刷新文件列表
            UpdateBackupButtonState(); // 初始化备份按钮状态
        }

        // 刷新文件列表
        public void Refresh()
        {
            MainStackPanel.Children.Clear(); // 清空主网格
            var files = db.GetAllFileData(); // 获取所有文件数据
            foreach (var file in files)
            {
                CreateFileItem(file); // 创建文件项
            }
            UpdateBackupButtonState(); // 更新备份按钮状态
        }

        /// <summary>
        /// 创建文件项
        /// </summary>
        /// <param name="file">文件数据</param>
        private void CreateFileItem(FilesDatabase.FileData file)
        {
            // 创建主网格
            var grid = new Grid() { Style = (Style)FindResource("FileItemGrid") }; // 创建主网格
            MainStackPanel.Children.Add(grid); // 添加主网格

            // 添加复选框
            var checkbox = CreateCheckbox(file); // 创建复选框
            grid.Children.Add(checkbox); // 添加复选框

            // 添加删除按钮
            var deleteButton = CreateButton("删除", file.FileID, 497, DeleteButton_Click); // 创建删除按钮
            grid.Children.Add(deleteButton); // 添加删除按钮

            // 添加编辑按钮
            var editButton = CreateButton("编辑", file.FileID, 428, EditButton_Click); // 创建编辑按钮
            grid.Children.Add(editButton); // 添加编辑按钮
        }

        /// <summary>
        /// 创建文件项的复选框
        /// </summary>
        /// <param name="file">文件数据</param>
        /// <returns>复选框</returns>
        private CheckBox CreateCheckbox(FilesDatabase.FileData file)
        {
            var checkbox = new CheckBox
            {
                Tag = file.FileID, // 设置复选框标签
                Content = file.FileName, // 设置复选框内容
                Margin = new Thickness(10, 0, 158, 0) // 设置复选框边距
            }; // 返回复选框

            checkbox.Click += Checkbox_Click; // 添加点击事件
            return checkbox;
        }

        /// <summary>
        /// 创建文件项的按钮
        /// </summary>
        /// <param name="content">按钮内容</param>
        /// <param name="fileId">文件ID</param>
        /// <param name="leftMargin">左侧边距</param>
        /// <param name="clickHandler">点击事件</param>
        /// <returns>按钮</returns>
        private Button CreateButton(string content, int fileId, double leftMargin, RoutedEventHandler clickHandler)
        {
            var button = new Button
            {
                Width = 64, // 设置按钮宽度
                Content = content, // 设置按钮内容
                Tag = fileId, // 设置按钮标签
                Margin = new Thickness(leftMargin, 0, 0, 0), // 设置按钮边距
                Style = (Style)FindResource("WhiteButton"), // 设置按钮样式
                HorizontalAlignment = HorizontalAlignment.Left // 设置按钮水平对齐方式
            }; // 返回按钮
            button.Click += clickHandler; // 添加点击事件
            return button; // 返回按钮
        }

        /// <summary>
        /// 复选框点击事件处理
        /// </summary>
        /// <param name="sender">事件源</param>
        /// <param name="e">事件参数</param>
        private void Checkbox_Click(object sender, RoutedEventArgs e)
        {
            UpdateBackupButtonState();
        }

        /// <summary>
        /// 更新备份按钮状态
        /// </summary>
        private void UpdateBackupButtonState()
        {
            var checkboxes = FindVisualChildren<CheckBox>(scrollViewer);
            bool hasCheckedItems = checkboxes.Any(cb => cb.IsChecked == true);
            BackupButton.IsEnabled = hasCheckedItems;
        }

        // 备份选中的文件
        private async void BackupButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CollectSelectedFiles())
            {
                return; // 如果未收集选中的文件，则返回
            }

            PrepareForBackup(); // 准备备份操作

            try
            {
                await BackupFilesAsync(); // 备份选中的文件
            }
            catch (Exception ex)
            {
                ShowToast($"备份失败: {ex.Message}", ToastType.Error); // 显示备份失败通知
            }
            finally
            {
                CompleteBackup(); // 完成备份操作
            }
        }

        /// <summary>
        /// 收集选中的文件
        /// </summary>
        /// <returns>是否收集到选中的文件</returns>
        private bool CollectSelectedFiles()
        {
            var checkboxes = FindVisualChildren<CheckBox>(scrollViewer); // 查找所有复选框
            selectedFiles.Clear(); // 清空选中的文件列表
            foreach (var checkbox in checkboxes)
            {
                if (checkbox.IsChecked != true) 
                    continue; // 如果复选框未选中，则跳过
                
                if (int.TryParse(checkbox.Tag?.ToString(), out int fileID))
                {
                    var fileData = db.GetFileData(fileID); // 获取文件数据
                    if (fileData != null) 
                    {
                        selectedFiles.Add(fileData); // 添加文件数据到选中的文件列表
                    }
                }
            }
            return true; // 返回true
        }

        // 准备备份操作
        private void PrepareForBackup()
        {
            TipLabel.Content = "备份中，请勿关闭窗口"; // 设置提示标签内容
            BackupButton.IsEnabled = false; // 禁用备份按钮
            WindowState = WindowState.Minimized; // 最小化窗口
        }

        // 完成备份操作
        private void CompleteBackup()
        {
            ShowToast("备份完成！", ToastType.Success); // 显示备份完成通知
            TipLabel.Content = "备份完成！"; // 设置提示标签内容
            BackupButton.IsEnabled = true; // 启用备份按钮
            
            if (ExitAfterBackupCheckBox.IsChecked == true)
                this.Close(); // 关闭窗口
                
            WindowState = WindowState.Normal; // 恢复窗口状态
        }

        // 显示Toast通知
        [SupportedOSPlatform("windows")]
        private static void ShowToast(string message, ToastType type)
        {
            using var toast = new ToastManager(); // 创建Toast管理器
            toast.Show(message, type); // 显示Toast通知
        }

        // 备份文件
        private async Task BackupFilesAsync()
        {
            foreach (var file in selectedFiles)
            {
                await BackupSingleFileAsync(file); // 备份单个文件
            }
        }

        // 备份单个文件
        private async Task BackupSingleFileAsync(FilesDatabase.FileData file)
        {
            try
            {
                Directory.CreateDirectory(file.TargetPath); // 创建目标文件夹
                string sourcePaths = PrepareSourcePaths(file.SourcePath); // 准备源路径字符串
                await Task.Run(() => CopyFiles(sourcePaths, file.TargetPath, file.Style, file.CleanTargetFloder)); // 异步执行文件复制
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                    ShowToast($"备份文件 {file.FileName} 失败: {ex.Message}", ToastType.Error)
                );
            }
        }

        /// <summary>
        /// 准备源路径字符串
        /// </summary>
        /// <param name="sourcePath">源路径</param>
        /// <returns>源路径字符串</returns>
        private static string PrepareSourcePaths(string sourcePath)
        {
            return string.Join("\n", sourcePath.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)); // 返回源路径字符串
        }

        // 删除文件
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender; // 获取按钮
            int fileID = (int)button.Tag; // 获取文件ID
            db.DeleteFileData(fileID); // 删除文件数据
            if (button.Parent is Grid grid) // 获取父Grid并释放资源
            {
                // 查找并释放CheckBox的事件绑定
                foreach (var child in grid.Children)
                {
                    if (child is CheckBox checkbox)
                    {
                        checkbox.Click -= Checkbox_Click; // 解除事件绑定
                        break;
                    }
                }

                // 释放按钮的事件绑定
                button.Click -= DeleteButton_Click;
                MainStackPanel.Children.Remove(grid); // 删除文件项
            }

            UpdateBackupButtonState(); // 更新备份按钮状态
        }

        // 编辑文件
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender; // 获取按钮
            int fileID = (int)button.Tag; // 获取文件ID
            AddBackupItemWindow addwindow = new(fileID.ToString()) { Owner = this }; // 创建编辑窗口
            addwindow.ShowDialog(); // 显示窗口
        }

        // 关闭窗口时释放数据库资源
        protected override void OnClosed(EventArgs e)
        {
            // 释放所有CheckBox的事件绑定
            var checkboxes = FindVisualChildren<CheckBox>(scrollViewer);
            foreach (var checkbox in checkboxes)
            {
                checkbox.Click -= Checkbox_Click;
            }
            
            // 释放所有按钮的事件绑定
            var buttons = FindVisualChildren<Button>(scrollViewer);
            foreach (var button in buttons)
            {
                if (button.Tag != null && int.TryParse(button.Tag.ToString(), out _))
                {
                    button.Click -= DeleteButton_Click;
                    button.Click -= EditButton_Click;
                }
            }
            
            base.OnClosed(e); // 调用基类的OnClosed方法
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
            AddBackupItemWindow addwindow = new("File") { Owner = this }; // 创建添加文件窗口
            addwindow.ShowDialog(); // 显示窗口
        }

        // 打开添加文件夹窗口
        private void AddFolderButton_Click(object sender, RoutedEventArgs e)
        {
            AddBackupItemWindow addwindow = new("Folder") { Owner = this }; // 创建添加文件夹窗口
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

        // 停止模块
        public void Stop()
        {
            if (this.IsVisible) // 如果窗口可见
            {
                this.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        this.Close(); // 关闭窗口
                    }
                    catch { }
                }); // 在UI线程中执行
            }
            MainGrid?.Children.Clear(); // 释放主界面资源
            GC.Collect(); // 回收内存
        }
    }
}