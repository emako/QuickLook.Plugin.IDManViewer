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

using QuickLook.Common.Plugin;
using System;
using System.IO;
using System.Windows;

namespace QuickLook.Plugin.IDManViewer;

public class Plugin : IViewer
{
    private SpaceKeyListener listener = null!;

    public int Priority => int.MinValue;

    public void Init()
    {
        listener = new SpaceKeyListener();
        listener.SpaceReleased += OnSpaceKeyReleased;
    }

    public bool CanHandle(string path)
    {
        return false;
    }

    public void Prepare(string path, ContextObject context)
    {
    }

    public void View(string path, ContextObject context)
    {
    }

    public void Cleanup()
    {
        GC.SuppressFinalize(this);
        listener.SpaceReleased -= OnSpaceKeyReleased;
    }

    private void OnSpaceKeyReleased()
    {
        if (IDManWindow.GetWindowName() is string windowName
         && IDManWindow.GetSelectItem(windowName) is string selectedItem
         && IDManWindow.GetFilePath(selectedItem) is string filePath)
        {
            if (File.Exists(filePath))
            {
                ViewWindowManager.GetInstance().InvokePreview(filePath);
            }
            else
            {
                MessageBox.Show($"File `{filePath}` is not found.", "QuickLook.Plugin.IDManViewer", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
