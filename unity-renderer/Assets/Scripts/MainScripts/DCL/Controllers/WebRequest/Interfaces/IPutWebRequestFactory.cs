namespace DCL
{
    public interface IPutWebRequestFactory : IWebRequestFactory
    {
        void SetBody(string postData);
    }
}
