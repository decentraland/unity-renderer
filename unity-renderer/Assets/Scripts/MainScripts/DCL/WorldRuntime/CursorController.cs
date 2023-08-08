using DCL;
using UnityEngine;
using UnityEngine.UI;
using static DCL.DataStore_Cursor;

public class CursorController : MonoBehaviour
{
    public Image cursorImage;
    public Sprite normalCursor;
    public Sprite hoverCursor;

    private Coroutine alphaRoutine;
    private bool isAllUIHidden;

    [SerializeField] private ShowHideAnimator animator;
    private BaseVariable<bool> visible;
    private BaseVariable<bool> visibleByCamera;

    private void Awake()
    {
        DataStore_Cursor data = DataStore.i.Get<DataStore_Cursor>();
        visible = data.cursorVisible;
        visible.OnChange += ChangeVisible;
        visibleByCamera = data.cursorVisibleByCamera;
        visibleByCamera.OnChange += ChangeVisible;
        data.cursorType.OnChange += OnChangeType;
        CommonScriptableObjects.allUIHidden.OnChange += AllUIVisible_OnChange;
    }

    private void OnDestroy()
    {
        DataStore_Cursor data = DataStore.i.Get<DataStore_Cursor>();
        visible.OnChange -= ChangeVisible;
        visibleByCamera.OnChange -= ChangeVisible;
        data.cursorType.OnChange -= OnChangeType;

        CommonScriptableObjects.allUIHidden.OnChange -= AllUIVisible_OnChange;
    }

    private void OnChangeType(CursorType current, CursorType _) =>
        SetCursor(current == CursorType.HOVER ? hoverCursor : normalCursor);

    public void SetCursor(Sprite sprite)
    {
        cursorImage.sprite = sprite;
        cursorImage.SetNativeSize();
    }

    private void AllUIVisible_OnChange(bool current, bool previous)
    {
        isAllUIHidden = current;

        DataStore_Cursor data = DataStore.i.Get<DataStore_Cursor>();
        ChangeVisible(IsVisible());
    }

    private bool IsVisible() =>
        visible.Get() && visibleByCamera.Get();

    private void ChangeVisible(bool current, bool _ = false)
    {
        if (IsVisible() && !isAllUIHidden)
            animator.Show();
        else
            animator.Hide();
    }
}
