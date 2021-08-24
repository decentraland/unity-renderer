using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarParticleSystemsHandler : MonoBehaviour
{
    [SerializeField] ParticleSystem footstepParticleSystem;
    [SerializeField] ParticleSystem heartParticleSystem;
    [SerializeField] ParticleSystem moneyParticleSystem;

    public void EmitFootstepParticles(Vector3 position, int amount) {
        footstepParticleSystem.transform.position = position;
        footstepParticleSystem.Emit(amount);
    }

    public void EmitHeartParticles(Vector3 position, int amount) {
        heartParticleSystem.transform.position = position;
        heartParticleSystem.Emit(amount);
    }

    public void EmitMoneyParticles(Vector3 position, int amount) {
        moneyParticleSystem.transform.position = position;
        moneyParticleSystem.Emit(amount);
    }
}
