using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.Json;
using System.Net.Http;
using Quicker.Extend;
using System.Windows;
using System.Text;

namespace AIHelper
{
    public partial class AIWindow : Window, IExtensionModule
    {
        private const string _apiKey = "sk-cy3413yv1rpjl54vkcjqkbhdxkza3620e8rmdlopiy3dnnum";
        private const string _apiUrl = "https://api.xiaomimimo.com/v1/chat/completions";
        private readonly MarkdownRenderer _markdownRenderer;
        private readonly HttpClient _httpClient = new();

        public new string Name => "AI助手";

        public string Version => "1.0.0";

        public string Author => "Anonymity";

        public byte[] IconData => Array.Empty<byte>();

        public string Description => "";

        public bool HasContextMenu => false;

        public AIWindow()
        {
            InitializeComponent();
            _markdownRenderer = new MarkdownRenderer(ResponseDocument);
        }

        /// <summary>
        /// 输入框获得焦点时的处理
        /// </summary>
        private void InputTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (InputTextBox.Text == "请输入您的问题...")
            {
                InputTextBox.Text = "";
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string userInput = InputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(userInput) || userInput == "请输入您的问题...")
            {
                return;
            }

            SendButton.IsEnabled = false;
            InputTextBox.Text = "";

            // 1. 显示用户输入：在 **用户：** 后面添加 &nbsp; 确保粗体不溢出
            string userMessageMarkdown = $"**用户：**&nbsp;\n\n{userInput}\n\n";
            AppendToResponse(userMessageMarkdown); //

            // 2. 显示“AI 思考中...”占位符 (使用行内粗体格式，避免生成独立的 Block)
            string thinkingMessage = "**AI：**&nbsp;\n\n思考中...\n\n";
            AppendToResponse(thinkingMessage);

            // 记录“思考中” Block 的实例 (由于使用了行内格式，这里不需要 Block 实例，但为了兼容后续移除操作，我们保留逻辑)
            Block thinkingBlockInstance = null;

            Dispatcher.Invoke(() =>
            {
                // 获取最后一个添加的 Block 实例，以便在收到回复后移除
                if (ResponseDocument.Blocks.Count > 0)
                {
                    thinkingBlockInstance = ResponseDocument.Blocks.LastBlock;
                }
            });

            try
            {
                // 3. 调用MiMO AI API
                string aiResponse = await CallMiMOAPI(userInput);

                // 4. 移除 “AI: 思考中...” 的 Block
                if (thinkingBlockInstance != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        // 使用 Remove(Block element) 方法
                        ResponseDocument.Blocks.Remove(thinkingBlockInstance);
                    });
                }

                // 5. 构造并显示最终的 AI 回复：在 **AI：** 后面添加 &nbsp; 确保粗体不溢出
                string aiResponseMarkdown = $"{aiResponse}\n\n"; // 【修改点】
                AppendToResponse(aiResponseMarkdown); //

                // 6. 额外添加一个水平分割线
                AppendToResponse("---\n\n"); //
            }
            catch (Exception ex)
            {
                // 如果出错，确保移除“思考中”状态
                if (thinkingBlockInstance != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        ResponseDocument.Blocks.Remove(thinkingBlockInstance);
                    });
                }

                // 显示错误信息
                AppendToResponse($"**错误:** {ex.Message}\n\n");
            }
            finally
            {
                SendButton.IsEnabled = true;

                // 滚动到底部
                Dispatcher.Invoke(() =>
                {
                    ScrollToEnd(ResponseDocumentViewer);
                });
            }
        }

        // ... (其余方法保持不变)

        /// <summary>
        /// 修复：获取 FlowDocumentScrollViewer 内部的 ScrollViewer 并滚动到底部。
        /// </summary>
        private void ScrollToEnd(FlowDocumentScrollViewer viewer)
        {
            ScrollViewer scrollViewer = FindVisualChild<ScrollViewer>(viewer);
            scrollViewer?.ScrollToEnd();
        }

        /// <summary>
        /// 辅助方法：查找指定类型的可视子元素。
        /// </summary>
        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }
                else
                {
                    T result = FindVisualChild<T>(child);
                    if (result != null) return result;
                }
            }
            return null;
        }

        /// <summary>
        /// 调用MiMO AI API
        /// </summary>
        private async Task<string> CallMiMOAPI(string userMessage)
        {
            var requestBody = new
            {
                model = "mimo-v2-flash",
                messages = new[]
                {
                    new { role = "system", content = $"你是MiMo（中文名称也是MiMo），是小米公司研发的AI智能助手。今天的日期：{DateTime.Today}，你的知识截止日期是2024年12月。" },
                    new { role = "user", content = userMessage }
                },
                max_completion_tokens = 1024,
                temperature = 0.3,
                top_p = 0.95,
                stream = false,
                stop = (string?)null,
                frequency_penalty = 0,
                presence_penalty = 0,
                extra_body = new
                {
                    thinking = new
                    {
                        type = "disabled"
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.PostAsync(_apiUrl, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);

            return responseJson.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "无法获取AI回复";
        }

        /// <summary>
        /// 追加到响应区域
        /// </summary>
        /// <param name="text">文本</param>
        private void AppendToResponse(string text)
        {
            Dispatcher.Invoke(() =>
            {
                // 这里调用 MarkdownRenderer
                _markdownRenderer.AppendMarkdown(text);
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            _httpClient?.Dispose();
            base.OnClosed(e);
        }

        void IExtensionModule.Activate()
        {
            Show();
        }

        public void Deactivate()
        {

        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendButton_Click(sender, e);
                // FIX: 标记事件已处理，防止 AcceptsReturn="True" 插入换行符
                e.Handled = true;
            }
        }
    }
}