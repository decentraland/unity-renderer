using DCL.Builder;

public interface IBIWEditor
{
    void Initialize(IContext context);
    void Dispose();
    void OnGUI();
    void Update();
    void LateUpdate();
}