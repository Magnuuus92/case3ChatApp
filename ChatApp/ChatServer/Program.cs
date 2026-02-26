using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatServer;

class Program
{
    static List<TcpClient> clients = new List<TcpClient>();
    //^statisk liste som holder på alle tilkoblede klientene.

    static async Task Main(string[] args)
    //^Main starter serveren og async tilrettelegger for bruk av await.
    {
        int port = 5000;
        //^bestemmer hvilken port serveren skal lytte til.

        var listener = new TcpListener(IPAddress.IPv6Any, port);
        // TcpListener starter en tcp server ipv6Any gjør at den lytter til alle nettverksort på maskinen

        listener.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
        // "IPv6only, false" gjør at samme socket kan ta ipv6 og ipv4.
        listener.Start();
        //^Her begynner serveren å faktisk lytte.
        Console.WriteLine($"Chat server started on port {port}");
        Console.WriteLine("Waiting for clients...");

        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            // venter til en klient kobles til. 
            // > Når en klient kobles til returnerer den en TcpClient
            clients.Add(client);
            //Legger klienten til clients lista sånn at den kan motta meldinger senere.

            Console.WriteLine("New client connected");

            _ = HandleClient(client);
            //starter håndtering av klienten. HandleClient starter en ny asyncron metode.
            //serveren kan fortsatt ta imot flere 
        }
    }
    static async Task HandleClient(TcpClient client)
    {
        var stream = client.GetStream();
        var buffer = new byte[1024];
        try
        {
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                    break;
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received {message}");
                await BroadCast(message, client);
            }
        }
        catch
        {
            Console.WriteLine("Client disconnected");
        }
        finally
        {
            clients.Remove(client);
            client.Close();
        }
    }
    static async Task BroadCast(string message, TcpClient sender)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        foreach (var client in clients)
        {
            if (client != sender)
            {
                try
                {
                    await client.GetStream().WriteAsync(data, 0, data.Length);
                }
                catch { }
            }
        }
    }
}
