using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.VisualBasic;

namespace FolderComments
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 读取文件夹自定义设置的标志
        /// </summary>
        private static UInt32 FCS_READ = 0x00000001;

        /// <summary>
        /// 强制写入文件夹自定义设置的标志
        /// </summary>
        private static UInt32 FCS_FORCEWRITE = 0x00000002;

        /// <summary>
        /// 文件夹信息提示标志
        /// </summary>
        private static UInt32 FCSM_INFOTIP = 0x00000004;

        /// <summary>
        /// 获取或设置文件夹自定义设置
        /// </summary>
        /// <returns>返回操作结果</returns>
        /// <param name="pfcs">文件夹自定义设置结构体</param>
        /// <param name="pszPath">文件夹路径</param>
        /// <param name="dwReadWrite">读写标志</param>
        [DllImport("Shell32.dll", CharSet = CharSet.Auto)]
        private static extern UInt32 SHGetSetFolderCustomSettings(ref LPSHFOLDERCUSTOMSETTINGS pfcs, string pszPath, UInt32 dwReadWrite);

        /// <summary>
        /// 文件夹自定义设置结构体
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct LPSHFOLDERCUSTOMSETTINGS
        {
            public UInt32 dwSize; // 结构体大小
            public UInt32 dwMask; // 掩码
            public IntPtr pvid; // 视图ID
            public string pszWebViewTemplate; // Web视图模板路径
            public UInt32 cchWebViewTemplate; // Web视图模板路径长度
            public string pszWebViewTemplateVersion; // Web视图模板版本
            public string pszInfoTip; // 信息提示
            public UInt32 cchInfoTip; // 信息提示长度
            public IntPtr pclsid; // 类ID
            public UInt32 dwFlags; // 标志
            public string pszIconFile; // 图标文件路径
            public UInt32 cchIconFile; // 图标文件路径长度
            public int iIconIndex; // 图标索引
            public string pszLogo; // 徽标路径
            public UInt32 cchLogo; // 徽标路径长度
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // 检查参数
            if (e.Args.Length > 0)
            {
                // 设置路径
                string path = e.Args[0];

                // 检查是文件还是文件夹
                bool isDirectory = Directory.Exists(path);
                bool isFile = File.Exists(path);

                // 如果既不是文件也不是文件夹，则停止流程
                if (!isDirectory && !isFile)
                {
                    return;
                }

                // 如果是文件，获取其所在文件夹
                string directoryPath = isDirectory ? path : Path.GetDirectoryName(path);

                // 错误处理和日志记录
                try
                {
                    // 声明当前注释
                    string currentComments = string.Empty;

                    /* 获取当前注释 */
                    // 设置缓冲区大小
                    const int BUFFERSIZE = 4096;

                    // 获取设置
                    LPSHFOLDERCUSTOMSETTINGS folderCustomSettings = new LPSHFOLDERCUSTOMSETTINGS
                    {
                        dwMask = FCSM_INFOTIP,
                        pszInfoTip = new String(' ', BUFFERSIZE),
                        cchInfoTip = BUFFERSIZE
                    };

                    // 设置结构体大小
                    folderCustomSettings.dwSize = (uint)Marshal.SizeOf(folderCustomSettings);

                    // 读取文件夹自定义设置
                    UInt32 HRESULT = SHGetSetFolderCustomSettings(ref folderCustomSettings, directoryPath, FCS_READ);

                    // 检查是否成功
                    if (HRESULT == 0)
                    {
                        // 获取之前的目录注释
                        currentComments = folderCustomSettings.pszInfoTip;
                    }

                    /* 设置当前注释 */
                    // 让用户编辑注释
                    var inputDialog = new InputDialog("", isFile ? "编辑文件所在文件夹注释" : "编辑注释", currentComments);
                    if (inputDialog.ShowDialog() == true)
                    {
                        currentComments = inputDialog.InputText;
                    }
                    else
                    {
                        // 用户点击了取消，可以在此处理取消操作
                        return;
                    }

                    // 设置新的注释
                    LPSHFOLDERCUSTOMSETTINGS FolderCustomSettings = new LPSHFOLDERCUSTOMSETTINGS
                    {
                        dwMask = FCSM_INFOTIP,
                        pszInfoTip = currentComments.Length > 0 ? currentComments : null,
                        cchInfoTip = 0
                    };

                    // 写入文件夹自定义设置
                    HRESULT = SHGetSetFolderCustomSettings(ref FolderCustomSettings, directoryPath, FCS_FORCEWRITE);
                }
                catch (Exception exception)
                {
                    // 通知用户
                    MessageBox.Show(
                        $"设置文件夹注释失败。{Environment.NewLine}请检查错误日志以获取详细信息。",
                        "文件夹注释错误",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    try
                    {
                        // 记录错误事件
                        File.AppendAllText("FolderComments-ErrorLog.txt", $"设置文件夹注释失败。路径: {directoryPath}{Environment.NewLine}错误信息: {exception.Message}{Environment.NewLine}{Environment.NewLine}");
                    }
                    catch (Exception fileAppendException)
                    {
                        // 通知用户
                        MessageBox.Show(
                            $"写入 \"FolderComments-ErrorLog.txt\" 文件时出错。{Environment.NewLine}{Environment.NewLine}错误信息:{Environment.NewLine}{fileAppendException.Message}",
                            "文件追加错误",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                // 启动主窗口（WPF方式）
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
        }

    }
}