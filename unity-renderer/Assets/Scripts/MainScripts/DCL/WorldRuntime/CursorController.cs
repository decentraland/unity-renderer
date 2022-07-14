using System;
using System.Collections;
using DCL;
using UnityEngine;
using UnityEngine.UI;

public class CursorController : MonoBehaviour
{
    private const float ALPHA_INTERPOLATION_DURATION = 0.1f;

    public static CursorController i { get; private set; }
    public Image cursorImage;
    public Sprite normalCursor;
    public Sprite hoverCursor;
    public CanvasGroup canvasGroup;

    private Coroutine alphaRoutine;
    private bool isAllUIHidden;

    void Awake()
    {
        i = this;
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

    private void OnChangeType(DataStore_Cursor.CursorType current, DataStore_Cursor.CursorType previous)
    {
        switch (current)
        {
            case DataStore_Cursor.CursorType.NORMAL:
                SetNormalCursor();
                break;
            case DataStore_Cursor.CursorType.HOVER:
                SetHoverCursor();
                break;
        }
    }

    private void AllUIVisible_OnChange(bool current, bool previous)
    {
        isAllUIHidden = current;

        DataStore_Cursor data = DataStore.i.Get<DataStore_Cursor>();
        ChangeCursorVisible(data.cursorVisible.Get(), false);
    }

    private void ChangeCursorVisible(bool current, bool previous)
    {
        if (current && !isAllUIHidden)
            Show();
        else
            Hide();
    }

    public void Show()
    {
        if (cursorImage == null) return;
        if (cursorImage.gameObject.activeSelf) return;

        cursorImage.gameObject.SetActive(true);

        if (gameObject.activeSelf)
        {
            if (alphaRoutine != null) StopCoroutine(alphaRoutine);
            alphaRoutine = StartCoroutine(SetAlpha(0f, 1f, ALPHA_INTERPOLATION_DURATION));
        }
    }

    public void Hide()
    {
        if (cursorImage == null) return;
        if (!cursorImage.gameObject.activeSelf) return;

        if (gameObject.activeSelf)
        {
            if (alphaRoutine != null) StopCoroutine(alphaRoutine);
            alphaRoutine = StartCoroutine(SetAlpha(1f, 0f, ALPHA_INTERPOLATION_DURATION,
                () => cursorImage.gameObject.SetActive(false)));
        }
        else
            cursorImage.gameObject.SetActive(false);
    }

    public void SetNormalCursor()
    {
        cursorImage.sprite = normalCursor;
        cursorImage.SetNativeSize();
    }

    public void SetHoverCursor()
    {
        cursorImage.sprite = hoverCursor;
        cursorImage.SetNativeSize();
    }

    private IEnumerator SetAlpha(float from, float to, float duration, Action callback = null)
    {
        var time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, time / duration);
            yield return null;
        }

        canvasGroup.alpha = to;
        callback?.Invoke();
    }
}