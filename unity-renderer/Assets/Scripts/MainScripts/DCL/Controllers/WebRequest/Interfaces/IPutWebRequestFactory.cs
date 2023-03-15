namespace DCL
{
    public interface IPutWebRequestFactory : IWebRequestFactory
    {
        void SetBody(string postData);

        //use this to perform PATCH requests
        void SetPatchRequest(bool isPatchRequest);
    }
}
