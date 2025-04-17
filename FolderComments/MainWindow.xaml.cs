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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// The context menu text.
        /// </summary>
        private string contextMenuText = "Edit comments";

        /// <summary>
        /// The registry key list.
        /// </summary>
        private List<string> registryKeyList = new List<string>();

        public MainWindow()
        {
            InitializeComponent();

            /* Set values */

            // Add context menu item text
            this.registryKeyList.Add($"Software\\Classes\\directory\\shell\\{this.contextMenuText}");

            // Update the program by registry key
            this.UpdateByRegistryKey();
        }

        /// <summary>
        /// Updates the program by registry key.
        /// </summary>
        private void UpdateByRegistryKey()
        {
            // Update by registry key
            using (var registryKey = Registry.CurrentUser.OpenSubKey(this.registryKeyList[0]))
            {
                // Check for no returned registry key
                if (registryKey == null)
                {
                    // Disable remove button
                    this.removeButton.IsEnabled = false;

                    // Enable add button
                    this.addButton.IsEnabled = true;

                    // Update status text
                    this.activityToolStripStatusLabel.Text = "右键菜单状态: 未注册";

                    // Update status indicator color
                    this.statusIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F8C8D"));
                }
                else
                {
                    // Disable add button
                    this.addButton.IsEnabled = false;

                    // Enable remove button
                    this.removeButton.IsEnabled = true;

                    // Update status text
                    this.activityToolStripStatusLabel.Text = "右键菜单状态: 已注册";

                    // Update status indicator color
                    this.statusIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60"));
                }
            }
        }

        /// <summary>
        /// Handles the add button click.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnAddButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string executablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string startupPath = System.IO.Path.GetDirectoryName(executablePath);

                // Iterate registry keys
                foreach (string currentRegistryKey in this.registryKeyList)
                {
                    // Add command to registry
                    RegistryKey registryKey;
                    registryKey = Registry.CurrentUser.CreateSubKey(currentRegistryKey);
                    registryKey.SetValue("icon", executablePath);
                    registryKey.SetValue("position", "-");
                    registryKey = Registry.CurrentUser.CreateSubKey($"{currentRegistryKey}\\command");
                    registryKey.SetValue(string.Empty, $"\"{executablePath}\" \"%1\"");
                    registryKey.Close();
                }

                // Update the program by registry key
                this.UpdateByRegistryKey();

                // Notify user
                MessageBox.Show($"{this.contextMenuText} 右键菜单已添加！{Environment.NewLine}{Environment.NewLine}在资源管理器中右键点击文件夹即可使用。",
                    "成功",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Notify user
                MessageBox.Show($"添加 {this.contextMenuText.ToLowerInvariant()} 右键菜单到注册表时出错。{Environment.NewLine}{Environment.NewLine}错误信息：{Environment.NewLine}{ex.Message}",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the remove button click.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnRemoveButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Iterate registry keys 
                foreach (string currentRegistryKey in this.registryKeyList)
                {
                    // Remove command from registry
                    Registry.CurrentUser.DeleteSubKeyTree(currentRegistryKey);
                }

                // Update the program by registry key
                this.UpdateByRegistryKey();

                // Notify user
                MessageBox.Show($"{this.contextMenuText} 右键菜单已移除。",
                    "成功",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Notify user
                MessageBox.Show($"从注册表移除 {this.contextMenuText.ToLowerInvariant()} 命令时出错。{Environment.NewLine}{Environment.NewLine}错误信息：{Environment.NewLine}{ex.Message}",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}