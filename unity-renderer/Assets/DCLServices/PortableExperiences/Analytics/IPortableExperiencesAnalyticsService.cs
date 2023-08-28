using DCL;

namespace DCLServices.PortableExperiences.Analytics
{
    public interface IPortableExperiencesAnalyticsService : IService
    {
        void Spawn(string pexId);
        /// <param name="pexId">Portable experience id/urn</param>
        /// <param name="dontAskAgain">Does the user enabled the toggle to not be asked again about this PEX</param>
        /// <param name="source">scene | smart_wearable</param>
        void Accept(string pexId, bool dontAskAgain, string source);
        /// <param name="pexId">Portable experience id/urn</param>
        /// <param name="dontAskAgain">Does the user enabled the toggle to not be asked again about this PEX</param>
        /// <param name="source">scene | smart_wearable</param>
        void Reject(string pexId, bool dontAskAgain, string source);
    }
}
