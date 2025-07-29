using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using Quicker.Extend;
using System.Windows;
using AutoClicker;

namespace Connector
{
    public partial class MainWindow : Window, IExtensionModule
    {
        public new string Name => "AutoClicker"; // 名称
        public string Version => "1.0.0"; // 版本号
        public string Author => "Anonymity"; // 作者
        public string Description => "连点器"; // 描述
        public bool HasUI => true; // 窗口有UI
        public string[] Dependencies => []; // 依赖项

        public void Initialize()
        {
            InitializeComponent();
            ApplyClipping(); // 应用裁剪
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

        }

        public void Stop()
        {

        }

        /// <summary>
        /// 应用裁剪效果，使子元素不超出圆角边界
        /// </summary>
        private void ApplyClipping()
        {
            // 找到需要裁剪的Border元素
            if (this.FindName("TitleBorder") is Border titleBorder)
            {
                // 监听尺寸变化事件
                titleBorder.SizeChanged += (sender, e) => UpdateBorderClip(titleBorder);
                // 初始应用裁剪
                UpdateBorderClip(titleBorder);
            }
        }

        /// <summary>
        /// 更新Border的裁剪路径
        /// </summary>
        /// <param name="border">需要裁剪的Border</param>
        private void UpdateBorderClip(Border border)
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
    }
}