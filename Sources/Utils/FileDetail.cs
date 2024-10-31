using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ransomware.Sources.Utils;
public class FileDetail
{
<<<<<<< HEAD
    public long Size { get; set; } // Kích thước file
    public string FileName { get; set; } // Tên file
    public string FilePath { get; set; } // Đường dẫn đầy đủ
    

    public FileDetail(string filePath, long fileSize)
    {
        this.Size = fileSize;
        this.FilePath = filePath;
        this.FileName = Path.GetFileName(filePath);
=======
    public string FileName { get; set; } // Tên file
    public string FilePath { get; set; } // Đường dẫn đầy đủ
    public long Size { get; set; } // Kích thước file

    public FileDetail(string filePath, long fileSize)
    {
        FileName = Path.GetFileName(filePath);
        FilePath = filePath;
        Size = fileSize;
>>>>>>> 9354e9dd3a9a2a24d601710409caf0f9ccdcd5a4
    }

    public string FormatSize()
    {
<<<<<<< HEAD
        string[] units = { "bytes", "KB", "MB", "GB" };
        double size = this.Size;
        int unitIndex = 0;

        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:F2} {units[unitIndex]}";
=======
        if (Size >= 1024 * 1024 * 1024) // GB
            return $"{Size / (1024.0 * 1024 * 1024):F2} GB";

        else if (Size >= 1024 * 1024) // MB
            return $"{Size / (1024.0 * 1024):F2} MB";

        else if (Size >= 1024) // KB
            return $"{Size / 1024.0:F2} KB";
        
        else
            return $"{Size} bytes"; // bytes
>>>>>>> 9354e9dd3a9a2a24d601710409caf0f9ccdcd5a4
    }
}
