using TMPro;
using UnityEngine;

namespace Runtime.Controllers
{
    public class UITargetController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI targetCountText;
        [SerializeField] private GameObject checkMark;
        private int _targetCount;

        public void SetTargetCount(int targetCount)
        {
            _targetCount = targetCount;
            targetCountText.text = _targetCount.ToString();
        }

        public void DecrementTargetCount()
        {
            if (_targetCount > 1)
            {
                _targetCount--;
                targetCountText.text = _targetCount.ToString();
            }
            else
            {
                targetCountText.gameObject.SetActive(false);
                checkMark.SetActive(true);
            }
        }
    }
}