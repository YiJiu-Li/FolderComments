using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace FolderComments
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 右键菜单文本
        /// </summary>
        private string contextMenuText = "编辑注释";

        /// <summary>
        /// 注册表键列表
        /// </summary>
        private List<string> registryKeyList = new List<string>();

        public MainWindow()
        {
            InitializeComponent();

            /* 设置值 */

            // 添加文件夹右键菜单项
            this.registryKeyList.Add($"Software\\Classes\\directory\\shell\\{this.contextMenuText}");

            //// 添加文件右键菜单项
            //this.registryKeyList.Add($"Software\\Classes\\*\\shell\\{this.contextMenuText}");

            // 根据注册表键更新程序状态
            this.UpdateByRegistryKey();
        }

        /// <summary>
        /// 通过注册表键更新程序状态
        /// </summary>
        private void UpdateByRegistryKey()
        {
            bool anyRegistryKeyExists = false;

            // 检查任意一个注册表键是否存在
            foreach (string key in this.registryKeyList)
            {
                using (var registryKey = Registry.CurrentUser.OpenSubKey(key))
                {
                    if (registryKey != null)
                    {
                        anyRegistryKeyExists = true;
                        break;
                    }
                }
            }

            // 根据检查结果更新UI状态
            if (!anyRegistryKeyExists)
            {
                // 禁用移除按钮
                this.removeButton.IsEnabled = false;

                // 启用添加按钮
                this.addButton.IsEnabled = true;

                // 更新状态文本
                this.activityToolStripStatusLabel.Text = "右键菜单状态: 未注册";

                // 更新状态指示器颜色
                this.statusIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F8C8D"));
            }
            else
            {
                // 禁用添加按钮
                this.addButton.IsEnabled = false;

                // 启用移除按钮
                this.removeButton.IsEnabled = true;

                // 更新状态文本
                this.activityToolStripStatusLabel.Text = "右键菜单状态: 已注册";

                // 更新状态指示器颜色
                this.statusIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60"));
            }
        }

        /// <summary>
        /// 处理添加按钮点击事件
        /// </summary>
        /// <param name="sender">发送者对象</param>
        /// <param name="e">事件参数</param>
        private void OnAddButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string executablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string startupPath = System.IO.Path.GetDirectoryName(executablePath);

                bool allSucceeded = true;

                // 遍历注册表键
                foreach (string currentRegistryKey in this.registryKeyList)
                {
                    // 检查键是否已存在
                    using (var existingKey = Registry.CurrentUser.OpenSubKey(currentRegistryKey))
                    {
                        if (existingKey != null)
                        {
                            // 键已存在，跳过创建
                            continue;
                        }
                    }

                    try
                    {
                        // 添加命令到注册表
                        RegistryKey registryKey;
                        registryKey = Registry.CurrentUser.CreateSubKey(currentRegistryKey);
                        registryKey.SetValue("icon", executablePath);
                        registryKey.SetValue("position", "-");
                        registryKey = Registry.CurrentUser.CreateSubKey($"{currentRegistryKey}\\command");
                        registryKey.SetValue(string.Empty, $"\"{executablePath}\" \"%1\"");
                        registryKey.Close();
                    }
                    catch (Exception keyEx)
                    {
                        // 处理单个键的错误
                        allSucceeded = false;
                        MessageBox.Show($"添加注册表键 {currentRegistryKey} 时出错。{Environment.NewLine}错误信息：{keyEx.Message}",
                            "警告",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                }

                // 根据注册表键更新程序状态
                this.UpdateByRegistryKey();

                // 通知用户（如果全部成功且需要通知）
                if (allSucceeded)
                {
                    //MessageBox.Show($"{this.contextMenuText} 右键菜单已添加！{Environment.NewLine}{Environment.NewLine}在资源管理器中右键点击文件夹和文件即可使用。",
                    //    "成功",
                    //    MessageBoxButton.OK,
                    //    MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                // 通知用户
                MessageBox.Show($"添加 {this.contextMenuText.ToLowerInvariant()} 右键菜单到注册表时出错。{Environment.NewLine}{Environment.NewLine}错误信息：{Environment.NewLine}{ex.Message}",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 处理移除按钮点击事件
        /// </summary>
        /// <param name="sender">发送者对象</param>
        /// <param name="e">事件参数</param>
        private void OnRemoveButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                bool allSucceeded = true;

                // 遍历注册表键
                foreach (string currentRegistryKey in this.registryKeyList)
                {
                    try
                    {
                        // 检查键是否存在
                        using (var existingKey = Registry.CurrentUser.OpenSubKey(currentRegistryKey))
                        {
                            if (existingKey == null)
                            {
                                // 键不存在，跳过删除
                                continue;
                            }
                        }

                        // 从注册表移除命令
                        Registry.CurrentUser.DeleteSubKeyTree(currentRegistryKey);
                    }
                    catch (Exception keyEx)
                    {
                        // 处理单个键的错误
                        allSucceeded = false;
                        MessageBox.Show($"移除注册表键 {currentRegistryKey} 时出错。{Environment.NewLine}错误信息：{keyEx.Message}",
                            "警告",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                }

                // 根据注册表键更新程序状态
                this.UpdateByRegistryKey();

                // 通知用户（如果全部成功且需要通知）
                if (allSucceeded)
                {
                    //MessageBox.Show($"{this.contextMenuText} 右键菜单已移除。",
                    //    "成功",
                    //    MessageBoxButton.OK,
                    //    MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                // 通知用户
                MessageBox.Show($"从注册表移除 {this.contextMenuText.ToLowerInvariant()} 命令时出错。{Environment.NewLine}{Environment.NewLine}错误信息：{Environment.NewLine}{ex.Message}",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}