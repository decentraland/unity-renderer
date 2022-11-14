using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class inBoxCheck : MonoBehaviour
{
    // variables 

    [SerializeField] private Vector2 playerPosition;
    [Space]
    [SerializeField] private Vector2 boxCornerA = new Vector2(128f, 128f);
    [SerializeField] private Vector2 boxCornerB = new Vector2(128f, -128f);
    [SerializeField] private Vector2 boxCornerC = new Vector2(-128f, -128f);
    [SerializeField] private Vector2 boxCornerD = new Vector2(-128f, 128f);
    [SerializeField] private Vector2 boxCenter = new Vector2(0f, 0f);
    
    [Space]
    [SerializeField] private float outResult = 0f;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckPlayerPosition(); 
    }
    
    // check if player is in box
    void CheckPlayerPosition()
    {
        if (playerPosition.x > boxCornerA.x && playerPosition.y > boxCornerA.y)
        {
            Debug.Log("Player is not in box out of corner A ");
            outResult = 1f;
        }
        else if (playerPosition.x > boxCornerB.x && playerPosition.y < boxCornerB.y)
        {
            Debug.Log("Player is not in box out of corner B ");
            outResult = 1f;
        }
        else if (playerPosition.x < boxCornerC.x && playerPosition.y < boxCornerC.y)
        {
            Debug.Log("Player is not in box out of corner C ");
            outResult = 1f;
        }
        else if (playerPosition.x < boxCornerD.x && playerPosition.y > boxCornerD.y)
        {
            Debug.Log("Player is not in the box out of corner D ");
            outResult = 1f;
        }
        else
        {
            Debug.Log("Player is IN box");
            outResult = 0f;
        }
    }
}
