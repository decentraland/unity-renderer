using DCL.Configuration;
using UnityEngine;

public class CharacterController : MonoBehaviour {
  float aimingHorizontalSpeed = 100f;
  float aimingVerticalSpeed = 100f;
  float aimingVerticalMinimumAngle = -60f;
  float aimingVerticalMaximumAngle = 30f;
  float movementSpeed = 5f;
  float jumpForce = 150f;
  float sunInclination = -0.31f;

  float aimingHorizontalAngle;
  float aimingVerticalAngle;

  new Transform camera;
  new Rigidbody rigidbody;
  new Collider collider;

  Vector3 originalPosition = Vector3.zero;
  Vector3 movementDirection;

  bool flyingMode = false;

  GameObject ground;
  GameObject sun;
  Light sunLight;

  void Awake() {
    ground = GameObject.Find("Ground");
    sun = GameObject.Find("Sun");
    sunLight = sun.GetComponent<Light>();

    if (originalPosition.Equals(Vector3.zero)) {
      originalPosition = transform.position;
    } else {
      transform.position = originalPosition;
    }

    camera = GetComponentInChildren<Camera>().transform;
    rigidbody = GetComponent<Rigidbody>();
    collider = GetComponent<Collider>();
  }

  Vector3 SunPosition() {
    var theta = Mathf.PI * sunInclination;
    var phi = Mathf.PI * -0.4f;

    return new Vector3(
      500 * Mathf.Cos(phi),
      400 * Mathf.Sin(phi) * Mathf.Sin(theta),
      500 * Mathf.Sin(phi) * Mathf.Cos(theta)
    );
  }

  void SetPosition(string positionVector) {
    originalPosition = transform.position = JsonUtility.FromJson<Vector3>(positionVector);

    UpdateEnvironment();
  }

  void UpdateEnvironment() {
    DCL.Interface.WebInterface.ReportPosition(transform.position, transform.rotation);

    var originalY = ground.transform.position.y;

    ground.transform.position = new Vector3(
      Mathf.Floor(transform.position.x / ParcelSettings.PARCEL_SIZE) * ParcelSettings.PARCEL_SIZE,
      originalY,
      Mathf.Floor(transform.position.z / ParcelSettings.PARCEL_SIZE) * ParcelSettings.PARCEL_SIZE
    );

    sun.transform.position = SunPosition();

    var sunfade = 1.0f - Mathf.Min(Mathf.Max(1.0f - Mathf.Exp(sun.transform.position.y / 10f), 0.0f), 0.9f);

    sunLight.intensity = sunfade;
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
      UpdateEnvironment();
    }
  }
}
