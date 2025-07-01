using System.Windows.Forms;
using Quicker.Interface;
using System;

namespace Quicker.Examples
{
    public class SampleModule : IExtensionModule
    {
        private Form _moduleWindow;

        // 模块元数据
        public string Name => "扩展名称";
        public string Version => "1.0.0";
        public string Author => "作者名称";
        public string Description => "扩展描述";
        
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
            // 显示窗口
            _moduleWindow.Show();
        }
    }
} 