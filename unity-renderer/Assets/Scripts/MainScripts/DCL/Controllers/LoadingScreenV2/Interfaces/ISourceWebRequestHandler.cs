using Cysharp.Threading.Tasks;

public interface ISourceWebRequestHandler
{
    UniTask<string> Get(string url);
}
