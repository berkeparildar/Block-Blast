using System;
using Runtime.Events;
using UnityEngine;

namespace Runtime.Managers
{
    public class AudioManager: MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip blockBlastClip;
        
        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            LevelEvents.Instance.OnBlast += PlayBlockBlast;
        }
        
        private void PlayBlockBlast()
        {
            if (audioSource && blockBlastClip)
            {
                audioSource.PlayOneShot(blockBlastClip);
            }
        }
    }
}