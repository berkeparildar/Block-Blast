using System.Collections;
using UnityEngine;

namespace Runtime.Controllers
{
    public class ColoredBlockMovementController : MonoBehaviour
    {
        
        private const float BOUNCE_OFFSET = 0.05f;
        private const float BOUNCE_UP_DURATION = 0.1f;
        private const float BOUNCE_DOWN_DURATION = 0.05f;
        private const float FALL_SPEED = 0.2f;
        
        private float m_BaseSpeedModifier;
        private Vector2 m_TargetPos;
        private bool m_MovementInProgress;
        
        public IEnumerator MoveToTargetPosition(Vector2Int target)
        {
            m_TargetPos = target;
            yield return StartCoroutine(FallCoroutine());
        }

        private IEnumerator FallCoroutine()
        {
            if (m_MovementInProgress) yield return null;
            float timeElapsed = 0f;
            m_MovementInProgress = true;

            while ((Vector2)transform.position != m_TargetPos)
            {
                timeElapsed += Time.deltaTime;
                m_BaseSpeedModifier = GetSineValue(timeElapsed, 5);
                float step = FALL_SPEED * m_BaseSpeedModifier;
                transform.position = Vector2.MoveTowards(transform.position, m_TargetPos, step);
                yield return null;
            }

            transform.position = m_TargetPos;
            StartCoroutine(ApplyBounceEffect());
            m_BaseSpeedModifier = 0;
            m_MovementInProgress = false;
        }


        private IEnumerator ApplyBounceEffect()
        {
            Vector3 originalPosition = transform.position;
            Vector3 targetUpPosition = new(originalPosition.x, originalPosition.y + BOUNCE_OFFSET);

            yield return StartCoroutine(
                MoveTowardsTarget(originalPosition, targetUpPosition, BOUNCE_UP_DURATION, false));

            transform.position = targetUpPosition;
            
            yield return StartCoroutine(
                MoveTowardsTarget(targetUpPosition, originalPosition, BOUNCE_DOWN_DURATION, true));
        }

        private IEnumerator MoveTowardsTarget(Vector3 current, Vector3 target, float duration, bool easeIn)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float easeValue = easeIn ? Mathf.Pow(t, 3) : 1 - Mathf.Pow(1 - t, 3);
                transform.position = Vector3.Lerp(current, target, easeValue);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = target;
        }

        private static float GetSineValue(float time, float maxTime)
        {
            float t = Mathf.Clamp01(time / maxTime);
            return Mathf.Sin(t * Mathf.PI) * 2f;
        }
    }
}