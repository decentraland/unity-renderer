using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BIWLoadingPlaceHolder : MonoBehaviour
{
    [SerializeField] private Animator placeHolderAnimator;
    [SerializeField] private List<ParticleSystem> placeHolderParticleSystems;

    private static readonly int disspose = Animator.StringToHash(EXIT_TRIGGER_NAME);
    private const string EXIT_TRIGGER_NAME = "Exit";
    private const string EXIT_ANIMATION_NAME = "Exit";

    private Coroutine checkCoroutine;
    public void Disspose()
    {
        if (checkCoroutine != null)
            CoroutineStarter.Stop(checkCoroutine);

        Destroy(gameObject);
    }

    public void DestroyAfterAnimation()
    {
        placeHolderAnimator.SetTrigger(disspose);

        foreach (ParticleSystem placeHolderParticleSystem in placeHolderParticleSystems)
        {
            placeHolderParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        checkCoroutine = CoroutineStarter.Start(CheckIfAnimationHasFinish());
    }

    IEnumerator CheckIfAnimationHasFinish()
    {
        yield return null;
        if (placeHolderAnimator != null)
        {
            while (!placeHolderAnimator.GetCurrentAnimatorStateInfo(0).IsName(EXIT_ANIMATION_NAME))
            {
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}