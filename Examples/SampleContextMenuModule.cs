using System.Reflection;
using System.IO;
using Quicker.Extend;
using Quicker.Managers;

namespace Quicker.Examples
{
    /// <summary>
    /// 示例扩展模块 - 演示如何创建带右键菜单的扩展
    /// </summary>
    public class SampleContextMenuModule : IExtensionModule
    {
        // 模块元数据
        public string Name => "示例右键菜单扩展";
        public string Version => "1.0.0";
        public string Author => "Quicker Team";
        public string Description => "演示如何创建带右键菜单的扩展模块";
        public byte[] IconData => GetIconData();

        // UI相关
        public bool HasUI => false; // 这个扩展没有主窗口UI

        // 右键菜单相关
        public bool HasContextMenu => true; // 支持右键菜单

        // 依赖关系
        public string[] Dependencies => []; // 无依赖

        /// <summary>
        /// 获取图标字节数组
        /// </summary>
        /// <returns>图标数据</returns>
        private static byte[] GetIconData()
        {
            try
            {
                // 从嵌入资源中读取图标
                Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Quicker.Examples.icon.ico");
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

        // 生命周期方法
        public void Initialize()
        {
            // 在这里进行模块的初始化工作
            System.Diagnostics.Debug.WriteLine($"{Name} 正在初始化...");
        }

        public void Start()
        {
            // 在这里启动模块的功能
            System.Diagnostics.Debug.WriteLine($"{Name} 已启动");
        }

        public void Stop()
        {
            // 在这里清理资源、停止任务
            System.Diagnostics.Debug.WriteLine($"{Name} 正在停止...");
        }

        public void ShowWindow()
        {
            // 由于HasUI为false，此方法可以为空
            // 如果需要显示主窗口，可以在这里实现
        }
    }
}
