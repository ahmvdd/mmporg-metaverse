using UnityEngine;

public class CharacterController : MonoBehaviour {
    public NetworkManager networkManager;

    void Update() {
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0) {
            string moveMsg = $"MOVE|{transform.position.x}|{transform.position.z}";
            networkManager.SendMessage(moveMsg);
        }
    }
}