using Quicker.Windows.Menus;
using Quicker.Managers;
using System.Windows;

namespace Quicker.Examples
{
    /// <summary>
    /// SampleContextMenu.xaml 的交互逻辑
    /// 这是一个示例扩展菜单，演示如何创建带右键菜单的扩展
    /// </summary>
    public partial class SampleContextMenu : BaseMenuWindow
    {
        public SampleContextMenu(int buttonId, string tableName, Window sourceWindow)
        {
            InitializeComponent();
            base.SetWindowTopmost(); // 设置窗口置顶
        }

        // 重写基类的窗口加载方法
        protected override void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            base.OnWindowLoaded(sender, e); // 调用基类方法处理动画
            base.SetWindowPositionNearMouse(); // 设置窗口位置
        }

        // 重写基类的失焦处理方法
        protected override void HandleDeactivated()
        {
            using var windowManager = new WindowManager(); // 创建窗口管理器
            windowManager.SetMainWindowFocused(); // 让MainWindow获得焦点
            // 使用基类的动画隐藏方法
            base.HideWithAnimation();
            _ = base.CloseMenuAsync(); // 延时关闭窗口（不等待）
            // 调用基类方法以触发ClosingOrHiding事件
            base.HandleDeactivated();
        }
    }
}
