using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ransomware.Sources.Utils;
public class FileDetail
{
    public long Size { get; set; } // Kích thước file
    public string FileName { get; set; } // Tên file
    public string FilePath { get; set; } // Đường dẫn đầy đủ

    public FileDetail(string filePath, long fileSize)
    {
        this.Size = fileSize;
        this.FilePath = filePath;
        this.FileName = Path.GetFileName(filePath);
    }

    public string FormatSize()
    {
        string[] units = { "bytes", "KB", "MB", "GB" };
        double size = this.Size;
        int unitIndex = 0;

        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:F2} {units[unitIndex]}";
    }
}
