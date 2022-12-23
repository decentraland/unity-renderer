using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceBugHack : MonoBehaviour
{
    [SerializeField]
    private GameObject[] m_rootObjects = null;

    // Update is called once per frame
    void Update()
    {
        ReactToKeyPress ();
    }

    private void ReactToKeyPress ()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (m_rootObjects == null || m_rootObjects.Length == 0)
            {   
                Debug.Log ("FD:: O key pressed - Fetching root objects");
                m_rootObjects = CollectRootGameObjects();
            }
            else
            {
                Debug.Log ("FD:: O key pressed - hacking root objects");
                HackAllGameObjects (m_rootObjects);
            }
            
        }
    }


    private GameObject[] CollectRootGameObjects()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
    }

    private void HackAllGameObjects (GameObject[] gos)
    {
        var arrayLng = m_rootObjects.Length;
        for (int i = 1; i < arrayLng; i++)
        {
            // yield return new WaitForEndOfFrame();
            // yield return HackResetGameObjectActiveState(m_rootObjects[i]);
            StartCoroutine (HackResetGameObjectActiveState(m_rootObjects[i]));
        } 
    }

    IEnumerator HackResetGameObjectActiveState (GameObject go)
    {
        var wait = new WaitForEndOfFrame();
        bool originalState = go.activeInHierarchy;
        go.SetActive (true);
        yield return wait;
        go.SetActive (false);
        yield return wait;
        go.SetActive (true);
        yield return wait;
        go.SetActive (originalState);
    }
}
