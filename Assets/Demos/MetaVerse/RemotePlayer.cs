using UnityEngine;

public class RemotePlayer : MonoBehaviour
{
  private Vector3 targetPosition;
  private float targetRotationY;

  void Start()
  {
    targetPosition = transform.position;
    targetRotationY = transform.eulerAngles.y;
  }

  void Update()
  {
    // Déplacement fluide vers la position cible
    transform.position = Vector3.Lerp(
        transform.position,
        targetPosition,
        10f * Time.deltaTime
    );

    // Rotation fluide
    float currentY = transform.eulerAngles.y;
    float newY = Mathf.LerpAngle(currentY, targetRotationY, 10f * Time.deltaTime);
    transform.eulerAngles = new Vector3(0, newY, 0);
  }

  public void SetTarget(float x, float y, float z, float rotY)
  {
    targetPosition = new Vector3(x, y, z);
    targetRotationY = rotY;
  }
}