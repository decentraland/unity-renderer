using System;
using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Special type of entry to be used as date separator in chat conversations.
/// </summary>
public class DateSeparatorEntry : ChatEntry
{
    [SerializeField] internal TextMeshProUGUI title;
    
    [Header("Preview Mode")]
    [SerializeField] private Image previewBackgroundImage;
    [SerializeField] private Color previewBackgroundColor;
    [SerializeField] private Color previewFontColor;
    
    private DateTime timestamp;
    private ChatEntryModel chatEntryModel;
    private Color originalBackgroundColor;
    private Color originalFontColor;

    public override ChatEntryModel Model => chatEntryModel;
    
    private void Awake()
    {
        originalBackgroundColor = previewBackgroundImage.color;
        originalFontColor = title.color;
    }

    public override void Populate(ChatEntryModel model)
    {
        chatEntryModel = model;
        title.text = GetDateFormat(GetDateTimeFromUnixTimestampMilliseconds(model.timestamp));
    }

    public override void SetFadeout(bool enabled)
    {
        if (!enabled)
        {
            group.alpha = 1;
            fadeEnabled = false;
            return;
        }

        fadeEnabled = true;
    }

    public override void DeactivatePreview()
    {
        if (!gameObject.activeInHierarchy)
        {
            previewBackgroundImage.color = originalBackgroundColor;
            title.color = originalFontColor;
            return;
        }
        
        if (previewInterpolationRoutine != null)
            StopCoroutine(previewInterpolationRoutine);

        if (previewInterpolationAlphaRoutine != null)
            StopCoroutine(previewInterpolationAlphaRoutine);

        group.alpha = 1;
        previewInterpolationRoutine =
            StartCoroutine(InterpolatePreviewColor(originalBackgroundColor, originalFontColor, 0.5f));
    }
    
    public override void FadeOut()
    {
        
        if (!gameObject.activeInHierarchy)
            return;
        
        if (previewInterpolationAlphaRoutine != null)
            StopCoroutine(previewInterpolationAlphaRoutine);

        previewInterpolationAlphaRoutine = StartCoroutine(InterpolateAlpha(0, 0.5f));
    }

    public override void ActivatePreview()
    {
        if (!gameObject.activeInHierarchy)
        {
            ActivatePreviewInstantly();
            return;
        }
        
        if (previewInterpolationRoutine != null)
            StopCoroutine(previewInterpolationRoutine);
        
        previewInterpolationRoutine = StartCoroutine(InterpolatePreviewColor(previewBackgroundColor, previewFontColor, 0.5f));
        
        previewInterpolationAlphaRoutine = StartCoroutine(InterpolateAlpha(1, 0.5f));
    }
    
    public override void ActivatePreviewInstantly()
    {
        if (previewInterpolationRoutine != null)
            StopCoroutine(previewInterpolationRoutine);
        
        previewBackgroundImage.color = previewBackgroundColor;
        title.color = previewFontColor;
    }
    
    public override void DeactivatePreviewInstantly()
    {
        if (previewInterpolationRoutine != null)
            StopCoroutine(previewInterpolationRoutine);
        
        previewBackgroundImage.color = originalBackgroundColor;
        title.color = originalFontColor;
    }

    private string GetDateFormat(DateTime date)
    {
        string result = string.Empty;

        if (date.Year == DateTime.Now.Year &&
            date.Month == DateTime.Now.Month &&
            date.Day == DateTime.Now.Day)
        {
            result = "Today";
        }
        else
        {
            result = date.ToString("D", DateTimeFormatInfo.InvariantInfo);
        }

        return result;
    }
    
    private DateTime GetDateTimeFromUnixTimestampMilliseconds(ulong milliseconds)
    {
        DateTime result = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return result.AddMilliseconds(milliseconds);
    }
    
    private IEnumerator InterpolatePreviewColor(Color backgroundColor, Color fontColor, float duration)
    {
        var t = 0f;
        
        while (t < duration)
        {
            t += Time.deltaTime;

            previewBackgroundImage.color = Color.Lerp(previewBackgroundImage.color, backgroundColor, t / duration);
            title.color = Color.Lerp(title.color, fontColor, t / duration);
            
            yield return null;
        }

        previewBackgroundImage.color = backgroundColor;
        title.color = fontColor;
    }
    
}