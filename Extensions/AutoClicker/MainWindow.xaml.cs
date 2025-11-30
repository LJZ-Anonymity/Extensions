using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Reflection;
using Quicker.Extend;
using System.Windows;
using AutoClicker;
using System.IO;

namespace Connector
{
    public partial class MainWindow : Window, IExtensionModule
    {
        public new string Name => "AutoClicker"; // 名称
        public string Version => "1.0.0"; // 版本号
        public string Author => "Anonymity"; // 作者
        public string Description => "连点器"; // 描述
        public byte[] IconData => GetIconData(); // 扩展图标
        public bool HasContextMenu => false; // 是否具有右键菜单

        void IExtensionModule.Activate()
        {
            InitializeComponent();
            ApplyClipping(); // 应用裁剪
            Show(); // 显示窗口
            Activate(); // 激活窗口
        }

        /// <summary>
        /// 获取图标字节数组
        /// </summary>
        /// <returns>图标数据</returns>
        private static byte[] GetIconData()
        {
            try
            {
                // 从嵌入资源中读取图标
                Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("AutoClicker.icon.ico");
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
            catch
            {
                return [];
            }
        }

        /// <summary>
        /// 应用裁剪效果，使子元素不超出圆角边界
        /// </summary>
        private void ApplyClipping()
        {
            // 找到需要裁剪的Border元素
            if (this.FindName("TitleBorder") is Border titleBorder)
            {
                titleBorder.SizeChanged += (sender, e) => UpdateBorderClip(titleBorder); // 监听尺寸变化事件
                UpdateBorderClip(titleBorder); // 初始应用裁剪
            }
        }

        /// <summary>
        /// 更新Border的裁剪路径
        /// </summary>
        /// <param name="border">需要裁剪的Border</param>
        private static void UpdateBorderClip(Border border)
        {
            double width = border.ActualWidth;
            double height = border.ActualHeight;
            
            if (width <= 0 || height <= 0) return;

            // 获取圆角设置
            var cornerRadius = border.CornerRadius;
            
            // 针对"5,5,0,0"圆角设置的优化实现
            if (cornerRadius.TopLeft > 0 || cornerRadius.TopRight > 0)
            {
                // 创建自定义路径几何体
                var geometry = new PathGeometry();
                var figure = new PathFigure
                {
                    IsClosed = true,                 // 起始点（左上角圆角起点）
                    StartPoint = new Point(cornerRadius.TopLeft, 0)
                };

                // 上边线（从左到右）
                figure.Segments.Add(new LineSegment(
                    new Point(width - cornerRadius.TopRight, 0), true));
                
                // 右上角圆弧（如果有）
                if (cornerRadius.TopRight > 0)
                {
                    figure.Segments.Add(new ArcSegment(
                        new Point(width, cornerRadius.TopRight),
                        new Size(cornerRadius.TopRight, cornerRadius.TopRight),
                        0, false, SweepDirection.Clockwise, true));
                }
                else
                {
                    figure.Segments.Add(new LineSegment(new Point(width, 0), true));
                }
                
                // 右边线
                figure.Segments.Add(new LineSegment(new Point(width, height), true));
                
                // 下边线
                figure.Segments.Add(new LineSegment(new Point(0, height), true));
                
                // 左边线
                figure.Segments.Add(new LineSegment(
                    new Point(0, cornerRadius.TopLeft), true));
                
                // 左上角圆弧（如果有）
                if (cornerRadius.TopLeft > 0)
                {
                    figure.Segments.Add(new ArcSegment(
                        new Point(cornerRadius.TopLeft, 0),
                        new Size(cornerRadius.TopLeft, cornerRadius.TopLeft),
                        0, false, SweepDirection.Clockwise, true));
                }
                else
                {
                    figure.Segments.Add(new LineSegment(new Point(0, 0), true));
                }
                
                geometry.Figures.Add(figure);
                border.Clip = geometry;
            }
            else
            {
                // 没有圆角，使用矩形裁剪
                var geometry = new RectangleGeometry(new Rect(0, 0, width, height));
                border.Clip = geometry;
            }
        }

        // 鼠标按下允许拖动窗口
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

        public void Deactivate()
        {

        }
    }
}