// Copyright (C) PhcNguyen Developers
// Distributed under the terms of the Modified BSD License.


using Ransomware.Sources.Application;
class Program
{
    static async Task Main()
    {
        var scanner = new FileExplorer(); // Tạo một đối tượng FileExplorer
        var fileDetails = scanner.Scan(1024 * 1024, FileExplorer.SortOrder.Descending); // Quét thư mục gốc

        Console.WriteLine($"Số lượng file cụ thể được lưu: {fileDetails.Count}");

        foreach (var fileDetail in fileDetails)
        {
            Console.WriteLine($"{fileDetail.FilePath} - {fileDetail.FormatSize()}");
            await Task.Delay(300);
        }
    }
}