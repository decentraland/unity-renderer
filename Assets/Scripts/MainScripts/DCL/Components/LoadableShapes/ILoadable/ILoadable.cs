namespace DCL.Components
{
    public interface ILoadable
    {
        System.Action OnSuccess { get; set; }
        System.Action OnFail { get; set; }
        bool InitialVisibility { get; set; }

        void Load(string url, bool useVisualFeedback);
    }
}