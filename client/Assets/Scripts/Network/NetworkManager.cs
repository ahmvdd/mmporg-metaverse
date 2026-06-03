using UnityEngine;
using System.Collections.Generic;
using System.Globalization;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    public string ServerIP = "127.0.0.1";
    public int ServerPort = 5555;
    public GameObject RemotePlayerPrefab;

    public string PlayerId { get; private set; }

    TCPClient tcp;
    Dictionary<string, GameObject> remotePlayers = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        PlayerId = System.Guid.NewGuid().ToString("N").Substring(0, 8);
    }

    void Start()
    {
        tcp = GetComponent<TCPClient>();
        tcp.DestinationIP = ServerIP;
        tcp.DestinationPort = ServerPort;
        bool ok = tcp.Connect(OnMessageReceived);
        if (ok)
        {
            tcp.SendTCPMessage($"CONNECT|{PlayerId}");
            Debug.Log($"Connecté au serveur en tant que {PlayerId}");
        }
        else
            Debug.LogWarning("Connexion au serveur échouée.");
    }

    public void SendPosition(Vector3 pos, float rotY)
    {
        if (!tcp.IsConnected) return;
        tcp.SendTCPMessage(string.Format(CultureInfo.InvariantCulture,
            "MOVE|{0}|{1}|{2}|{3}|{4}", PlayerId, pos.x, pos.y, pos.z, rotY));
    }

    void OnMessageReceived(string message)
    {
        string[] parts = message.Trim().Split('|');
        if (parts.Length < 2) return;

        string type = parts[0];
        string id = parts[1];

        if (id == PlayerId) return;

        switch (type)
        {
            case "CONNECT":
                if (!remotePlayers.ContainsKey(id))
                {
                    GameObject go = Instantiate(RemotePlayerPrefab, Vector3.zero, Quaternion.identity);
                    remotePlayers[id] = go;
                    Debug.Log($"Nouveau joueur distant : {id}");
                }
                break;

            case "MOVE":
                if (parts.Length < 6) return;
                float x = float.Parse(parts[2], CultureInfo.InvariantCulture);
                float y = float.Parse(parts[3], CultureInfo.InvariantCulture);
                float z = float.Parse(parts[4], CultureInfo.InvariantCulture);
                float rot = float.Parse(parts[5], CultureInfo.InvariantCulture);

                if (!remotePlayers.ContainsKey(id))
                {
                    GameObject go = Instantiate(RemotePlayerPrefab,
                        new Vector3(x, y, z), Quaternion.identity);
                    remotePlayers[id] = go;
                }
                RemotePlayer rp = remotePlayers[id].GetComponent<RemotePlayer>();
                if (rp != null) rp.SetTarget(x, y, z, rot);
                break;

            case "DISCONNECT":
                if (remotePlayers.ContainsKey(id))
                {
                    Destroy(remotePlayers[id]);
                    remotePlayers.Remove(id);
                    Debug.Log($"Joueur distant déconnecté : {id}");
                }
                break;
        }
    }

    void OnDisable()
    {
        if (tcp != null && tcp.IsConnected)
            tcp.SendTCPMessage($"DISCONNECT|{PlayerId}");
        tcp?.Close();
    }
}
