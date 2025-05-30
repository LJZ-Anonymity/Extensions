using Quicker.Interface;
using System;
using System.Windows.Forms;

namespace Quicker.Examples
{
    public class SampleModule : IExtensionModule
    {
        private Form _moduleWindow;

        // 模块元数据
        public string Name => "SampleModule";
        public string Version => "1.0.0";
        public string Author => "Your Name";
        public string Description => "这是一个示例模块，展示如何实现IExtensionModule接口";
        
        // 依赖关系
        public string[] Dependencies => new string[0]; // 无依赖
        
        // UI相关
        public bool HasUI => true;
        
        public void Initialize()
        {
            // 在这里进行模块的初始化工作
            Console.WriteLine($"{Name} 正在初始化...");
            
            // 可以加载配置、准备资源等
        }
        
        public void Start()
        {
            // 在这里启动模块的功能
            Console.WriteLine($"{Name} 已启动");
            
            // 可以启动后台任务、注册事件处理程序等
        }
        
        public void Stop()
        {
            // 在这里清理资源、停止任务
            Console.WriteLine($"{Name} 正在停止...");
            
            // 关闭窗口
            _moduleWindow?.Close();
            _moduleWindow = null;
        }
        
        public void ShowWindow()
        {
            // 如果窗口已经创建，则显示它
            if (_moduleWindow != null)
            {
                _moduleWindow.Show();
                _moduleWindow.BringToFront();
                return;
            }
            
            // 创建一个简单的窗口
            _moduleWindow = new Form
            {
                Text = Name,
                Width = 400,
                Height = 300
            };
            
            // 添加一些控件
            var label = new Label
            {
                Text = Description,
                AutoSize = true,
                Location = new System.Drawing.Point(10, 10)
            };
            
            var versionLabel = new Label
            {
                Text = $"版本: {Version}",
                AutoSize = true,
                Location = new System.Drawing.Point(10, 40)
            };
            
            var authorLabel = new Label
            {
                Text = $"作者: {Author}",
                AutoSize = true,
                Location = new System.Drawing.Point(10, 70)
            };
            
            _moduleWindow.Controls.Add(label);
            _moduleWindow.Controls.Add(versionLabel);
            _moduleWindow.Controls.Add(authorLabel);
            
            // 显示窗口
            _moduleWindow.Show();
        }
    }
} 