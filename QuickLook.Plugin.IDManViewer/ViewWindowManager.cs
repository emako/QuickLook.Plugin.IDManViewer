﻿// Copyright © 2025 QL-Win Contributors
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

using System;
using System.Linq;
using System.Reflection;

namespace QuickLook.Plugin.IDManViewer;

internal class ViewWindowManager
{
    private static ViewWindowManager? _instance;

    public static ViewWindowManager GetInstance()
    {
        return _instance ??= new ViewWindowManager();
    }

    public void InvokePreview(string path = null!)
    {
        Assembly? quickLook = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name?.Contains(nameof(QuickLook)) == true);
        if (quickLook == null) return;

        Type? viewWindowManagerType = quickLook.GetTypes().FirstOrDefault(t => t.Name == nameof(ViewWindowManager));
        if (viewWindowManagerType == null) return;

        MethodInfo? getInstanceMethod = viewWindowManagerType.GetMethod(nameof(GetInstance), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (getInstanceMethod == null) return;

        object? instance = getInstanceMethod.Invoke(null, null);
        if (instance == null) return;

        MethodInfo? invokePreviewMethod = viewWindowManagerType.GetMethod(nameof(InvokePreview), BindingFlags.Instance | BindingFlags.Public);
        if (invokePreviewMethod == null) return;

        invokePreviewMethod.Invoke(instance, [path]);
    }
}
