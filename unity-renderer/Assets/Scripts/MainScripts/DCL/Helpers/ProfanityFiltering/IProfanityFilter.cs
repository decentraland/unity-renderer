using Cysharp.Threading.Tasks;

public interface IProfanityFilter
{
    UniTask<string> Filter(string message);
}