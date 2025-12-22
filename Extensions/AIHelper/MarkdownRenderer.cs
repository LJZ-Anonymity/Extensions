using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Net;
using Markdig;
using System.Linq; // 引入 Linq 以使用 OfType<T>()

namespace AIHelper
{
    /// <summary>
    /// 负责将 Markdown 文本渲染为 WPF FlowDocument 的工具类。
    /// </summary>
    /// <param name="targetDocument">要添加内容的 FlowDocument 实例</param>
    public class MarkdownRenderer(FlowDocument targetDocument)
    {
        private static readonly Regex HeadingRegex = new(@"<(h[1-6])>(.*?)</\1>", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex CodeBlockRegex = new(@"<pre><code.*?>(.*?)</code></pre>", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ListRegex = new(@"<ul[^>]*>(.*?)</ul>|<ol[^>]*>(.*?)</ol>", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ListItemRegex = new(@"<li[^>]*>(.*?)</li>", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ParagraphRegex = new(@"<p>(.*?)</p>", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // 简化 EmphasisRegex，确保捕获到完整的标签
        private static readonly Regex EmphasisRegex = new(@"(<strong>.*?</strong>|<em>.*?</em>|<code>.*?</code>)", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // **【修复新增】** 用于安全地提取行内元素（如 strong, em, code）的标签名和内容
        private static readonly Regex InnerTagContentRegex = new(@"^<(\w+)(?:[^>]*)?>(.*?)</\1>$", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);


        private static readonly Regex HorizontalRuleRegex = new(@"<hr\s*/?>", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // 匹配 Markdig 生成的表格结构
        private static readonly Regex TableRegex = new(@"<table[^>]*>(.*?)</table>", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex TableRowRegex = new(@"<tr[^>]*>(.*?)</tr>", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // 修改 TableCellRegex，捕获单元格标签内部的属性 (Groups[2])，用于获取对齐信息
        private static readonly Regex TableCellRegex = new(@"<(th|td)(\s[^>]*)?>(.*?)</\1>", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// 解析 Markdown 并追加到目标文档。
        /// </summary>
        /// <param name="text">包含 Markdown 格式的文本。</param>
        public void AppendMarkdown(string text)
        {
            // 配置 Markdig 管道，启用常用扩展，包括 Tables
            var pipeline = new MarkdownPipelineBuilder()
                .UseEmphasisExtras()
                .UseAutoLinks()
                .UsePipeTables() // 启用对表格的支持
                .Build();

            var markdownHtml = Markdown.ToHtml(text, pipeline);
            AppendMarkdownToDocument(markdownHtml);
        }

        /// <summary>
        /// 将 Markdown HTML 转换为简化的 WPF FlowDocument 元素并追加。
        /// </summary>
        /// <param name="html">Markdown 转换后的 HTML 内容。</param>
        private void AppendMarkdownToDocument(string html)
        {
            html = WebUtility.HtmlDecode(html).Trim();

            // 移除 Markdig 可能生成的空段落
            html = Regex.Replace(html, @"^\s*<p>\s*</p>\s*", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            string remainingHtml = html; // 从 HTML 开始，而不是包裹在 <p> 中

            while (!string.IsNullOrWhiteSpace(remainingHtml))
            {
                Match match = null;
                Block block = null;
                int matchIndex = -1;
                int matchLength = 0;

                // 1. 尝试匹配代码块（优先级最高）
                if ((match = CodeBlockRegex.Match(remainingHtml)).Success)
                {
                    block = ProcessCodeBlock(match.Groups[1].Value);
                    matchIndex = match.Index;
                    matchLength = match.Length;
                }
                // 2. 尝试匹配水平分割线
                else if ((match = HorizontalRuleRegex.Match(remainingHtml)).Success)
                {
                    block = ProcessHorizontalRule();
                    matchIndex = match.Index;
                    matchLength = match.Length;
                }
                // 3. 尝试匹配表格
                else if ((match = TableRegex.Match(remainingHtml)).Success)
                {
                    block = ProcessTable(match.Groups[1].Value);
                    matchIndex = match.Index;
                    matchLength = match.Length;
                }
                // 4. 尝试匹配列表
                else if ((match = ListRegex.Match(remainingHtml)).Success)
                {
                    // match.Value: 完整的 ul/ol 标签； match.Groups[1].Success: 是否是 ul (无序列表)
                    block = ProcessList(match.Value, match.Groups[1].Success);
                    matchIndex = match.Index;
                    matchLength = match.Length;
                }
                // 5. 尝试匹配标题
                else if ((match = HeadingRegex.Match(remainingHtml)).Success)
                {
                    // Groups[1]: h1-h6; Groups[2]: content
                    block = ProcessHeading(match.Groups[1].Value, match.Groups[2].Value);
                    matchIndex = match.Index;
                    matchLength = match.Length;
                }
                // 6. 尝试匹配段落（包含其他文本、行内格式）
                else if ((match = ParagraphRegex.Match(remainingHtml)).Success)
                {
                    block = ProcessParagraph(match.Groups[1].Value);
                    matchIndex = match.Index;
                    matchLength = match.Length;
                }

                if (block != null)
                {
                    targetDocument.Blocks.Add(block);
                    // 移除已处理的块级元素
                    remainingHtml = remainingHtml.Remove(matchIndex, matchLength).Trim();
                }
                else
                {
                    // 如果没有匹配到任何已知的块级元素，可能是解析错误或剩余的纯文本
                    // 移除任何未匹配的起始标签，防止死循环
                    if (Regex.IsMatch(remainingHtml, @"^\s*<[^>]+>.*"))
                    {
                        remainingHtml = Regex.Replace(remainingHtml, @"^\s*<[^>]+>", "").Trim();
                    }
                    else
                    {
                        // 将剩余部分视为一个段落（兜底处理）
                        if (!string.IsNullOrWhiteSpace(remainingHtml))
                        {
                            targetDocument.Blocks.Add(ProcessParagraph(remainingHtml));
                        }
                        remainingHtml = string.Empty; // 退出循环
                    }
                }
            }
        }

        /// <summary>
        /// 处理代码块。
        /// </summary>
        private Block ProcessCodeBlock(string codeHtml)
        {
            var codeText = Regex.Replace(codeHtml, "<br\\s*/?>|&#xA;", "\n");
            codeText = RemoveHtmlTags(codeText, keepBr: false);

            var paragraph = new Paragraph
            {
                Margin = new Thickness(10, 5, 10, 5),
                Background = new SolidColorBrush(Color.FromRgb(40, 44, 52)),
                Padding = new Thickness(10)
            };

            var run = new Run(codeText.Trim())
            {
                Foreground = Brushes.White,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
            };

            paragraph.Inlines.Add(run);
            return paragraph;
        }

        /// <summary>
        /// 处理标题。
        /// </summary>
        private Block ProcessHeading(string tag, string content)
        {
            var paragraph = new Paragraph
            {
                Margin = new Thickness(0, 15, 0, 5),
            };

            double fontSize = 14;
            FontWeight fontWeight = FontWeights.Bold;

            switch (tag.ToLower())
            {
                case "h1":
                    fontSize = 20;
                    fontWeight = FontWeights.ExtraBold;
                    break;
                case "h2":
                    fontSize = 18;
                    fontWeight = FontWeights.Bold;
                    break;
                case "h3":
                    fontSize = 16;
                    fontWeight = FontWeights.Bold;
                    break;
                case "h4":
                case "h5":
                case "h6":
                    fontSize = 14;
                    fontWeight = FontWeights.Normal; // 修改为 Normal
                    break;
            }

            // **【修复点 1】**：使用 Span 容器包裹内容，并用 ProcessInlines 支持标题内部的行内格式（如加粗）
            var contentSpan = new Span
            {
                FontSize = fontSize,
                FontWeight = fontWeight
            };

            ProcessInlines(contentSpan.Inlines, content);

            paragraph.Inlines.Add(contentSpan);

            return paragraph;
        }

        /// <summary>
        /// 处理列表。
        /// </summary>
        private Block ProcessList(string listHtml, bool isUnordered)
        {
            var list = new List
            {
                Margin = new Thickness(10, 10, 0, 10),
                MarkerStyle = isUnordered ? TextMarkerStyle.Disc : TextMarkerStyle.Decimal
            };

            var listMatches = ListItemRegex.Matches(listHtml);
            foreach (Match itemMatch in listMatches)
            {
                var listItemContent = itemMatch.Groups[1].Value.Trim();
                var listItem = new ListItem();
                // 递归处理列表项内的内容，将其视为一个普通段落
                listItem.Blocks.Add(ProcessParagraph(listItemContent));
                list.ListItems.Add(listItem);
            }
            return list;
        }

        /// <summary>
        /// 处理水平分割线 (<hr>)。
        /// </summary>
        private Block ProcessHorizontalRule()
        {
            var section = new Section { Margin = new Thickness(0, 10, 0, 10) };

            var line = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(0, 0.5, 0, 0),
                Height = 1
            };

            section.Blocks.Add(new BlockUIContainer(line));
            return section;
        }

        /// <summary>
        /// 处理表格。
        /// </summary>
        private Block ProcessTable(string tableHtml)
        {
            // 创建一个 WPF Table 元素
            var table = new Table
            {
                CellSpacing = 0,
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1), // 整个表格边框
                Margin = new Thickness(0, 10, 0, 10)
            };

            var rowMatches = TableRowRegex.Matches(tableHtml);

            // 1. 确定列数 (使用第一行/表头的列数)
            int columnCount = rowMatches.Cast<Match>()
                .Select(row => TableCellRegex.Matches(row.Groups[1].Value).Count)
                .DefaultIfEmpty(0)
                .Max();

            if (columnCount == 0)
            {
                return ProcessParagraph(tableHtml);
            }

            // 2. 创建 TableColumn
            for (int i = 0; i < columnCount; i++)
            {
                table.Columns.Add(new TableColumn());
            }

            // 3. 确定表格对齐方式 (从 Markdig 生成的 HTML 属性中解析)
            TextAlignment[] columnAlignments = new TextAlignment[columnCount];
            var firstRowContent = rowMatches.FirstOrDefault()?.Groups[1].Value ?? string.Empty;
            var headerCellMatches = TableCellRegex.Matches(firstRowContent);

            for (int i = 0; i < columnCount && i < headerCellMatches.Count; i++)
            {
                // Groups[2] 包含单元格标签内部的所有属性，例如 ' align="right"'
                var attrs = headerCellMatches[i].Groups[2].Value;

                // 检查 align 属性或 style 属性
                if (attrs.Contains("align=\"right\"", StringComparison.OrdinalIgnoreCase) ||
                    attrs.Contains("text-align: right", StringComparison.OrdinalIgnoreCase))
                {
                    columnAlignments[i] = TextAlignment.Right;
                }
                else if (attrs.Contains("align=\"center\"", StringComparison.OrdinalIgnoreCase) ||
                         attrs.Contains("text-align: center", StringComparison.OrdinalIgnoreCase))
                {
                    columnAlignments[i] = TextAlignment.Center;
                }
                else
                {
                    columnAlignments[i] = TextAlignment.Left;
                }
            }


            // 4. 创建行组并处理行
            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            int rowIndex = 0;

            foreach (Match rowMatch in rowMatches)
            {
                var tableRow = new TableRow();
                var cellMatches = TableCellRegex.Matches(rowMatch.Groups[1].Value);
                int cellIndex = 0;

                foreach (Match cellMatch in cellMatches)
                {
                    // Groups[1]: "th" 或 "td"
                    var isTh = cellMatch.Groups[1].Value.ToLower() == "th";
                    // Groups[3]: 单元格内容
                    var cellContent = cellMatch.Groups[3].Value.Trim();

                    // 使用第 3 步解析的对齐方式
                    TextAlignment cellAlignment = (cellIndex < columnAlignments.Length) ? columnAlignments[cellIndex] : TextAlignment.Left;


                    var tableCell = new TableCell
                    {
                        BorderBrush = Brushes.LightGray,
                        // 初始边框：右侧和底部有边框
                        BorderThickness = new Thickness(0, 0, 1, 1),
                        Padding = new Thickness(8),
                        // 如果是表头，设置背景
                        Background = isTh ? new SolidColorBrush(Color.FromRgb(245, 245, 245)) : Brushes.White,
                        FontWeight = isTh ? FontWeights.Bold : FontWeights.Normal
                    };

                    // 将单元格内容（可能包含行内格式）作为一个 Paragraph 添加
                    var cellParagraph = new Paragraph
                    {
                        Margin = new Thickness(0), // 移除单元格内段落的默认边距
                        TextAlignment = cellAlignment // 应用表格对齐
                    };
                    ProcessInlines(cellParagraph.Inlines, cellContent);
                    tableCell.Blocks.Add(cellParagraph);

                    tableRow.Cells.Add(tableCell);
                    cellIndex++;
                }
                rowGroup.Rows.Add(tableRow);
                rowIndex++;
            }

            // 5. 修正：处理边框，确保只有内部边框
            if (table.RowGroups.Count > 0)
            {
                var rows = table.RowGroups[0].Rows;
                for (int i = 0; i < rows.Count; i++) // 行
                {
                    var row = rows[i];
                    for (int j = 0; j < row.Cells.Count; j++) // 列
                    {
                        var cell = row.Cells[j];
                        // 移除最底部的边框 (最后一行)
                        double bottomThickness = (i == rows.Count - 1) ? 0 : 1;
                        // 移除最右侧的边框 (最后一列)
                        double rightThickness = (j == row.Cells.Count - 1) ? 0 : 1;

                        // 设置边框：左侧和顶部由 Table 自身的 BorderThickness(1) 提供
                        cell.BorderThickness = new Thickness(0, 0, rightThickness, bottomThickness);
                    }
                }
            }

            return table;
        }

        /// <summary>
        /// 处理段落文本和行内格式。
        /// </summary>
        private Block ProcessParagraph(string contentHtml)
        {
            var paragraph = new Paragraph
            {
                // 确保段落有足够的垂直间距
                Margin = new Thickness(0, 5, 0, 5)
            };

            // 如果内容为空或只包含空白，则插入一个不可见的空格 Run。
            if (string.IsNullOrWhiteSpace(RemoveHtmlTags(contentHtml, keepBr: true)))
            {
                paragraph.Inlines.Add(new Run(" ") { FontSize = 1 });
                return paragraph;
            }

            // 使用一个辅助方法来处理行内元素
            ProcessInlines(paragraph.Inlines, contentHtml);

            return paragraph;
        }

        /// <summary>
        /// 递归处理行内元素，例如加粗、斜体、行内代码。
        /// </summary>
        private void ProcessInlines(InlineCollection inlines, string contentHtml)
        {
            string remainingContent = contentHtml;

            // 将 <br> 标签转换为换行符
            remainingContent = Regex.Replace(remainingContent, @"<br\s*/?>", "\n");

            while (!string.IsNullOrWhiteSpace(remainingContent))
            {
                Match match = EmphasisRegex.Match(remainingContent);

                if (match.Success)
                {
                    // 1. 处理匹配前的纯文本
                    if (match.Index > 0)
                    {
                        // 纯文本不需要递归，直接移除 HTML 标签
                        // 注意：这里保留换行符（<br> -> \n）
                        inlines.Add(new Run(RemoveHtmlTags(remainingContent[..match.Index], keepBr: true)));
                    }

                    // 2. 处理匹配到的行内元素
                    Inline inlineElement = null;
                    // 使用 match.Value 获取完整的标签内容，例如 <strong>text</strong>
                    string fullTag = match.Value;

                    string content = string.Empty;
                    string tagName = string.Empty;

                    // **【修复点 2】**：使用 InnerTagContentRegex 安全地提取标签名和内容
                    var innerMatch = InnerTagContentRegex.Match(fullTag);

                    if (innerMatch.Success)
                    {
                        tagName = innerMatch.Groups[1].Value.ToLower(); // 例如 "strong"
                        content = innerMatch.Groups[2].Value; // 标签内部的内容
                    }

                    if (tagName == "strong") // 加粗
                    {
                        var span = new Span { FontWeight = FontWeights.Bold };
                        ProcessInlines(span.Inlines, content); // 递归处理内容
                        inlineElement = span;
                    }
                    else if (tagName == "em") // 斜体
                    {
                        var span = new Span { FontStyle = FontStyles.Italic };
                        ProcessInlines(span.Inlines, content); // 递归处理内容
                        inlineElement = span;
                    }
                    else if (tagName == "code") // 行内代码
                    {
                        // 行内代码内容通常不包含其他行内格式，直接用 Run 包裹
                        var run = new Run(RemoveHtmlTags(content, keepBr: true))
                        {
                            Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                            FontFamily = new FontFamily("Consolas")
                        };
                        inlineElement = run;
                    }

                    if (inlineElement != null)
                    {
                        inlines.Add(inlineElement);
                    }

                    // 3. 更新剩余文本 (跳过整个匹配到的标签)
                    remainingContent = remainingContent.Remove(match.Index, match.Length);
                }
                else
                {
                    // 没有匹配到行内元素，将剩余部分作为普通文本处理
                    inlines.Add(new Run(RemoveHtmlTags(remainingContent, keepBr: true)));
                    remainingContent = string.Empty; // 退出循环
                }
            }
        }

        /// <summary>
        /// 移除所有 HTML 标签。
        /// </summary>
        /// <param name="html">包含 HTML 标签的字符串。</param>
        /// <param name="keepBr">是否保留 <br> 标签（并将其替换为 \n）。</param>
        private string RemoveHtmlTags(string html, bool keepBr = false)
        {
            string result = html;
            if (keepBr)
            {
                // 将 <br> 替换为换行符
                result = Regex.Replace(result, @"<br\s*/?>", "\n");
            }

            // 移除所有剩余的 HTML 标签
            return Regex.Replace(result, "<[^>]*>", "");
        }
    }
}