using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;

public class GameServer : MonoBehaviour
{
    private TcpListener listener;
    private CollectableManager collectableManager; // Référence au manager
    private Dictionary<TcpClient, string> clientIds = new Dictionary<TcpClient, string>();

    void Start()
    {
        collectableManager = GetComponent<CollectableManager>();
        listener = new TcpListener(IPAddress.Any, 5555);
        listener.Start();
        Task.Run(() => AcceptClients());
    }

    async Task AcceptClients()
    {
        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            _ = Task.Run(() => GererClient(client)); // On lance la gestion
        }
    }

    async Task GererClient(TcpClient client)
    {
        var reader = new StreamReader(client.GetStream());
        string line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            string[] parts = line.Split('|');
            switch (parts[0])
            {
                case "COLLECT":
                    string bonusId = parts[1];
                    // On utilise le manager récupéré au Start
                    if (collectableManager.TryCollect(bonusId, out _))
                    {
                        // Logique de Broadcast ici
                        Debug.Log($"Objet {bonusId} collecté !");
                    }
                    break;
                    // ... vos autres cases ...
            }
        }
    }
}