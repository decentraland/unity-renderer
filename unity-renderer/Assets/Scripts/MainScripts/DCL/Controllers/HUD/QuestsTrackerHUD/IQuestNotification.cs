using System.Collections;

namespace DCL.Huds.QuestsTracker
{
    public interface IQuestNotification
    {
        void Show();
        void Dispose();
        IEnumerator Waiter();
    }
}