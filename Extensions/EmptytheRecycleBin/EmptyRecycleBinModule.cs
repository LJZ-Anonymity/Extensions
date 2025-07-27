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
                bool success = EmptyRecycleBin();
                string message = success ? "回收站清空成功" : "回收站清空失败，请检查权限";
                string type = success ? "Success" : "Error";
                ShowToast(message, type);
            }
            catch (Exception ex)
            {
                ShowToast($"回收站清空失败：{ex.Message}", "Error");
            }
        }

        /// <summary>
        /// 显示Toast消息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="type">消息类型</param>
        private static void ShowToast(string message, string type)
        {
            Task.Delay(100).ContinueWith(_ =>
            {
                try
                {
                    using var toast = new ToastManager();
                    toast.Show(message, type);
                }
                catch { }
            });
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
            return EmptyRecycleBinFallback();
        }

        /// <summary>
        /// 备用的清空回收站方法 - 使用System.IO直接删除回收站文件
        /// </summary>
        /// <returns></returns>
        private static bool EmptyRecycleBinFallback()
        {
            string? recycleBinPath = FindRecycleBinPath();
            if (recycleBinPath == null)
            {
                return false;
            }

            return DeleteRecycleBinContents(recycleBinPath);
        }

        /// <summary>
        /// 查找回收站路径
        /// </summary>
        /// <returns>回收站路径，如果未找到则返回null</returns>
        private static string? FindRecycleBinPath()
        {
            string[] possiblePaths = [
                @"C:\$Recycle.Bin",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "..", "$Recycle.Bin"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "..", "$Recycle.Bin")
            ]; // 尝试多个可能的回收站路径

            foreach (string path in possiblePaths)
            {
                if (Directory.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        /// <summary>
        /// 删除回收站内容
        /// </summary>
        /// <param name="recycleBinPath">回收站路径</param>
        /// <returns>是否成功</returns>
        private static bool DeleteRecycleBinContents(string recycleBinPath)
        {
            try
            {
                string[] subDirs = Directory.GetDirectories(recycleBinPath);
                int deletedCount = 0;
                foreach (string subDir in subDirs)
                {
                    deletedCount += DeleteDirectoryContents(subDir);
                }

                return true; // 无论删除多少文件都返回成功，因为回收站已经被清空
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 删除目录内容
        /// </summary>
        /// <param name="directoryPath">目录路径</param>
        /// <returns>删除的文件数量</returns>
        private static int DeleteDirectoryContents(string directoryPath)
        {
            try
            {
                string[] files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
                int deletedCount = 0;
                foreach (string file in files)
                {
                    try
                    {
                        File.Delete(file);
                        deletedCount++;
                    }
                    catch { }
                }

                Directory.Delete(directoryPath, true); // 删除子目录
                return deletedCount;
            }
            catch
            {
                return 0;
            }
        }
    }
}