using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarParticleSystemsHandler : MonoBehaviour
{
    [SerializeField] ParticleSystem footstepParticleSystem;
    [SerializeField] ParticleSystem heartParticleSystem;
    [SerializeField] ParticleSystem moneyParticleSystem;
    [SerializeField] ParticleSystem clapParticleSystem;

    Vector3 moneyInitialRotation, heartInitialRotation, clapInitialRotation;

    private void Start()
    {
        heartInitialRotation = heartParticleSystem.transform.rotation.eulerAngles;
        moneyInitialRotation = moneyParticleSystem.transform.rotation.eulerAngles;
        clapInitialRotation = clapParticleSystem.transform.rotation.eulerAngles;
    }

    public void EmitFootstepParticles(Vector3 position, Vector3 direction, int amount)
    {
        MoveAndEmit(footstepParticleSystem, position, direction, amount);
    }

    public void EmitHeartParticles(Vector3 position, Vector3 direction, int amount)
    {
        MoveAndEmit(heartParticleSystem, position, heartInitialRotation + direction, amount);
    }

    public void EmitMoneyParticles(Vector3 position, Vector3 direction, int amount)
    {
        MoveAndEmit(moneyParticleSystem, position, moneyInitialRotation + direction, amount);
    }

    public void EmitClapParticles(Vector3 position, Vector3 direction, int amount)
    {
        MoveAndEmit(clapParticleSystem, position, clapInitialRotation + direction, amount);
    }

    void MoveAndEmit(ParticleSystem particleSystem, Vector3 position, Vector3 direction, int amount)
    {
        particleSystem.transform.position = position;
        particleSystem.transform.rotation = Quaternion.Euler(direction.x, direction.y, direction.z);
        particleSystem.Emit(amount);
    }
}
