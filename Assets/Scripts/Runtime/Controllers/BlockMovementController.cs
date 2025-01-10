using System.Collections;
using UnityEngine;

namespace Runtime.Controllers
{
    public class BlockMovementController : MonoBehaviour
    {
        private const float BounceOffset = 0.05f;
        private const float BounceUpDuration = 0.1f;
        private const float BounceDownDuration = 0.05f;
        private const float FallSpeed = 0.2f;
        private float _baseSpeedModifier;
        private Vector2 _targetPos;
        private bool _movementInProgress;
        
        public IEnumerator FallCoroutine(Vector2 targetPos)
        {
            _targetPos = targetPos;
            if (_movementInProgress) yield return null;
            float timeElapsed = 0f;
            _movementInProgress = true;

            while ((Vector2)transform.position != _targetPos)
            {
                timeElapsed += Time.deltaTime;
                _baseSpeedModifier = GetSineValue(timeElapsed, 1);
                float step = FallSpeed * _baseSpeedModifier;
                transform.position = Vector2.MoveTowards(transform.position, _targetPos, step);
                yield return null;
            }

            transform.position = _targetPos;
            StartCoroutine(ApplyBounceEffect());
            _baseSpeedModifier = 0;
            _movementInProgress = false;
        }
        
        private IEnumerator ApplyBounceEffect()
        {
            Vector3 originalPosition = transform.position;
            Vector3 targetUpPosition = new(originalPosition.x, originalPosition.y + BounceOffset);

            yield return StartCoroutine(
                MoveTowardsTarget(originalPosition, targetUpPosition, BounceUpDuration, false));

            transform.position = targetUpPosition;
            
            yield return StartCoroutine(
                MoveTowardsTarget(targetUpPosition, originalPosition, BounceDownDuration, true));
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

        public Vector2 GetTargetPos()
        {
            return _targetPos;
        }
    }
}