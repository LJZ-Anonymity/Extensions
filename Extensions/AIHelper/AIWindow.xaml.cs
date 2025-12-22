using System.Text.Json;
using System.Net.Http;
using Quicker.Extend;
using System.Windows;
using System.Text;

namespace AIHelper
{
    public partial class AIWindow : Window, IExtensionModule
    {
        private const string _apiKey = "sk-SYIeraiqBBd4hxdNurZ4ZHO2wHitczSeyJa29ifGL8glbgWg";
        private const string _apiUrl = "https://api.moonshot.cn/v1/chat/completions";
        private readonly MarkdownRenderer _markdownRenderer;
        private readonly HttpClient _httpClient = new();

        public string Version => "1.0.0";

        public string Author => "Anonymity";

        public byte[] IconData => [];

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
    }
}