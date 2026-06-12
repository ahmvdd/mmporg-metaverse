using UnityEngine;
using System.Collections.Generic;

public class RemotePlayerManager : MonoBehaviour
{
  public GameObject RemotePlayerPrefab;

  private Dictionary<string, RemotePlayer> players = new Dictionary<string, RemotePlayer>();

  public void HandleMessage(string message)
  {
    string[] parts = message.Split('|');
    string type = parts[0];

    switch (type)
    {
      case "CONNECT":
        HandleConnect(parts[1]);
        break;

      case "MOVE":
        HandleMove(parts);
        break;

      case "DISCONNECT":
        HandleDisconnect(parts[1]);
        break;
    }
  }

  private void HandleConnect(string id)
  {
    Debug.Log("Joueur connecté : " + id);
  }

  private void HandleMove(string[] parts)
  {
    string id = parts[1];
    float x = float.Parse(parts[2]);
    float y = float.Parse(parts[3]);
    float z = float.Parse(parts[4]);
    float rotY = float.Parse(parts[5]);

    if (!players.ContainsKey(id))
    {
      GameObject go = Instantiate(RemotePlayerPrefab, new Vector3(x, y, z), Quaternion.identity);
      RemotePlayer rp = go.GetComponent<RemotePlayer>();
      players[id] = rp;
    }

    players[id].SetTarget(x, y, z, rotY);
  }

  private void HandleDisconnect(string id)
  {
    if (!players.ContainsKey(id)) return;

    Destroy(players[id].gameObject);
    players.Remove(id);

    Debug.Log("Joueur déconnecté : " + id);
  }
}