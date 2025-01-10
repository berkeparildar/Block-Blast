using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GoogleMobileAds.Api;
using Runtime.Data.ValueObjects;
using Runtime.Events;
using Runtime.Extensions;
using UnityEngine;

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
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            _firestoreReader = new FirestoreReader();
            InitializeLevel();
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            GameEvents.Instance.OnBlockBlasted += BlockBlasted;
            GameEvents.Instance.OnBlast += PlayerBlasted;
            GameEvents.Instance.IsLevelFinished += () => _levelFinish;
        }

        private void InitializeLevel()
        {
            _levelFinish = false;
            GetPlayerLevel();
            GetLevelData();
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
            GameEvents.Instance.OnLevelInitialized.Invoke();
        }

        private void GetPlayerLevel()
        {
            _playerLevel = PlayerPrefs.GetInt("level", 1);
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
            if (_moveCount <= 0) return;
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
            gridManager.ResetGrid();
            InterstitialAd ad = adsManager.ShowInterstitialAd();
            ad.OnAdFullScreenContentClosed += InitializeLevel;
            ad.OnAdFullScreenContentFailed += error =>
            {
                InitializeLevel();
            };
        }
    }
}