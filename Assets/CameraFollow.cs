using UnityEngine;

[ExecuteAlways]
public class CameraFollow : MonoBehaviour
{
    private float currentHeight;

    private Quaternion currentRotation;

    private float currentRotationAngle;
    public float distance = 20.0f;
    public float distanceMultiplier;

    public float distanceSnapTime;
    public float height = 5.0f;
    public float heightDamping = 2.0f;

    public float lookAtHeight = 0.0f;

    private Vector3 lookAtVector;

    public Rigidbody parentRigidbody;

    public float rotationSnapTime = 0.3F;
    public Transform target;

    private float usedDistance;
    private float wantedHeight;
    private Vector3 wantedPosition;

    private float wantedRotationAngle;

    private float yVelocity;
    private float zVelocity;

    private void Start()
    {
        lookAtVector = new Vector3(0, lookAtHeight, 0);
    }

    private void LateUpdate()
    {
        wantedHeight = target.position.y + height;
        currentHeight = transform.position.y;

        wantedRotationAngle = target.eulerAngles.y;
        currentRotationAngle = transform.eulerAngles.y;

        // if (Input.GetKey(KeyCode.Q)) {
        //   wantedRotationAngle -= 90f;
        // }

        // if (Input.GetKey(KeyCode.E)) {
        //   wantedRotationAngle += 90f;
        // }

        currentRotationAngle =
            Mathf.SmoothDampAngle(currentRotationAngle, wantedRotationAngle, ref yVelocity, rotationSnapTime);

        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

        wantedPosition = target.position;
        wantedPosition.y = currentHeight;

        usedDistance = Mathf.SmoothDampAngle(usedDistance,
            distance + parentRigidbody.velocity.magnitude * distanceMultiplier, ref zVelocity, distanceSnapTime);

        wantedPosition += Quaternion.Euler(0, currentRotationAngle, 0) * new Vector3(0, 0, -usedDistance);

        transform.position = wantedPosition;

        transform.LookAt(target.position + lookAtVector);
    }
}