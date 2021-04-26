using System.Collections;
using UnityEngine;
using UnityEngine.UI;


//Note (Adrian): This class is temporary and will dissapear when the new UI is implemented
public class CircleLoadingAnimator : MonoBehaviour
{
    public float animSpeed = 6f;

    Image fillImage;
    Coroutine coroutine;

    private void OnEnable()
    {
        if (fillImage == null)
            fillImage = GetComponent<Image>();

        coroutine = StartCoroutine(LoadinAnimation());
    }

    private void OnDisable()
    {
        StopCoroutine(coroutine);
    }


    IEnumerator LoadinAnimation()
    {
        fillImage.fillAmount = 0;
        float currentSpeed = animSpeed * Time.deltaTime;
        while(true)
        {
            fillImage.fillAmount += currentSpeed;

            if (fillImage.fillAmount >= 1)
                currentSpeed = -animSpeed * Time.deltaTime;
            else if (fillImage.fillAmount <= 0)
                currentSpeed = animSpeed * Time.deltaTime;

            yield return null;
        }
    }
}
