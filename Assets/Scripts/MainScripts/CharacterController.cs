using UnityEngine;

public class CharacterController : MonoBehaviour {
  public float aimingHorizontalSpeed = 400f;
  public float aimingVerticalSpeed = 400f;
  public float aimingVerticalMinimumAngle = -60f;
  public float aimingVerticalMaximumAngle = 30f;
  public float movementSpeed = 1f;
  public float jumpForce = 7f;

  float currentHorizontalAxis;
  float currentVerticalAxis;
  float aimingHorizontalAngle;
  float aimingVerticalAngle;
  Transform camera;
  Rigidbody rigidbody;
  Collider collider;

  void Awake() {
    camera = GetComponentInChildren<Camera>().transform;
    rigidbody = GetComponent<Rigidbody>();
    collider = GetComponent<Collider>();
  }

  void Update() {
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
    if (Input.GetAxis("Vertical") > 0f)
      transform.position += (transform.forward * movementSpeed * Time.deltaTime);
    else if (Input.GetAxis("Vertical") < 0f)
      transform.position += (-transform.forward * movementSpeed * Time.deltaTime);

    if (Input.GetAxis("Horizontal") > 0f)
      transform.position += (transform.right * movementSpeed * Time.deltaTime);
    else if (Input.GetAxis("Horizontal") < 0f)
      transform.position += (-transform.right * movementSpeed * Time.deltaTime);

    // Jump
    if (Input.GetKeyDown(KeyCode.Space)) {
      if (Physics.Raycast(transform.position, Vector3.down, collider.bounds.extents.y + 0.1f)) // Grounded check
        rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Acceleration);
    }
  }
}
