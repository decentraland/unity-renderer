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

    private void Awake()
    {
        DataStore_Cursor data = DataStore.i.Get<DataStore_Cursor>();
        data.cursorVisible.OnChange += ChangeCursorVisible;
        data.cursorType.OnChange += OnChangeType;
        CommonScriptableObjects.allUIHidden.OnChange += AllUIVisible_OnChange;
    }

    private void OnDestroy()
    {
        DataStore_Cursor data = DataStore.i.Get<DataStore_Cursor>();
        data.cursorVisible.OnChange -= ChangeCursorVisible;
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
        ChangeCursorVisible(data.cursorVisible.Get());
    }

    private void ChangeCursorVisible(bool current, bool _ = false)
    {
        if (current && !isAllUIHidden)
            animator.Show();
        else
            animator.Hide();
    }
}
