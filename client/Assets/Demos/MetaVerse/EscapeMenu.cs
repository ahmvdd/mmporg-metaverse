using UnityEngine;
using UnityEngine.UI;

public class EscapeMenu : MonoBehaviour
{
    public GameObject MenuPanel;
    public Button DisconnectButton;
    public GameObject ConnectionUI;

    void Start()
    {
        MenuPanel.SetActive(false);
        DisconnectButton.onClick.AddListener(OnDisconnectClick);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Toggle();
    }

    void Toggle()
    {
        bool open = !MenuPanel.activeSelf;
        MenuPanel.SetActive(open);
        Time.timeScale = open ? 0f : 1f;
    }

    void OnDisconnectClick()
    {
        NetworkManager.Instance.Disconnect();
        Time.timeScale = 1f;
        MenuPanel.SetActive(false);
        if (ConnectionUI != null)
            ConnectionUI.SetActive(true);
    }
}
