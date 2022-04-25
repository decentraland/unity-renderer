using Cysharp.Threading.Tasks;

public interface IProfanityFilter
{
    public UniTask<string> Filter(string message);
}