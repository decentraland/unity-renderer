using System.Collections;
using System.Collections.Generic;
using Login;
using UnityEngine;

public class LoginSceneController : MonoBehaviour
{
    private LoginHUDController _loginHUDController;

    // Start is called before the first frame update
    void Start()
    {
        _loginHUDController = new LoginHUDController();
        _loginHUDController.Initialize();
    }
}
