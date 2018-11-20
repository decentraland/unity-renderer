using DCL.Configuration;
using UnityEngine;

public class CharacterController : MonoBehaviour {
  public float aimingHorizontalSpeed = 100f;
  public float aimingVerticalSpeed = 100f;
  public float aimingVerticalMinimumAngle = -60f;
  public float aimingVerticalMaximumAngle = 30f;
  public float movementSpeed = 5f;
  public float jumpForce = 150f;

  float aimingHorizontalAngle;
  float aimingVerticalAngle;

  new Transform camera;
  new Rigidbody rigidbody;
  new Collider collider;

  Vector3 originalPosition;
  Vector3 movementDirection;
  bool flyingMode = false;

  GameObject ground;

  void Awake() {
    ground = GameObject.Find("Ground");

    originalPosition = transform.position;
    camera = GetComponentInChildren<Camera>().transform;
    rigidbody = GetComponent<Rigidbody>();
    collider = GetComponent<Collider>();
  }

  void Update() {
    var didMove = false;

    if (Cursor.visible) return;

    if (Input.GetKeyDown(KeyCode.P)) {
      transform.position = originalPosition;
    } else if (Input.GetKeyDown(KeyCode.F)) {
      flyingMode = !flyingMode;

      rigidbody.useGravity = !flyingMode;
      rigidbody.isKinematic = flyingMode;
    }

    // Aiming
    var currentVerticalAxis = Input.GetAxis("Mouse Y");
    var currentHorizontalAxis = Input.GetAxis("Mouse X");

    aimingVerticalAngle += Mathf.Clamp(currentVerticalAxis, -1, 1) * aimingVerticalSpeed * Time.deltaTime;
    aimingHorizontalAngle += Mathf.Clamp(currentHorizontalAxis, -1, 1) * aimingHorizontalSpeed * Time.deltaTime;

    // Limit vertical aiming angle
    aimingVerticalAngle = Mathf.Clamp(aimingVerticalAngle, aimingVerticalMinimumAngle, aimingVerticalMaximumAngle);

    transform.rotation = Quaternion.Euler(0f, aimingHorizontalAngle, 0f);
    camera.localRotation = Quaternion.Euler(-aimingVerticalAngle, 0f, 0f);

    // Movement
    movementDirection = Vector3.zero;

    if (Input.GetAxis("Vertical") > 0f) {
      movementDirection += (transform.forward * movementSpeed * Time.deltaTime);
      didMove = true;
    } else if (Input.GetAxis("Vertical") < 0f) {
      movementDirection += (-transform.forward * movementSpeed * Time.deltaTime);
      didMove = true;
    }

    if (Input.GetAxis("Horizontal") > 0f) {
      movementDirection += (transform.right * movementSpeed * Time.deltaTime);
      didMove = true;
    } else if (Input.GetAxis("Horizontal") < 0f) {
      movementDirection += (-transform.right * movementSpeed * Time.deltaTime);
      didMove = true;
    }

    if (movementDirection != Vector3.zero &&
       (flyingMode || !Physics.Raycast(transform.position, movementDirection.normalized, collider.bounds.extents.x + 0.5f))) { // Wall-Collision check
      transform.position += movementDirection;
      didMove = true;
    }

    // Jump
    if (!flyingMode && Input.GetKeyDown(KeyCode.Space) && Physics.Raycast(transform.position, Vector3.down, collider.bounds.extents.y + 0.1f)) {
      rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Acceleration);
      didMove = true;
    }

    if (didMove) {
      DCL.Interface.WebInterface.ReportPosition(transform.position);

      ground.transform.position = new Vector3(
        Mathf.Floor(transform.position.x / ParcelSettings.PARCEL_SIZE) * ParcelSettings.PARCEL_SIZE,
        0f,
        Mathf.Floor(transform.position.z / ParcelSettings.PARCEL_SIZE) * ParcelSettings.PARCEL_SIZE
      );
    }
  }
}
