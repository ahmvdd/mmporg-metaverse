public class CollectableManager : MonoBehaviour {
    public NetworkManager networkManager; // Glissez l'objet dans l'inspecteur

    public void RequestCollect(string bonusId) {
        // Envoi au serveur pour validation
        networkManager.SendMessage($"COLLECT|{bonusId}");
    }
}