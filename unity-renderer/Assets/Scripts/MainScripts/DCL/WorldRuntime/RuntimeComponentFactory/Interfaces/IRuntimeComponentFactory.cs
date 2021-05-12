using DCL.Components;

namespace DCL
{
    public interface IRuntimeComponentFactory
    {
        IComponent CreateComponent(int classId);
        void Initialize();
    }
}