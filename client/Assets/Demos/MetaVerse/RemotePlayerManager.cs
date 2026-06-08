using UnityEngine;
using System.Collections.Generic;

public class RemotePlayerManager : MonoBehaviour
{
  public GameObject RemotePlayerPrefab;
  private Dictionary<string, RemotePlayer> remotePlayers = new Dictionary<string, RemotePlayer>();

  public void HandleMessage(string message)
  {
    string[] parts = message.Split('|');
    string type = parts[0];
    switch (type)
    {
      case "CONNECT": HandleConnect(parts[1]); break;
      case "MOVE": HandleMove(parts); break;
      case "DISCONNECT": HandleDisconnect(parts[1]); break;
    }
  }

  private void HandleConnect(string id)
  {
    if (remotePlayers.ContainsKey(id)) return;
    GameObject go = Instantiate(RemotePlayerPrefab, Vector3.zero, Quaternion.identity);
    RemotePlayer rp = go.GetComponent<RemotePlayer>();
    remotePlayers[id] = rp;
  }

  private void HandleMove(string[] parts)
  {
    string id = parts[1];
    if (!remotePlayers.ContainsKey(id)) return;
    float x = float.Parse(parts[2]);
    float y = float.Parse(parts[3]);
    float z = float.Parse(parts[4]);
    float rotY = float.Parse(parts[5]);
    remotePlayers[id].SetTarget(x, y, z, rotY);
  }

  private void HandleDisconnect(string id)
  {
    if (!remotePlayers.ContainsKey(id)) return;
    Destroy(remotePlayers[id].gameObject);
    remotePlayers.Remove(id);
  }
}