using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ransomware.Sources.Utils;
public class FileDetail
{
    public string FileName { get; set; } // Tên file
    public string FilePath { get; set; } // Đường dẫn đầy đủ
    public long Size { get; set; } // Kích thước file

    public FileDetail(string filePath, long fileSize)
    {
        FileName = Path.GetFileName(filePath);
        FilePath = filePath;
        Size = fileSize;
    }

    public string FormatSize()
    {
        if (Size >= 1024 * 1024 * 1024) // GB
            return $"{Size / (1024.0 * 1024 * 1024):F2} GB";

        else if (Size >= 1024 * 1024) // MB
            return $"{Size / (1024.0 * 1024):F2} MB";

        else if (Size >= 1024) // KB
            return $"{Size / 1024.0:F2} KB";
        
        else
            return $"{Size} bytes"; // bytes
    }
}
