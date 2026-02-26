using System.Net.Sockets;
using System.Text;

namespace ChatClient;

class Program
{
    static async Task Main(string[] args)
    {
        Console.Write("Enter server IP:  ");
        string? ip = Console.ReadLine();

        int port = 5000;
        var client = new TcpClient();
        await client.ConnectAsync(ip, port);
        Console.WriteLine("Connected to server");
        var stream = client.GetStream();
        _ = Task.Run(async () =>
        {
            var buffer = new byte[1024];
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                Console.WriteLine(" >> " +
                Encoding.UTF8.GetString(buffer, 0, bytesRead));
            }
        });
        while (true)
        {
            string? message = Console.ReadLine();
            if (string.IsNullOrEmpty(message)) continue;

            byte[] data = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(data, 0, data.Length);
        }
    }
}