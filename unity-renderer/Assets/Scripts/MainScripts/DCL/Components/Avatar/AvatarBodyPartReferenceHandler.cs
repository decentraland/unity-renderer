using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarBodyPartReferenceHandler : MonoBehaviour
{
    [HideInInspector]
    public Transform footL, footR, handL, handR;

    private void Awake() {
        // Find body parts
        Transform[] children = GetComponentsInChildren<Transform>();
        for (int i = 0; i < children.Length; i++) {
            if (children[i].name == "Avatar_LeftToeBase")
                footL = children[i];
            if (children[i].name == "Avatar_RightToeBase")
                footR = children[i];
            if (children[i].name == "Avatar_LeftHand")
                handL = children[i];
            if (children[i].name == "Avatar_RightHand")
                handR = children[i];
        }
    }
}
