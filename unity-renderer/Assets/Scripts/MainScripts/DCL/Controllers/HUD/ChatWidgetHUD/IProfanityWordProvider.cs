using System.Collections.Generic;

public interface IProfanityWordProvider
{
    IEnumerable<string> GetAll();
}