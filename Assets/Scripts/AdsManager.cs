using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdsManager : MonoBehaviour, IUnityAdsListener  {

    public static AdsManager sharedInstance;
    public string idIos = "3625804";
    public string idAndroid = "3625805";
    public string extendVideoPlacement = "extendVideo";

    void Start() {
        if (sharedInstance == null) {
            sharedInstance = this;
            InitializeAds();
            DontDestroyOnLoad(this);
        }
    }

    private void InitializeAds() {
        Advertisement.AddListener(this);
        // ----------- ONLY NECESSARY FOR ASSET PACKAGE INTEGRATION --------------//
#if UNITY_IOS
        string gameID = idIos;
#elif UNITY_ANDROID
        string gameID = idAndroid;
#endif
        // ----------- ONLY NECESSARY FOR ASSET PACKAGE INTEGRATION --------------//
#if UNITY_EDITOR
        Advertisement.Initialize(gameID, true);
#else
        Advertisement.Initialize(gameID, false);
#endif
    }

    /*
    public IEnumerator ShowAdsWhenReady() {
        while(!Advertisement.isInitialized || !Advertisement.IsReady()) {
            yield return new WaitForSeconds(0.5f);
        }

        Advertisement.Show("video"); 
        //ShowAds();
        //Advertisement.Show();
        //Advertisement.Show(string placementID, ShowOptions showOptions);
        //el placementID es para especificar en el dashboard una propaganda para algun caso
        //showOptions es para poner opciones tipo boton de cerrar y eso

        StopCoroutine(ShowAdsWhenReady());
    }
    */

    public void ShowAds() {
        if (Advertisement.IsReady() && !Advertisement.isShowing) {
            Advertisement.Show("extendVideo");
        }
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult) {
        if (showResult == ShowResult.Finished) {
            // Reward the user for watching the ad to completion.
            GameManager.sharedInstance.ExtendGame();
        } else if (showResult == ShowResult.Skipped) {
            // Do not reward the user for skipping the ad.
        } else if (showResult == ShowResult.Failed) {
            Debug.LogWarning("The ad did not finish due to an error.");
        }
    }

    public void OnUnityAdsReady(string placementId) {
    }
    public void OnUnityAdsDidError(string message) {

    }
    public void OnUnityAdsDidStart(string placementId) {
    }
}
