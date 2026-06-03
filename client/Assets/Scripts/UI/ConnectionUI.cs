

using UnityEngine;
using UnityEngine.UI;

public class ConnectionUI : MonoBehaviour {
    public InputField idInput;
    public NetworkManager networkManager;

    public void OnConnectClick() {
        networkManager.Connect(idInput.text);
    }
}