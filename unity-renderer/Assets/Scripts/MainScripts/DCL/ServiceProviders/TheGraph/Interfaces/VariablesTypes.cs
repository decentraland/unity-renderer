using System;

[Serializable]
public class QueryVariablesBase { }

[Serializable]
public class AddressVariable : QueryVariablesBase
{
    public string address;
}

[Serializable]
public class AddressAndUrnVariable : QueryVariablesBase
{
    public string address;
    public string urn;
}
