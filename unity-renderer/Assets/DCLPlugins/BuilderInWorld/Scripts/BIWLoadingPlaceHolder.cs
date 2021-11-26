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
    public void Dispose()
    {
        if (checkCoroutine != null)
            CoroutineStarter.Stop(checkCoroutine);

        Destroy(gameObject);
    }

    public void DestroyAfterAnimation()
    {
        if (placeHolderAnimator == null)
            return;
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
            bool isExitingAnimationActive = false;
            while (!isExitingAnimationActive)
            {
                if (placeHolderAnimator != null)
                    isExitingAnimationActive = placeHolderAnimator.GetCurrentAnimatorStateInfo(0).IsName(EXIT_ANIMATION_NAME);
                else
                    isExitingAnimationActive  = true;
                yield return null;
            }
            if (this != null)
                Destroy(gameObject);
        }
    }
}