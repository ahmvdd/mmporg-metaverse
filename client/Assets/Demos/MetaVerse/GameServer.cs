using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.IO;

public class GameServer : MonoBehaviour
{
    public static GameServer Instance;

    public bool IsRunning => isRunning;

    private TcpListener serveur;
    private Thread serverThread;
    private bool isRunning = false;

    private List<TcpClient> clients = new List<TcpClient>();
    private Dictionary<TcpClient, string> clientIds = new Dictionary<TcpClient, string>();
    private HashSet<string> collectedObjects = new HashSet<string>();

    private Queue<System.Action> mainThreadQueue = new Queue<System.Action>();
    private readonly object mainThreadLock = new object();
    private float pingTimer = 0f;
    private const float PingInterval = 5f;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        while (true)
        {
            System.Action action = null;
            lock (mainThreadLock)
            {
                if (mainThreadQueue.Count > 0)
                    action = mainThreadQueue.Dequeue();
            }
            if (action == null) break;
            action();
        }

        if (isRunning)
        {
            pingTimer += Time.deltaTime;
            if (pingTimer >= PingInterval)
            {
                pingTimer = 0f;
                BroadcastAll("PING");
            }
        }
    }

    public void StartServer(int port = 5555)
    {
        isRunning = true;
        serveur = new TcpListener(IPAddress.Any, port);
        serveur.Start();

        string localIPs = GetLocalIPAddresses();
        Debug.Log($"=== SERVEUR DÉMARRÉ ===\nPort : {port}\nIPs locales :\n{localIPs}\n=> Ton ami doit utiliser l'une de ces IPs pour rejoindre.");

        serverThread = new Thread(AcceptClients);
        serverThread.IsBackground = true;
        serverThread.Start();
    }

    private string GetLocalIPAddresses()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up) continue;
            if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
            foreach (UnicastIPAddressInformation addr in ni.GetIPProperties().UnicastAddresses)
            {
                if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    sb.AppendLine($"  [{ni.Name}] {addr.Address}");
            }
        }
        return sb.Length > 0 ? sb.ToString() : "  (aucune IP trouvée)";
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

                switch (type)
                {
                    case "CONNECT":
                        playerId = parts[1];
                        lock (clientIds)
                        {
                            foreach (var kv in clientIds)
                                SendTo(client, $"CONNECT|{kv.Value}");
                            clientIds[client] = playerId;
                        }
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

                    case "PONG":
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
                lock (mainThreadLock)
                {
                    mainThreadQueue.Enqueue(() => StartCoroutine(ResetBonus(objectId, 5f)));
                }
            }
            else
            {
                SendTo(expediteur, $"COLLECT_DENIED|{playerId}|{objectId}");
            }
        }
    }

    private System.Collections.IEnumerator ResetBonus(string objectId, float delay)
    {
        yield return new UnityEngine.WaitForSeconds(delay);
        lock (collectedObjects) { collectedObjects.Remove(objectId); }
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

    public void BroadcastAll(string message)
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
    }
}
