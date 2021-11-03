using System.Text.RegularExpressions;

public class ImmutableProfanityFiltering : IChatProfanityFiltering
{
    public string Filter(string message) => message;
}