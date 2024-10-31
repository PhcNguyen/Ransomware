// Copyright (C) PhcNguyen Developers
// Distributed under the terms of the Modified BSD License.

using System.Xml.Linq;


namespace Ransomware.Sources.Utils;
static class Config
{
    public static HashSet<string> LoadExtensions()
    {
        // Lấy đường dẫn tới tệp config.xml
        string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.xml");

        // Kiểm tra xem tệp có tồn tại không
        if (!File.Exists(configPath))
        {
            return new HashSet<string>();
        }

        var doc = XDocument.Load(configPath);
        var extensions = doc.Descendants("extension").Select(x => x.Value).ToList();

        // Kiểm tra xem extensions có giá trị không
        if (extensions == null || extensions.Count == 0)
        {
            return new HashSet<string>();
        }

        return new HashSet<string>(extensions);
    }
}
