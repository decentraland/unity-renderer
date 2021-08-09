using System;

[Serializable]
public class QueryVariablesBase { }

[Serializable]
public class AddressVariable : QueryVariablesBase
{
    public string address;
}