using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;

namespace QuickLook.Plugin.IDManViewer;

internal static class IDManWindow
{
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

    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowText(nint hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetClassName(nint hWnd, StringBuilder lpClassName, int nMaxCount);
}
