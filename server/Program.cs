using System.Net;
using System.Net.Sockets;
using System.Text;

Console.WriteLine("Serveur MetaVerse démarré sur le port 5555...");

TcpListener serveur = new TcpListener(IPAddress.Any, 5555);
serveur.Start();

List<TcpClient> clients = new List<TcpClient>();
while (true)
{
    TcpClient client = await serveur.AcceptTcpClientAsync();
    clients.Add(client);
    Console.WriteLine($"Nouveau joueur connecté ! Total : {clients.Count}");

    // Gérer ce client dans un thread séparé
    _ = Task.Run(() => GererClient(client, clients));
}

async Task GererClient(TcpClient client, List<TcpClient> tousLesClients)
{
    NetworkStream stream = client.GetStream();
    byte[] buffer = new byte[1024];

    try
    {
        while (true)
        {
            int bytesLus = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesLus == 0) break; // client déconnecté

            string message = Encoding.UTF8.GetString(buffer, 0, bytesLus);
            Console.WriteLine($"Reçu : {message}");

            // Broadcast à tous les autres clients
            foreach (TcpClient autre in tousLesClients)
            {
                if (autre != client && autre.Connected)
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    await autre.GetStream().WriteAsync(data, 0, data.Length);
                }
            }
        }
    }
    catch
    {
        Console.WriteLine("Un joueur s'est déconnecté.");
    }
    finally
    {
        tousLesClients.Remove(client);
        client.Close();
    }
}