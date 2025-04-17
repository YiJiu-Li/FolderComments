using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FolderComments
{
    /// <summary>
    /// InputDialog.xaml 的交互逻辑
    /// </summary>
    public partial class InputDialog : Window
    {
        // 占位符文本常量
        private const string PLACEHOLDER_TEXT = "输入文件夹注释内容...";

        // 文本框颜色
        private readonly Brush placeholderColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BBBBBB"));
        private readonly Brush normalTextColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));

        /// <summary>
        /// 获取输入的文本
        /// </summary>
        public string InputText { get; private set; }

        /// <summary>
        /// 初始化输入对话框
        /// </summary>
        /// <param name="prompt">提示文本</param>
        /// <param name="title">标题</param>
        /// <param name="defaultText">默认文本</param>
        public InputDialog(string prompt = "设置新的文件夹注释", string title = "编辑注释", string defaultText = "")
        {
            InitializeComponent();

            // 设置窗口标题
            this.Title = title;

            // 更新提示文本
            var promptTextBlock = this.FindName("PromptTextBlock") as TextBlock;
            if (promptTextBlock != null && !string.IsNullOrEmpty(prompt))
            {
                promptTextBlock.Text = prompt;
            }

            // 设置文本框事件
            CommentTextBox.GotFocus += CommentTextBox_GotFocus;
            CommentTextBox.LostFocus += CommentTextBox_LostFocus;

            // 设置默认文本
            if (!string.IsNullOrEmpty(defaultText))
            {
                CommentTextBox.Text = defaultText;
                CommentTextBox.Foreground = normalTextColor;
            }
            else
            {
                // 如果没有默认文本，显示占位符文本
                CommentTextBox.Text = PLACEHOLDER_TEXT;
                CommentTextBox.Foreground = placeholderColor;
            }

            // 焦点设置到确定按钮
            this.Loaded += (s, e) => OkButton.Focus();
        }

        /// <summary>
        /// 文本框获取焦点事件
        /// </summary>
        private void CommentTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (CommentTextBox.Text == PLACEHOLDER_TEXT)
            {
                CommentTextBox.Text = string.Empty;
                CommentTextBox.Foreground = normalTextColor;
            }
        }

        /// <summary>
        /// 文本框失去焦点事件
        /// </summary>
        private void CommentTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CommentTextBox.Text))
            {
                CommentTextBox.Text = PLACEHOLDER_TEXT;
                CommentTextBox.Foreground = placeholderColor;
            }
        }

        /// <summary>
        /// 处理标题栏的鼠标按下事件，用于拖动窗口
        /// </summary>
        private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// 文本变化事件，用于更新字符计数
        /// </summary>
        private void OnCommentTextChanged(object sender, TextChangedEventArgs e)
        {
            // 如果当前文本是占位符，则不更新计数
            if (CommentTextBox.Text == PLACEHOLDER_TEXT)
            {
                CharCounterTextBlock.Text = "0/4096";
                return;
            }

            // 更新字数计数
            int currentLength = CommentTextBox.Text.Length;
            CharCounterTextBlock.Text = $"{currentLength}/4096";

            // 如果超过限制，设置警告颜色
            if (currentLength > 4000)
            {
                CharCounterTextBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C"));
            }
            else
            {
                CharCounterTextBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F8C8D"));
            }
        }

        /// <summary>
        /// 点击确定按钮的事件处理
        /// </summary>
        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            // 如果文本是占位符，则视为空文本
            InputText = (CommentTextBox.Text == PLACEHOLDER_TEXT) ? string.Empty : CommentTextBox.Text;
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// 点击取消按钮的事件处理
        /// </summary>
        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}