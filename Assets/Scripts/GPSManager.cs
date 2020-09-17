using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class GPSManager : MonoBehaviour
{
    public static GPSManager sharedInstance;
    //private Text signInButtonText;
    private Text authStatus;
    private GameObject achButton;
    private GameObject ldrButton;
    private RawImage signInImage;
    private bool firstSignIn = true;
    public Texture2D googleSignInImage;
    private GameObject GPSTuto;

    private void Awake() {
        sharedInstance = this;
    }

    void Start()
    {
        //signInButtonText = GameObject.Find("SignInButton").GetComponentInChildren<Text>();
        authStatus = GameObject.Find("AuthStatus").GetComponentInChildren<Text>();
        achButton = GameObject.Find("AchButton");
        ldrButton = GameObject.Find("LdrButton");
        signInImage = GameObject.Find("SignInButton").GetComponentInChildren<RawImage>();
        GPSTuto = GameObject.Find("GPSTuto");
        GPSTuto.SetActive(false);


        //Create client configuration
        PlayGamesClientConfiguration config = new
            PlayGamesClientConfiguration.Builder()
            .Build();

        //Enable debugging output
        PlayGamesPlatform.DebugLogEnabled = true;

        //Initialize and activate the platform
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.Activate();
        
        // Try silent sign-in (second parameter is isSilent)
        PlayGamesPlatform.Instance.Authenticate(SignInCallback, true);
    }

    private void Update() {
        achButton.SetActive(Social.localUser.authenticated);
        ldrButton.SetActive(Social.localUser.authenticated);
        //if (PlayGamesPlatform.Instance.localUser.authenticated) {
        //    signInImage.texture = Social.localUser.image;
        //}
    }

    public void SignIn() {
        if (!PlayGamesPlatform.Instance.localUser.authenticated) {
            // Sign in with Play Game Services, showing the consent dialog
            // by setting the second parameter to isSilent=false.
            PlayGamesPlatform.Instance.Authenticate(SignInCallback, false);
        } else {
            // Sign out of play games
            PlayGamesPlatform.Instance.SignOut();
            //Al desloguearse mostramos el tutorial si nunca se mostro
            CheckTutorial();

            // Reset UI
            authStatus.text = "Iniciar sesion";
            //signInImage.texture = googleSignInImage;
        }
    }

    //This will be called when the authentication is completed.
    //The parameter indicates if the sign-in was successful or not.
    public void SignInCallback(bool success) {
        if (success) {
            Debug.Log("(Lollygagger) Signed in!");

            // Change sign-in button text
            //signInButtonText.text = "Sign out";

            // Show the user's name and image
            authStatus.text = "Registrado como " + Social.localUser.userName;
            //signInImage.texture = Social.localUser.image;
        } else {
            //Si al iniciar el juego no se logueo automaticamente mostramos el tutorial
            CheckTutorial();

            Debug.Log("(Lollygagger) Sign-in failed...");

            // Show failure message
            //signInButtonText.text = "Sign in";
            if (firstSignIn) {
                authStatus.text = "Iniciar sesion";
            } else {
                authStatus.text = "Error al iniciar sesion";
            }
            //signInImage.texture = googleSignInImage;
        }
        firstSignIn = false;
    }

    public void ShowLeaderboards() {
        if (PlayGamesPlatform.Instance.localUser.authenticated) {
            PlayGamesPlatform.Instance.ShowLeaderboardUI();
        }
        else {
          Debug.Log("Cannot show leaderboard: not authenticated");
        }
    }

    //Method to post value to leaderboard
    public void UpdateLeaderBoard(string leaderBoard, int quantity) {
        if (PlayGamesPlatform.Instance.localUser.authenticated) {
            PlayGamesPlatform.Instance.ReportScore(
            quantity,
            leaderBoard,
            (bool success) => {
                Debug.Log("(KillVirus) points " + quantity
                                    + " posted in  " + leaderBoard
                                    + "  "
                                    + success);
            });
        }
    }
    public void UpdateLeaderBoard(string leaderBoard, float quantity) {
        if (PlayGamesPlatform.Instance.localUser.authenticated) {
            PlayGamesPlatform.Instance.ReportScore(
            (long)quantity,
            leaderBoard,
            (bool success) => {
                Debug.Log("(KillVirus) points " + quantity
                                    + " posted in  " + leaderBoard
                                    + "  "
                                    + success);
            });
        }
    }

    public void ShowAchievements() {
        if (PlayGamesPlatform.Instance.localUser.authenticated) {
            PlayGamesPlatform.Instance.ShowAchievementsUI();
        } else {
            Debug.Log("Cannot show Achievements, not logged in");
        }
    }

    //Method to fully unlock an achievement
    public void UnlockAchievement(string achievement) {
        if (PlayGamesPlatform.Instance.localUser.authenticated) {
            PlayGamesPlatform.Instance.ReportProgress(
            achievement,
            100.0f, (bool success) => {
                Debug.Log("(KillVirus) achievement " + achievement + " Unlocked " +
                    success);
            });
        }
    }

    //Method to increment an achievement
    public void UnlockAchievement(string achievement, int quantity) {
        if (PlayGamesPlatform.Instance.localUser.authenticated) {
            PlayGamesPlatform.Instance.ReportProgress(
            achievement,
            quantity, (bool success) => {
                Debug.Log("(KillVirus) achievement " + achievement
                                    + " increased " + quantity
                                    + "  "
                                    + success);
            });
        }
    }

    private void CheckTutorial() {
        //Si todavia no se mostro el tutorial de google play services se muestra
        if (!PlayerPrefs.HasKey("GPSTuto")) {
            //Lo primero que se hace es setear el playerprefs para que no vuelva a mostrarse
            PlayerPrefs.SetInt("GPSTuto", 1);
            //Se activa el canvas con el tutorial
            GPSTuto.SetActive(true);
        }
    }
}
