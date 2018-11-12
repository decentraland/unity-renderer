using UnityEngine;

public class CharacterController : MonoBehaviour {
  public float aimingHorizontalSpeed = 400f;
  public float aimingVerticalSpeed = 400f;
  public float aimingVerticalMinimumAngle = -60f;
  public float aimingVerticalMaximumAngle = 30f;
  public float movementSpeed = 5f;
  public float jumpForce = 150f;

  float currentHorizontalAxis;
  float currentVerticalAxis;
  float aimingHorizontalAngle;
  float aimingVerticalAngle;
  Transform camera;
  Rigidbody rigidbody;
  Collider collider;
  Vector3 originalPosition;
  Vector3 movementDirection;
  bool flyingMode = false;

  void Awake() {
    originalPosition = transform.position;

    camera = GetComponentInChildren<Camera>().transform;
    rigidbody = GetComponent<Rigidbody>();
    collider = GetComponent<Collider>();
  }

  void Update() {
#if !UNITY_EDITOR
    if (Cursor.visible) return;
#endif

    if (Input.GetKeyDown(KeyCode.P)) {
      transform.position = originalPosition;
    } else if (Input.GetKeyDown(KeyCode.O)) {
      flyingMode = !flyingMode;

      rigidbody.useGravity = !flyingMode;
      rigidbody.isKinematic = flyingMode;
    }

    // Aiming
    currentVerticalAxis = Input.GetAxis("Mouse Y");
    currentHorizontalAxis = Input.GetAxis("Mouse X");

    aimingVerticalAngle += Mathf.Clamp(currentVerticalAxis, -1, 1) * aimingVerticalSpeed * Time.deltaTime;
    aimingHorizontalAngle += Mathf.Clamp(currentHorizontalAxis, -1, 1) * aimingHorizontalSpeed * Time.deltaTime;

    // Limit vertical aiming angle
    aimingVerticalAngle = Mathf.Clamp(aimingVerticalAngle, aimingVerticalMinimumAngle, aimingVerticalMaximumAngle);

    transform.rotation = Quaternion.Euler(0f, aimingHorizontalAngle, 0f);
    camera.localRotation = Quaternion.Euler(-aimingVerticalAngle, 0f, 0f);

    // Movement
    movementDirection = Vector3.zero;

    if (Input.GetAxis("Vertical") > 0f)
      movementDirection += (transform.forward * movementSpeed * Time.deltaTime);
    else if (Input.GetAxis("Vertical") < 0f)
      movementDirection += (-transform.forward * movementSpeed * Time.deltaTime);

    if (Input.GetAxis("Horizontal") > 0f)
      movementDirection += (transform.right * movementSpeed * Time.deltaTime);
    else if (Input.GetAxis("Horizontal") < 0f)
      movementDirection += (-transform.right * movementSpeed * Time.deltaTime);

    if (movementDirection != Vector3.zero &&
       (flyingMode || !Physics.Raycast(transform.position, movementDirection.normalized, collider.bounds.extents.x + 0.5f))) // Wall-Collision check
      transform.position += movementDirection;

    // Jump
    if (!flyingMode && Input.GetKeyDown(KeyCode.Space) && Physics.Raycast(transform.position, Vector3.down, collider.bounds.extents.y + 0.1f))
      rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Acceleration);
  }
}
