using System.Collections;
using Runtime.Controllers;
using Runtime.Events;
using TMPro;
using UnityEngine;

namespace Runtime.Managers
{
    public class UIManager : MonoBehaviour
    {
        private static readonly int Hide = Animator.StringToHash("Hide");
        private static readonly int Show = Animator.StringToHash("Show");
        [SerializeField] private UITargetController[] targets;
        [SerializeField] private TextMeshProUGUI moveCountText;
        [SerializeField] private Animator loadingAnimator;
        [SerializeField] private GameObject winScreen;
        [SerializeField] private GameObject loseScreen;

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            LevelEvents.Instance.OnLevelInitialized += HideLoadingScreen;
            UIEvents.Instance.OnTargetsSet += SetData;
            UIEvents.Instance.OnPlayerMove += SetMoveCount;
            UIEvents.Instance.OnBlockBlasted += BlockBlasted;
            UIEvents.Instance.OnLevelWin += ShowLevelWinScreen;
            UIEvents.Instance.OnLevelLose += ShowLevelLoseScreen;
        }

        private void ShowLevelWinScreen()
        {
            winScreen.SetActive(true);
            StartCoroutine(ShowLoading());
            return;
            IEnumerator ShowLoading()
            {
                yield return new WaitForSeconds(1.5f);
                ShowLoadingScreen();
                yield return new WaitForSeconds(1);
                foreach (UITargetController tc in targets)
                {
                    tc.gameObject.SetActive(false);
                }
                winScreen.SetActive(false);
            }
        }
        
        private void ShowLevelLoseScreen()
        {
            loseScreen.SetActive(true);
            StartCoroutine(ShowLoading());
            return;
            IEnumerator ShowLoading()
            {
                yield return new WaitForSeconds(1.5f);
                ShowLoadingScreen();
                yield return new WaitForSeconds(1);
                foreach (UITargetController tc in targets)
                {
                    tc.gameObject.SetActive(false);
                }
                loseScreen.SetActive(false);
            }
        }

        private void HideLoadingScreen()
        {
            loadingAnimator.SetTrigger(Hide);
        }

        private void ShowLoadingScreen()
        {
            loadingAnimator.SetTrigger(Show);
        }

        private void SetData(int[] target, int[] targetCount, int moveCount)
        {
            for (int i = 0; i < target.Length; i++)
            {
                int colorIndex = target[i] == -1 ? 6 : target[i];
                targets[colorIndex].gameObject.SetActive(true);
                targets[colorIndex].SetTargetCount(targetCount[i]);
            }
            
            moveCountText.text = moveCount.ToString();        
        }

        private void BlockBlasted(int targetIndex)
        {
            if (targetIndex == -1) targetIndex = 6;
            targets[targetIndex].DecrementTargetCount();
        }

        private void SetMoveCount(int moveCount)
        {
            moveCountText.text = moveCount.ToString();
        }
    }
}