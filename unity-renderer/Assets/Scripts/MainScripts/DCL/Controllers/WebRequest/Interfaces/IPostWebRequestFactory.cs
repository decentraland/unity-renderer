namespace DCL
{
    public interface IPostWebRequestFactory : IWebRequestFactory
    {
        void SetBody(string postData);
    }
}
