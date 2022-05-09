using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarReveal : MonoBehaviour
{
    public bool avatarLoaded;
    public GameObject revealer;
    public Transform finalPosition;
    public ParticleSystem particles;
    public ParticleSystem revealParticles;

    public float revealSpeed;

    private void Update()
    {
        if(avatarLoaded)
        {
            if(revealer.transform.position.y < finalPosition.position.y)
            {
                RevealAvatar();
            }
            else if(particles.isPlaying)
            {
                particles.Stop();
                revealParticles.Stop();
            }
        }
    }

    void RevealAvatar()
    {
        revealer.transform.position += revealer.transform.up * revealSpeed * Time.deltaTime;
        
        if(!particles.isPlaying)
        {
            particles.Play();
            revealParticles.Play();
        }
    }
}
