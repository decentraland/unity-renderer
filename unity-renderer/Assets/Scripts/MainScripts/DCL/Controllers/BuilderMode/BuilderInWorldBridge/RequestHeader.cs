using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RequestHeader
{
    public string endpoint;
    public Dictionary<string, string> headers;
}