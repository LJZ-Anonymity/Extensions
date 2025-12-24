using System.Runtime.InteropServices;
using System.Reflection;
using Quicker.Managers;
using Quicker.Extend;
using System.IO;

namespace EmptytheRecycleBin
{
    public partial class EmptyRecycleBinModule : IExtensionModule
    {
        // 模块元数据
        public string Name => "EmptyRecycleBin";
        public string Version => "1.0.0";
        public string Author => "Anonymity";
        public string Description => "提供清空回收站功能";
        public byte[] IconData => GetIconData();
        public bool HasContextMenu => false;

        // Windows API 声明 - 使用传统的DllImportAttribute确保兼容性
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, uint dwFlags);

        [DllImport("shell32.dll")]
        private static extern int SHUpdateRecycleBinIcon();

        // 回收站操作标志常量
        private const uint SHERB_NOCONFIRMATION = 0x00000001; // 不显示确认对话框
        private const uint SHERB_NOPROGRESSUI = 0x00000002;   // 不显示进度对话框
        private const uint SHERB_NOSOUND = 0x00000004;        // 不播放声音

        // 系统错误码常量
        private const int ERROR_INVALID_PARAMETER = unchecked((int)0x80070057); // 无效参数
        private const int ERROR_ACCESS_DENIED = unchecked((int)0x80070005);     // 访问被拒绝
        private const int ERROR_FILE_NOT_FOUND = unchecked((int)0x80070002);    // 文件未找到
        private const int E_UNEXPECTED = unchecked((int)0x8000FFFF);            // 未预期的错误
        private const int ERROR_NOT_READY = unchecked((int)0x80070015);         // 未就绪

        /// <summary>
        /// 显示Toast消息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="type">消息类型</param>
        private static void ShowToast(string message, ToastType type)
        {
            Task.Run(async () =>
            {
                await Task.Delay(100);
                using var toast = new ToastManager();
                toast.Show(message, type);
            });
        }

        /// <summary>
        /// 获取图标字节数组
        /// </summary>
        /// <returns>图标数据</returns>
        private static byte[] GetIconData()
        {
            // 从嵌入资源中读取图标
            Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("EmptytheRecycleBin.icon.ico");
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

        public async void Activate()
        {
            try
            {
                ShowToast("开始清空回收站。", ToastType.Common);
                bool success = await EmptyRecycleBinAsync();
                string message = success ? "回收站清空成功！" : "回收站清空失败！";
                ToastType type = success ? ToastType.Success : ToastType.Error;
                ShowToast(message, type);
            }
            catch (UnauthorizedAccessException ex)
            {
                ShowToast($"权限不足：{ex.Message}", ToastType.Error);
            }
            catch (InvalidOperationException ex)
            {
                ShowToast($"操作失败：{ex.Message}", ToastType.Error);
            }
            catch (Exception ex)
            {
                ShowToast($"回收站清空失败：{ex.Message}", ToastType.Error);
            }
        }

        /// <summary>
        /// 清空回收站的异步核心方法
        /// </summary>
        /// <returns> 清空是否成功</returns>
        public static async Task<bool> EmptyRecycleBinAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    uint flags = SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND; // 设置清空回收站的标志
                    int result = SHEmptyRecycleBin(IntPtr.Zero, null, flags); // 调用Windows API清空回收站
                    if (result == 0)
                    {
                        _ = SHUpdateRecycleBinIcon();
                        return true;
                    }

                    return result switch
                    {
                        ERROR_INVALID_PARAMETER => throw new InvalidOperationException("参数无效"),
                        ERROR_ACCESS_DENIED => throw new UnauthorizedAccessException("访问被拒绝，请以管理员身份运行"),
                        ERROR_FILE_NOT_FOUND => true,
                        E_UNEXPECTED => true,
                        ERROR_NOT_READY => throw new InvalidOperationException("回收站不可用"),
                        _ => throw new InvalidOperationException($"清空回收站失败，错误代码: 0x{result:X8}")
                    };
                }
                catch{ throw; }
            });
        }

        public void Deactivate()
        {

        }
    }
}