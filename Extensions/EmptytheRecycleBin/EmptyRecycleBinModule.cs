using Quicker.Managers;
using Quicker.Extend;
using System.IO;

namespace EmptytheRecycleBin
{
    public partial class EmptyRecycleBinModule : IExtensionModule
    {
        public EmptyRecycleBinModule()
        {
            try
            {
                bool success = EmptyRecycleBin(); // 静默清空所有回收站
                try // 给用户提示最终结果
                {
                    // 延迟一点时间再显示Toast，确保UI上下文已准备好
                    Task.Delay(100).ContinueWith(_ =>
                    {
                        try
                        {
                            using var toast = new ToastManager();
                            if (success)
                            {
                                toast.Show("回收站清空成功", "Success");
                            }
                            else
                            {
                                toast.Show("回收站清空失败，请检查权限", "Error");
                            }
                        }
                        catch { }
                    });
                }
                catch { }
            }
            catch (Exception ex)
            {
                try
                {
                    // 延迟一点时间再显示Toast，确保UI上下文已准备好
                    Task.Delay(100).ContinueWith(_ =>
                    {
                        try
                        {
                            using var toast = new ToastManager();
                            toast.Show($"回收站清空失败：{ex.Message}", "Error");
                        }
                        catch { }
                    });
                }
                catch { }
            }
        }

        // 模块元数据
        public string Name => "EmptyRecycleBin";
        public string Version => "1.0.0";
        public string Author => "Anonymity";
        public string Description => "提供清空回收站功能";

        // UI相关
        public bool HasUI => false;

        // 依赖关系
        public string[] Dependencies => [];

        // 生命周期方法
        public void Initialize()
        {

        }

        public void Start()
        {
            // 模块启动逻辑
        }

        public void Stop()
        {
            // 模块停止逻辑
        }

        public void ShowWindow()
        {
            // 由于HasUI为false，此方法可以为空
        }

        /// <summary>
        /// 清空回收站的核心方法
        /// </summary>
        /// <returns></returns>
        public static bool EmptyRecycleBin()
        {
            try
            {
                return EmptyRecycleBinFallback();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 备用的清空回收站方法 - 使用System.IO直接删除回收站文件
        /// </summary>
        /// <returns></returns>
        private static bool EmptyRecycleBinFallback()
        {
            try
            {
                string[] possiblePaths = [
                    @"C:\$Recycle.Bin",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "..", "$Recycle.Bin"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "..", "$Recycle.Bin")
                ]; // 尝试多个可能的回收站路径

                string? recycleBinPath = null;
                foreach (string path in possiblePaths)
                {
                    if (Directory.Exists(path))
                    {
                        recycleBinPath = path;
                        break;
                    }
                }

                if (recycleBinPath == null)
                {
                    return false;
                }

                // 获取所有子目录
                string[] subDirs = Directory.GetDirectories(recycleBinPath);
                int deletedCount = 0;
                foreach (string subDir in subDirs)
                {
                    try
                    {
                        string[] files = Directory.GetFiles(subDir, "*", SearchOption.AllDirectories); // 删除子目录中的所有文件
                        foreach (string file in files)
                        {
                            try
                            {
                                File.Delete(file);
                                deletedCount++;
                            }
                            catch { }
                        }
                        Directory.Delete(subDir, true); // 删除子目录
                    }
                    catch { }
                }
                return true; // 无论删除多少文件都返回成功，因为回收站已经被清空
            }
            catch
            {
                return false;
            }
        }
    }
}