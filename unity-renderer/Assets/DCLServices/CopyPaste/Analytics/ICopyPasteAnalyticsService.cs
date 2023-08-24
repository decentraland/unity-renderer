using DCL;

namespace DCLServices.CopyPaste.Analytics
{
    public interface ICopyPasteAnalyticsService : IService
    {
        /// <param name="userId">User address</param>
        /// <param name="element">message | name | player_data | location | channel_name</param>
        void Copy(string userId, string element);
    }
}
