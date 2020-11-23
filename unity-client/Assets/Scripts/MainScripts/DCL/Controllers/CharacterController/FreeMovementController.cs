using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeMovementController : MonoBehaviour
{

    [Header("Movement")]
    public float groundCheckExtraDistance = 0.25f;
    public float movementSpeed = 11f;
    public float runningSpeedMultiplier = 0.36f;

    [Header("InputActions")]
    public InputAction_Hold sprintAction;

    [System.NonSerialized] public CharacterController characterController;



    bool isSprinting = false;
    bool isActive;

    float deltaTime = 0.032f;

    Vector3 velocity = Vector3.zero;

    private Vector3NullableVariable characterForward => CommonScriptableObjects.characterForward;

    private Vector3Variable cameraForward => CommonScriptableObjects.cameraForward;
    private Vector3Variable cameraRight => CommonScriptableObjects.cameraRight;

    [SerializeField] private InputAction_Measurable characterYAxis;
    [SerializeField] private InputAction_Measurable characterXAxis;


    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    public bool IsActive()
    {
        return isActive;
    }

    public void SetActive(bool _isActive)
    {
        isActive = _isActive;
    }

    public Vector3 CalculateMovement()
    {
        return FreeMovement();
    }

    Vector3 FreeMovement()
    {
        velocity.x = 0f;
        velocity.z = 0f;
        velocity.y = 0;

        if (characterController.enabled)
        {
            var speed = movementSpeed * (isSprinting ? runningSpeedMultiplier : 1f);

            transform.forward = characterForward.Get().Value;

            var xzPlaneForward = Vector3.Scale(cameraForward.Get(), new Vector3(1, 0, 1));
            var xzPlaneRight = Vector3.Scale(cameraRight.Get(), new Vector3(1, 0, 1));

            Vector3 forwardTarget = Vector3.zero;

            if (characterYAxis.GetValue() > 0)
                forwardTarget += xzPlaneForward;
            if (characterYAxis.GetValue() < 0)
                forwardTarget -= xzPlaneForward;

            if (characterXAxis.GetValue() > 0)
                forwardTarget += xzPlaneRight;
            if (characterXAxis.GetValue() < 0)
                forwardTarget -= xzPlaneRight;


            if (Input.GetKey(KeyCode.Space)) forwardTarget += Vector3.up;
            else if (Input.GetKey(KeyCode.X)) forwardTarget += Vector3.down;

            forwardTarget.Normalize();

       
            velocity += forwardTarget * speed;
            CommonScriptableObjects.playerUnityEulerAngles.Set(transform.eulerAngles);

            characterController.Move(velocity * deltaTime);
        }

        return velocity;
    }
}