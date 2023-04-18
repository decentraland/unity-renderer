using System;

[Serializable]
public record InfoCardComponentModel
{
    public string name;
    public string description;
    public string category;
    public string[] hideList;
    public string[] removeList;
}
