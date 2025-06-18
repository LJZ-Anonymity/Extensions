using Quicker.Extend;
using System.Windows;
using AutoClicker;

namespace Connector
{
    public partial class MainWindow : Window, IExtensionModule
    {
        public string Version => "1.0.0"; // 版本号
        public string Author => "Anonymity"; // 作者
        public string Description => "连点器"; // 描述
        public bool HasUI => true; // 窗口有UI
        public string[] Dependencies => []; // 依赖项

        public MainWindow()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            Show(); // 显示窗口
            Activate(); // 激活窗口
        }

        public void ShowWindow()
        {
            Show(); // 显示窗口
            Activate(); // 激活窗口
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        // 鼠标按下允许拖动窗口
        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove(); //  允许拖动
        }

        // 击按钮隐藏窗体
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed; // 隐藏窗口
        }

        // 点击按钮最小化窗口
        private void MinimumButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized; // 最小化窗口
        }

        // 点击按钮打开设置窗口
        private void OpenSettingWindow_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow settingWindow = new() { Owner = this }; // 创建设置窗口
            settingWindow.ShowDialog(); // 显示设置窗口
        }
    }
}