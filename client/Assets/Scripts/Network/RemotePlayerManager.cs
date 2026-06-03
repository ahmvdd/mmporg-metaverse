using UnityEngine;
using System.Collections.Generic;

public class RemotePlayerManager : MonoBehaviour {
    public GameObject remotePlayerPrefab;
    private Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();

    public void UpdateRemotePlayer(string id, Vector3 pos) {
        if (!players.ContainsKey(id)) {
            players[id] = Instantiate(remotePlayerPrefab, pos, Quaternion.identity);
        }
        players[id].transform.position = pos;
    }
}