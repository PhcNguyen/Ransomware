// Copyright (C) PhcNguyen Developers
// Distributed under the terms of the Modified BSD License.

using System.Xml.Linq;

namespace Ransomware.Sources.Utils;

static class Config
{
    private static readonly string configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "config.xml");

    public static HashSet<string> LoadExtensions()
    {
        if (!File.Exists(configFile)) return new HashSet<string>();

        var extensions = XDocument.Load(configFile)
            .Descendants("extension")
            .Select(x => x.Value)
            .ToHashSet();

        return extensions.Count > 0 ? extensions : new HashSet<string>();
    }

    public static (string host, int port) LoadNetwork()
    {
        var networkElement = XDocument.Load(configFile).Element("config")?.Element("network");
        if (networkElement == null) throw new InvalidOperationException("Network configuration is missing.");

        string host = networkElement.Element("host")?.Value ?? "127.0.0.1";
        int port = int.TryParse(networkElement.Element("port")?.Value, out int parsedPort) ? parsedPort : 8080;

        return (host, port);
    }
}
