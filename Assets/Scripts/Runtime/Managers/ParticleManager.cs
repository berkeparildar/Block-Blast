using System.Collections.Generic;
using Runtime.Blocks;
using Runtime.Events;
using Runtime.Extensions;
using UnityEngine;

namespace Runtime.Managers
{
    public class ParticleManager: MonoBehaviour
    {
        [SerializeField] private ParticleSystem particlePrefab;

        private static Queue<ParticleSystem> _poolableObjectList;
        private int _poolAmount;

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            GridEvents.Instance.OnGridSizeSet += Setup;
        }

        private void Setup(int row, int column)
        {
            _poolAmount = row * column;
            _poolableObjectList = new Queue<ParticleSystem>();

            for (int i = 0; i < _poolAmount; i++)
            {
                ParticleSystem go = Instantiate(particlePrefab, transform, true);
                go.gameObject.SetActive(false);
                _poolableObjectList.Enqueue(go);
            }
        }

        public static void EnqueueParticle(ParticleSystem particle)
        {
            _poolableObjectList.Enqueue(particle);
        }

        public static ParticleSystem DequeueParticle(int colorIndex, Vector2 pos)
        {
            ParticleSystem particle = _poolableObjectList.Dequeue();
            if (particle.gameObject.activeSelf) DequeueParticle(colorIndex, pos);
            ParticleSystem.MainModule main = particle.main;
            main.startColor= GameValues.GetColor(colorIndex);
            particle.transform.position = pos;
            particle.gameObject.SetActive(true);
            return particle;
        }
    }
}