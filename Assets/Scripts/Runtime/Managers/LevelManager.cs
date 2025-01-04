using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Runtime.Data.ValueObjects;
using Runtime.Events;
using Runtime.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Runtime.Managers
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private GridManager gridManager;
        [SerializeField] private AdsManager adsManager;
        private FirestoreReader _firestoreReader;
        private int _playerLevel;
        private LevelData _levelData;
        private int _moveCount;
        private int[] _targets;
        private int[] _targetCounts;
        private bool _levelFinish;

        public void Awake()
        {
            _firestoreReader = new FirestoreReader();
            GetPlayerLevel();
            GetLevelData();
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            LevelEvents.Instance.OnBlockBlasted += BlockBlasted;
            LevelEvents.Instance.OnBlast += PlayerBlasted;
            LevelEvents.Instance.IsLevelFinished += () => _levelFinish;
        }

        private async void GetLevelData()
        {
            _levelData = await _firestoreReader.GetLevelData(_playerLevel);
            _playerLevel = _levelData.Level;
            gridManager.SetGridData(_levelData.GridData);
            UIEvents.Instance.OnTargetsSet.Invoke(_levelData.Targets, _levelData.TargetCounts, _levelData.MoveLimit);
            _moveCount = _levelData.MoveLimit;
            _targets = _levelData.Targets;
            _targetCounts = _levelData.TargetCounts;
            LevelEvents.Instance.OnLevelInitialized.Invoke();
        }

        private void GetPlayerLevel()
        {
            _playerLevel = PlayerPrefs.GetInt("level", 1);
            Debug.LogWarning("current level:" + _playerLevel);
        }

        private void BlockBlasted(int colorIndex)
        {
            List<int> targetList = _targets.ToList();
            if (targetList.Contains(colorIndex))
            {
                int blastedIndex = targetList.IndexOf(colorIndex);
                _targetCounts[blastedIndex]--;
                UIEvents.Instance.OnBlockBlasted.Invoke(colorIndex);
            }

            if (TargetsFinished() && !_levelFinish)
            {
                _levelFinish = true;
                LevelWon();
            }
        }

        private void PlayerBlasted()
        {
            if (_moveCount > 0)
            {
                _moveCount--;
                if (_moveCount == 0)
                {
                    if (TargetsFinished())
                    {
                        LevelWon();
                    }
                    else
                    {
                        LevelLost();
                    }
                }

                UIEvents.Instance.OnPlayerMove.Invoke(_moveCount);
            }
        }

        private bool TargetsFinished()
        {
            for (int i = 0; i < _targets.Length; i++)
            {
                if (_targetCounts[i] > 0)
                {
                    return false;
                }
            }

            return true;
        }

        private void LevelWon()
        {
            _playerLevel++;
            PlayerPrefs.SetInt("level", _playerLevel);
            Debug.LogWarning("new level:" + _playerLevel);

            UIEvents.Instance.OnLevelWin.Invoke();
            StartCoroutine(ReloadScene());
        }

        private void LevelLost()
        {
            UIEvents.Instance.OnLevelLose.Invoke();
            StartCoroutine(ReloadScene());
        }

        private IEnumerator ReloadScene()
        {
            yield return new WaitForSeconds(3);
            var ad = adsManager.ShowInterstitialAd();
            if (ad == null)
            {
                Debug.LogWarning("Interstitial ad was not ready, reloading scene...");
                SceneManager.LoadScene(0);
                yield break;
            }
            ad.OnAdFullScreenContentClosed += () => { SceneManager.LoadScene(0); };

            ad.OnAdFullScreenContentFailed += error => { SceneManager.LoadScene(0); };
        }
    }
}