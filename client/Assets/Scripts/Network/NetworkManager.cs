// (Logique Client)

// Ce script envoie les messages de type CONNECT|ID, MOVE|X|Y, COLLECT|ID.
using UnityEngine;
using System.Net.Sockets;
using System.Text;

public class NetworkManager : MonoBehaviour {
    private TcpClient client;
    private NetworkStream stream;

    public void Connect(string playerId) {
        client = new TcpClient("127.0.0.1", 5555);
        stream = client.GetStream();
        SendMessage($"CONNECT|{playerId}");
    }

    public void SendMessage(string msg) {
        byte[] data = Encoding.UTF8.GetBytes(msg + "\n");
        stream.Write(data, 0, data.Length);
    }
}