// Copyright © 2025 QL-Win Contributors
//
// This file is part of QuickLook program.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Automation;

namespace QuickLook.Plugin.IDManViewer;

internal static class IDManWindow
{
    public static Dictionary<string, string> FoldersTree { get; private set; } = null!;

    public static string? GetWindowName()
    {
        nint hwnd = GetForegroundWindow();
        if (hwnd == IntPtr.Zero)
            return null;

        StringBuilder titleBuilder = new(256);
        GetWindowText(hwnd, titleBuilder, titleBuilder.Capacity);
        string windowTitle = titleBuilder.ToString();

        if (!windowTitle.StartsWith("Internet Download Manager", StringComparison.Ordinal))
            return null;

        StringBuilder classNameBuilder = new(256);
        GetClassName(hwnd, classNameBuilder, classNameBuilder.Capacity);
        string className = classNameBuilder.ToString();

        if (!className.Equals("#32770", StringComparison.Ordinal))
            return null;

        return windowTitle;
    }

    public static string? GetSelectItem(string windowName = "Internet Download Manager 6.42")
    {
        var idmWindow = AutomationElement.RootElement.FindFirst(TreeScope.Children,
            new PropertyCondition(AutomationElement.NameProperty, windowName));

        if (idmWindow == null)
            return null;

        var listView = idmWindow.FindFirst(TreeScope.Descendants,
            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.DataGrid))
            ?? idmWindow.FindFirst(TreeScope.Descendants,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.List));

        if (listView == null)
            return null;

        var selectedItems = listView.FindAll(TreeScope.Children,
            new PropertyCondition(AutomationElement.IsSelectionItemPatternAvailableProperty, true));

        foreach (AutomationElement item in selectedItems)
        {
            if (item.GetCurrentPattern(SelectionItemPattern.Pattern) is SelectionItemPattern selectionItemPattern
                && selectionItemPattern.Current.IsSelected)
            {
                string name = item.Current.Name;
                return name;
            }
        }

        return null;
    }

    public static string? GetFilePath(string? name)
    {
        PrepareFoldersTree();

        string ext = Path.GetExtension(name);

        if (FoldersTree.ContainsKey(ext))
        {
            return Path.Combine(FoldersTree[ext], name);
        }
        else
        {
            foreach (KeyValuePair<string, string> kv in FoldersTree.Where(kv => kv.Key.Contains("*")))
            {
                if (IsMatch(ext, kv.Key))
                {
                    return Path.Combine(kv.Value, name);
                }

                static bool IsMatch(string input, string pattern)
                {
                    // Escape regex special characters except '*'
                    string regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
                    return Regex.IsMatch(input, regexPattern, RegexOptions.IgnoreCase);
                }
            }
        }

        // The selected file has not been downloaded yet
        // Try to read the full name from IDM temporary folder
        {
            using RegistryKey baseKey = Registry.CurrentUser.OpenSubKey(@"Software\DownloadManager");

            if (baseKey == null)
            {
                return null;
            }

            foreach (string subKeyName in baseKey.GetSubKeyNames())
            {
                using RegistryKey subKey = baseKey.OpenSubKey(subKeyName);

                if (subKey != null)
                {
                    string? frfncd = subKey.GetValue("FR_FNCD") as string;

                    if (frfncd == name)
                    {
                        // Hit filename

                        if (subKey.GetValue("LocalFileName") is string localFileName)
                        {
                            return localFileName;
                        }
                        break;
                    }
                }
            }
            return null;
        }
    }

    private static void PrepareFoldersTree()
    {
        if (FoldersTree != null) return;
        FoldersTree = [];

        string? downloadPath = null;

        {
            using RegistryKey baseKey = Registry.CurrentUser.OpenSubKey(@"Software\DownloadManager");
            if (baseKey == null)
            {
                return;
            }

            object? localPathW = baseKey.GetValue("LocalPathW");

            if (baseKey.GetValue("LocalPathW") is byte[] localPathBytes)
            {
                downloadPath = Encoding.Unicode.GetString(localPathBytes).TrimEnd('\0');
            }
        }

        if (downloadPath is null) return;
        FoldersTree.Add("*", downloadPath);

        {
            using RegistryKey baseKey = Registry.CurrentUser.OpenSubKey(@"Software\DownloadManager\FoldersTree");
            if (baseKey == null)
            {
                return;
            }

            foreach (string subKeyName in baseKey.GetSubKeyNames())
            {
                using RegistryKey subKey = baseKey.OpenSubKey(subKeyName);

                if (subKey != null)
                {
                    if (subKey.GetValue("mask") is string maskValue)
                    {
                        string[] exts = maskValue.Split(' ');

                        foreach (string ext in exts)
                        {
                            FoldersTree.Add($".{ext}", Path.Combine(downloadPath, subKeyName));
                        }
                    }
                }
            }
        }
    }

    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowText(nint hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetClassName(nint hWnd, StringBuilder lpClassName, int nMaxCount);
}
