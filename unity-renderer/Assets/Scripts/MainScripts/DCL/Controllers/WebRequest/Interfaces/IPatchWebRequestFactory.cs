namespace DCL
{
    public interface IPatchWebRequestFactory : IWebRequestFactory
    {
        void SetBody(string postData);
    }
}
