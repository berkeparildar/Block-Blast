using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private FirestoreWriter _firestoreWriter;
        private static int _playerLevel;
        private LevelData _levelData;
        private int _moveCount;
        private int[] _targets;
        private int[] _targetCounts;
        private bool _levelFinish;

        public void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
            _firestoreReader = new FirestoreReader();
            _firestoreWriter = new FirestoreWriter();
            _playerLevel = PlayerPrefs.GetInt("level", 1);
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

        private async void InitializeLevel()
        {
            _levelFinish = false;
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                var randomLevel = _firestoreWriter.GetRandomLevelData();
                if (randomLevel.Level == 0)
                {
                    return;
                }
                await Task.Delay(1000);
                _levelData = randomLevel;
            }
            else
            { 
                _levelData = await _firestoreReader.GetLevelData(_playerLevel);
                _firestoreWriter.SaveLevelData(_levelData);
            }
            SetLevelData();
        }
        
        private void SetLevelData()
        {
            gridManager.SetGridData(_levelData.GridData);
            UIEvents.Instance.OnTargetsSet.Invoke(_levelData.Targets, _levelData.TargetCounts, _levelData.MoveLimit);
            _moveCount = _levelData.MoveLimit;
            _targets = _levelData.Targets;
            _targetCounts = _levelData.TargetCounts;
            GameEvents.Instance.OnLevelInitialized.Invoke();
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
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                _playerLevel++;
                PlayerPrefs.SetInt("level", _playerLevel);
            }
            UIEvents.Instance.OnLevelWin.Invoke();
            StartCoroutine(ReloadScene());
        }

        private void LevelLost()
        {
            UIEvents.Instance.OnLevelLose.Invoke();
            StartCoroutine(ReloadScene());
        }
        
        private void RunOnMainThread(Action action)
        {
            StartCoroutine(InvokeOnMainThread(action));
        }

        private IEnumerator InvokeOnMainThread(Action action)
        {
            yield return null;
            action?.Invoke();
        }

        private IEnumerator ReloadScene()
        {
            yield return new WaitForSeconds(3);
            gridManager.ResetGrid();
            if (_playerLevel % 2 == 0)
            {
                InterstitialAd ad = adsManager.ShowInterstitialAd();
                if (ad is not null && _playerLevel % 2 == 0)
                {
                    ad.OnAdFullScreenContentClosed += () =>
                    {
                        RunOnMainThread(InitializeLevel);
                    };
                    ad.OnAdFullScreenContentFailed += error =>
                    {
                        RunOnMainThread(InitializeLevel);
                    };
                }
                else
                {
                    InitializeLevel();
                }
            }
            else
            {
                InitializeLevel();
            }
        }
    }
}