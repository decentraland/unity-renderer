using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AltScrollViewButtonController : MonoBehaviour
{
    // Start is called before the first frame update
    public static int Counter = 0;
    void Start()
    {
        Counter = 0;
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    // Update is called once per frame
    public void OnClick()
    {
        Counter++;
        Debug.Log("Tapped:  " + name);
    }
}
