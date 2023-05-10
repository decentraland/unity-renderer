using DCL.Models;

namespace DCL.Controllers
{
    public interface ISceneBoundsChecker : IService
    {
        float timeBetweenChecks { get; set; }
        bool enabled { get; }
        int entitiesToCheckCount { get; }
        void SetFeedbackStyle(ISceneBoundsFeedbackStyle feedbackStyle);
        ISceneBoundsFeedbackStyle GetFeedbackStyle();
        void Stop();
        void AddEntityToBeChecked(IDCLEntity entity, bool isPersistent = false, bool runPreliminaryEvaluation = false);
        void RemoveEntity(IDCLEntity entity, bool removeIfPersistent = false, bool resetState = false);
        bool WasAddedAsPersistent(IDCLEntity entity);
        void RunEntityEvaluation(IDCLEntity entity, bool onlyOuterBoundsCheck);
    }
}
