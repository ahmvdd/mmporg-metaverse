using UnityEngine;
using TMPro;

public class RemotePlayer : MonoBehaviour
{
    private Vector3 targetPosition;
    private float targetRotationY;
    private Vector3 velocity;
    private float lastUpdateTime;

    private TMP_Text scoreLabel;

    void Start()
    {
        targetPosition = transform.position;
        targetRotationY = transform.eulerAngles.y;
        lastUpdateTime = Time.time;

        scoreLabel = GetComponentInChildren<TMP_Text>();
        if (scoreLabel != null) scoreLabel.text = "0";
    }

    public void UpdateScore(int score)
    {
        if (scoreLabel != null) scoreLabel.text = score.ToString();
    }

    void Update()
    {
        // Extrapolation : prédit où sera le joueur en prolongeant sa trajectoire
        float timeSinceUpdate = Time.time - lastUpdateTime;
        Vector3 extrapolated = targetPosition + velocity * Mathf.Min(timeSinceUpdate, 0.15f);

        transform.position = Vector3.Lerp(transform.position, extrapolated, 15f * Time.deltaTime);
        float newY = Mathf.LerpAngle(transform.eulerAngles.y, targetRotationY, 15f * Time.deltaTime);
        transform.eulerAngles = new Vector3(0, newY, 0);
    }

    public void SetTarget(float x, float y, float z, float rotY)
    {
        Vector3 newTarget = new Vector3(x, y, z);
        float dt = Time.time - lastUpdateTime;
        velocity = (dt > 0f && dt < 0.5f) ? (newTarget - targetPosition) / dt : Vector3.zero;
        targetPosition = newTarget;
        targetRotationY = rotY;
        lastUpdateTime = Time.time;
    }
}
