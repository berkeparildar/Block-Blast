using System;
using GoogleMobileAds.Api;
using UnityEngine;

namespace Runtime.Managers
{
    public class AdsManager : MonoBehaviour
    {
#if UNITY_ANDROID
        private string _adUnitId = "ca-app-pub-3082331033671775/3977567206";
#elif UNITY_IPHONE
  private string _adUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
  private string _adUnitId = "unused";
#endif
        private InterstitialAd _interstitialAd;

        public void Start()
        {
            MobileAds.Initialize((InitializationStatus initStatus) =>
            {
                Debug.Log("Initialization status: " + initStatus);
                LoadInterstitialAd();
            });
        }

        private void LoadInterstitialAd()
        {
            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }

            AdRequest adRequest = new AdRequest();
            InterstitialAd.Load(_adUnitId, adRequest,
                (InterstitialAd ad, LoadAdError error) =>
                {
                    if (error != null || ad == null)
                    {
                        return;
                    }
                    Debug.Log("Ad was loaded");
                    _interstitialAd = ad;
                    RegisterEventHandlers(_interstitialAd);
                });
        }

        public InterstitialAd ShowInterstitialAd()
        {
            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                _interstitialAd.Show();
                return _interstitialAd;
            }
            else
            {
                return null;
            }
        }

        private void RegisterEventHandlers(InterstitialAd interstitialAd)
        {
            // Raised when the ad is estimated to have earned money.
            interstitialAd.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            interstitialAd.OnAdImpressionRecorded += () => { Debug.Log("Interstitial ad recorded an impression."); };
            interstitialAd.OnAdClicked += () => { Debug.Log("Interstitial ad was clicked."); };
            interstitialAd.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Interstitial ad full screen content opened.");
            };
            interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Interstitial ad full screen content closed.");
                LoadInterstitialAd();
            };
            interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Interstitial ad failed to open full screen content " +
                               "with error : " + error);
                LoadInterstitialAd();
            };
        }
    }
}