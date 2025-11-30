using System.Windows.Documents;
using System.Windows.Media;
using System.Text.Json;
using System.Net.Http;
using System.Windows;
using Quicker.Extend;
using System.Text;
using Markdig;

namespace Quicker.Windows.ToolWindows
{
    public partial class AIWindow : Window, IExtensionModule
    {
        private const string _apiKey = "sk-SYIeraiqBBd4hxdNurZ4ZHO2wHitczSeyJa29ifGL8glbgWg";
        private const string _apiUrl = "https://api.moonshot.cn/v1/chat/completions";
        private readonly HttpClient _httpClient = new();

        public string Version => "1.0.0";

        public string Author => "Anonymity";

        public byte[] IconData => throw new NotImplementedException();

        public string Description => throw new NotImplementedException();

        public bool HasContextMenu => false;

        public AIWindow()
        {
            InitializeComponent();
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

            // 禁用按钮防止重复点击
            SendButton.IsEnabled = false;
            InputTextBox.Clear();

            try
            {
                // 显示用户输入
                AppendToResponse($"用户: {userInput}\n\n");
                
                // 调用Moonshot AI API
                string aiResponse = await CallMoonshotAPI(userInput);
                
                // 显示AI回复
                AppendToResponse($"AI: {aiResponse}\n\n");
            }
            catch (Exception ex)
            {
                AppendToResponse($"错误: {ex.Message}\n\n");
            }
            finally
            {
                // 恢复按钮状态
                SendButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// 调用Moonshot AI API
        /// </summary>
        /// <param name="userMessage">用户消息</param>
        /// <returns>AI回复</returns>
        private async Task<string> CallMoonshotAPI(string userMessage)
        {
            var requestBody = new
            {
                model = "moonshot-v1-8k",
                messages = new[]
                {
                    new { role = "system", content = "你是Kimi，由Moonshot AI提供的人工智能助手，你更擅长中文和英文的对话。你会为用户提供安全、有帮助、准确的回答。" },
                    new { role = "user", content = userMessage }
                },
                temperature = 0.6
            }; // 请求体

            var json = JsonSerializer.Serialize(requestBody); // 序列化请求体
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear(); // 清空请求头
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.PostAsync(_apiUrl, content); // 发送请求
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(); // 读取响应内容
            var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);

            return responseJson.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString(); // 返回AI回复
        }

        /// <summary>
        /// 追加到响应区域
        /// </summary>
        /// <param name="text">文本</param>
        private void AppendToResponse(string text)
        {
            Dispatcher.Invoke(() =>
            {
                // 解析Markdown并添加到文档
                var markdownHtml = Markdown.ToHtml(text);
                AppendMarkdownToDocument(markdownHtml);
            });
        }

        /// <summary>
        /// 将Markdown HTML添加到文档中
        /// </summary>
        /// <param name="html">HTML内容</param>
        private void AppendMarkdownToDocument(string html)
        {
            // 简单的HTML到WPF FlowDocument转换
            // 这里我们简化处理，将HTML转换为纯文本并保持基本格式
            var plainText = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", "");
            plainText = plainText.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&");

            // 创建新的段落
            var paragraph = new Paragraph
            {
                Margin = new Thickness(0, 5, 0, 5)
            };

            // 处理基本的Markdown格式
            var lines = plainText.Split('\n');
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    paragraph.Inlines.Add(new LineBreak());
                    continue;
                }
                
                var run = new Run(line);
                
                // 简单的格式检测
                if (line.StartsWith("# "))
                {
                    run.FontSize = 18;
                    run.FontWeight = FontWeights.Bold;
                }
                else if (line.StartsWith("## "))
                {
                    run.FontSize = 16;
                    run.FontWeight = FontWeights.Bold;
                }
                else if (line.StartsWith("### "))
                {
                    run.FontSize = 14;
                    run.FontWeight = FontWeights.Bold;
                }
                else if (line.StartsWith("**") && line.EndsWith("**"))
                {
                    run.FontWeight = FontWeights.Bold;
                }
                else if (line.StartsWith("*") && line.EndsWith("*"))
                {
                    run.FontStyle = FontStyles.Italic;
                }
                else if (line.StartsWith("`") && line.EndsWith("`"))
                {
                    run.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
                    run.FontFamily = new FontFamily("Consolas");
                }
                
                paragraph.Inlines.Add(run);
                paragraph.Inlines.Add(new LineBreak());
            }
            
            ResponseDocument.Blocks.Add(paragraph);
        }

        protected override void OnClosed(EventArgs e)
        {
            _httpClient?.Dispose();
            base.OnClosed(e);
        }

        void IExtensionModule.Activate()
        {
            throw new NotImplementedException();
        }

        public void Deactivate()
        {
            throw new NotImplementedException();
        }
    }
}