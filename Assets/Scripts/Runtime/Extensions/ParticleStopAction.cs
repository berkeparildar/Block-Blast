using Runtime.Managers;
using UnityEngine;

namespace Runtime.Extensions
{
    public class ParticleStopAction : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particle;
        private void OnParticleSystemStopped()
        {
            ParticleManager.EnqueueParticle(particle);
            particle.gameObject.SetActive(false);
        }
    }
}