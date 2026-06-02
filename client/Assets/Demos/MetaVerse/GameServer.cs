using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.IO;

public class GameServer : MonoBehaviour
{
    public static GameServer Instance;

    private TcpListener serveur;
    private Thread serverThread;
    private bool isRunning = false;

    private List<TcpClient> clients = new List<TcpClient>();
    private Dictionary<TcpClient, string> clientIds = new Dictionary<TcpClient, string>();

    // Pour les race conditions sur les collectables
    private HashSet<string> collectedObjects = new HashSet<string>();

    void Awake()
    {
        Instance = this;
    }

    // Appelé par le bouton "Héberger" dans ConnectionUI
    public void StartServer(int port = 5555)
    {
        isRunning = true;
        serveur = new TcpListener(IPAddress.Any, port);
        serveur.Start();
        Debug.Log($"Serveur démarré sur le port {port}");

        serverThread = new Thread(AcceptClients);
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    void AcceptClients()
    {
        while (isRunning)
        {
            try
            {
                TcpClient client = serveur.AcceptTcpClient();
                lock (clients) { clients.Add(client); }
                Debug.Log($"Nouveau joueur ! Total : {clients.Count}");

                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.IsBackground = true;
                clientThread.Start();
            }
            catch { break; }
        }
    }

    void HandleClient(TcpClient client)
    {
        StreamReader reader = new StreamReader(client.GetStream(), Encoding.UTF8);
        string playerId = "";

        try
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] parts = line.Split('|');
                string type = parts[0];

                Debug.Log($"Reçu : {line}");

                switch (type)
                {
                    case "CONNECT":
                        playerId = parts[1];
                        lock (clientIds) { clientIds[client] = playerId; }
                        Broadcast(line, client);
                        break;

                    case "MOVE":
                        Broadcast(line, client);
                        break;

                    case "DISCONNECT":
                        BroadcastAll(line);
                        break;

                    case "COLLECT":
                        HandleCollect(parts, client);
                        break;
                }
            }
        }
        catch
        {
            Debug.Log($"Déconnexion brutale : {playerId}");
        }
        finally
        {
            if (playerId != "")
                BroadcastAll($"DISCONNECT|{playerId}");

            lock (clients) { clients.Remove(client); }
            lock (clientIds) { clientIds.Remove(client); }
            client.Close();
        }
    }

    // Race condition — premier arrivé premier servi
    void HandleCollect(string[] parts, TcpClient expediteur)
    {
        string playerId = parts[1];
        string objectId = parts[2];

        lock (collectedObjects)
        {
            if (!collectedObjects.Contains(objectId))
            {
                collectedObjects.Add(objectId);
                BroadcastAll($"COLLECT_OK|{playerId}|{objectId}");
            }
            else
            {
                SendTo(expediteur, $"COLLECT_DENIED|{playerId}|{objectId}");
            }
        }
    }

    void Broadcast(string message, TcpClient expediteur)
    {
        byte[] data = Encoding.UTF8.GetBytes(message + "\n");
        lock (clients)
        {
            foreach (TcpClient c in clients)
            {
                if (c != expediteur && c.Connected)
                    try { c.GetStream().Write(data, 0, data.Length); } catch { }
            }
        }
    }

    void BroadcastAll(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message + "\n");
        lock (clients)
        {
            foreach (TcpClient c in clients)
            {
                if (c.Connected)
                    try { c.GetStream().Write(data, 0, data.Length); } catch { }
            }
        }
    }

    void SendTo(TcpClient client, string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message + "\n");
        try { client.GetStream().Write(data, 0, data.Length); } catch { }
    }

    void OnDestroy()
    {
        isRunning = false;
        serveur?.Stop();
        serverThread?.Abort();
    }
}