namespace UnityGLTF
{
    public interface IDownloadQueueElement
    {
        bool ShouldPrioritizeDownload();
        bool ShouldForceDownload();
        float GetSqrDistance();
    }
}