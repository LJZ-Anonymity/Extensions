using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using Markdig;

namespace AIHelper
{
    /// <summary>
    /// 负责将 Markdown 文本渲染为 WPF FlowDocument 的工具类。
    /// </summary>
    /// <param name="targetDocument">要添加内容的 FlowDocument 实例</param>
    public class MarkdownRenderer(FlowDocument targetDocument)
    {
        /// <summary>
        /// 解析 Markdown 并追加到目标文档。
        /// </summary>
        /// <param name="text">包含 Markdown 格式的文本。</param>
        public void AppendMarkdown(string text)
        {
            var markdownHtml = Markdown.ToHtml(text); // 解析 Markdown 为 HTML
            AppendMarkdownToDocument(markdownHtml); // 将 HTML 转换为 WPF FlowDocument 元素
        }

        /// <summary>
        /// 将 Markdown HTML 转换为简化的 WPF FlowDocument 元素并追加。
        /// </summary>
        /// <param name="html">Markdown 转换后的 HTML 内容。</param>
        private void AppendMarkdownToDocument(string html)
        {
            // 简单的HTML到WPF FlowDocument转换
            // 移除所有 HTML 标签
            var plainText = Regex.Replace(html, "<[^>]*>", "");
            // 解码基本 HTML 实体
            plainText = plainText.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&");

            // 创建新的段落
            var paragraph = new Paragraph
            {
                Margin = new Thickness(0, 5, 0, 5)
            };

            // 处理基本的 Markdown 格式
            var lines = plainText.Split('\n');
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    paragraph.Inlines.Add(new LineBreak());
                    continue;
                }

                var run = new Run(line);

                // 简单的格式检测和应用
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
                // 加粗 **text**
                else if (line.StartsWith("**") && line.EndsWith("**") && line.Length > 2)
                {
                    run.Text = line.Trim('*');
                    run.FontWeight = FontWeights.Bold;
                }
                // 斜体 *text*
                else if (line.StartsWith("*") && line.EndsWith("*") && line.Length > 1)
                {
                    run.Text = line.Trim('*');
                    run.FontStyle = FontStyles.Italic;
                }
                // 行内代码 `code`
                else if (line.StartsWith("`") && line.EndsWith("`") && line.Length > 1)
                {
                    run.Text = line.Trim('`');
                    run.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
                    run.FontFamily = new FontFamily("Consolas");
                }

                paragraph.Inlines.Add(run);
                paragraph.Inlines.Add(new LineBreak());
            }

            targetDocument.Blocks.Add(paragraph);
        }
    }
}