using System.Runtime.InteropServices;
using Quicker.Managers;
using Quicker.Extend;

namespace EmptytheRecycleBin
{
    public partial class EmptyRecycleBinModule : IExtensionModule
    {
        // Windows API 声明 - 使用传统的DllImportAttribute确保兼容性
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, uint dwFlags);

        [DllImport("shell32.dll")]
        private static extern int SHUpdateRecycleBinIcon();

        // 常量定义
        private const uint SHERB_NOCONFIRMATION = 0x00000001;
        private const uint SHERB_NOPROGRESSUI = 0x00000002;
        private const uint SHERB_NOSOUND = 0x00000004;

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
            try
            {
                bool success = EmptyRecycleBin();
                string message = success ? "回收站清空成功" : "回收站清空失败";
                string type = success ? "Success" : "Error";
                ShowToast(message, type);
            }
            catch (UnauthorizedAccessException ex)
            {
                ShowToast($"权限不足：{ex.Message}", "Error");
            }
            catch (InvalidOperationException ex)
            {
                ShowToast($"操作失败：{ex.Message}", "Error");
            }
            catch (Exception ex)
            {
                ShowToast($"回收站清空失败：{ex.Message}", "Error");
            }
        }

        public void Stop()
        {

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
                uint flags = SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND; // 调用Windows API
                int result = SHEmptyRecycleBin(IntPtr.Zero, null, flags);
                if (result == 0) // S_OK
                {
                    int iconResult = SHUpdateRecycleBinIcon(); // 更新回收站图标并检查返回值
                    return true;
                }
                else
                {
                    return result switch // 检查特定的错误代码
                    {
                        // ERROR_INVALID_PARAMETER
                        unchecked((int)0x80070057) => throw new InvalidOperationException("参数无效"),
                        // ERROR_ACCESS_DENIED
                        unchecked((int)0x80070005) => throw new UnauthorizedAccessException("访问被拒绝，请以管理员身份运行"),
                        // ERROR_FILE_NOT_FOUND
                        unchecked((int)0x80070002) => true,// 回收站为空或不存在，这实际上是成功的
                        // E_UNEXPECTED - 通常表示回收站为空
                        unchecked((int)0x8000FFFF) => true,// 回收站为空，这实际上是成功的
                        // ERROR_NOT_READY
                        unchecked((int)0x80070015) => throw new InvalidOperationException("回收站不可用"),
                        _ => throw new InvalidOperationException($"清空回收站失败，错误代码: 0x{result:X8}"),
                    };
                }
            }
            catch (InvalidOperationException)
            {
                throw; // 重新抛出业务逻辑异常
            }
            catch (UnauthorizedAccessException)
            {
                throw; // 重新抛出权限异常
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"清空回收站时发生未知错误: {ex.Message}");
            }
        }
    }
}