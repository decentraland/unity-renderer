using System.Collections;
using System.Collections.Generic;
using DCL.Configuration;
using UnityEngine;

public class EnvironmentController : MonoBehaviour
{
    public GameObject ground;
    public GameObject sun;
    public float sunInclination = -0.31f;

    Light sunLight;
    DCLCharacterController characterController;

    void Awake()
    {
        sunLight = sun.GetComponent<Light>();

        sun.transform.position = CalculateSunPosition();
    }

    void OnEnable()
    {
        DCLCharacterController.OnCharacterMoved += UpdateEnvironmentPosition;
    }

    void OnDisable()
    {
        DCLCharacterController.OnCharacterMoved -= UpdateEnvironmentPosition;
    }

    Vector3 CalculateSunPosition()
    {
        var theta = Mathf.PI * sunInclination;
        var phi = Mathf.PI * -0.4f;

        return new Vector3(
          500 * Mathf.Cos(phi),
          400 * Mathf.Sin(phi) * Mathf.Sin(theta),
          500 * Mathf.Sin(phi) * Mathf.Cos(theta)
        );
    }

    public void UpdateEnvironmentPosition(Vector3 characterPosition)
    {
        var originalY = ground.transform.position.y;

        ground.transform.position = new Vector3(
          Mathf.Floor(characterPosition.x / ParcelSettings.PARCEL_SIZE) * ParcelSettings.PARCEL_SIZE,
          originalY,
          Mathf.Floor(characterPosition.z / ParcelSettings.PARCEL_SIZE) * ParcelSettings.PARCEL_SIZE
        );

        var sunfade = 1.0f - Mathf.Min(Mathf.Max(1.0f - Mathf.Exp(sun.transform.position.y / 10f), 0.0f), 0.9f);
        sunLight.intensity = sunfade;
    }
}
