using Google;
using System;
using Firebase;
using UnityEngine;
using Firebase.Auth;
using System.Collections;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections.Generic;
using Firebase.Extensions;


public class Init : Singleton<Init>
{
    [SerializeField] TMPro.TMP_Text txtStatus;

    private bool isOffline;
    private FirebaseAuth auth;
    private GoogleSignInConfiguration configuration;
    private readonly string webClientId = "210287035650-2npd2g67sq1qlooomsju4rdjb8cdir6c.apps.googleusercontent.com";

    protected override void Awake()
    {
        base.Awake();

        configuration = new GoogleSignInConfiguration { WebClientId = webClientId, RequestEmail = true, RequestIdToken = true };
        // CheckDependencies();
        StartCoroutine(IECheckInternet());
    }


    private IEnumerator IECheckInternet()
    {
        UnityWebRequest request = new UnityWebRequest("http://google.com");
        yield return request.SendWebRequest();

        if (request.error != null)
        {
            isOffline = true;
            txtStatus.color = Color.red;
            txtStatus.text = "Offline";

            yield return new WaitForSecondsRealtime(2);

            StartCoroutine(IECheckInternet());
        }
        else
        {
            txtStatus.color = Color.green;
            txtStatus.text = "Online";
            isOffline = false;

            yield return new WaitUntil(() => AddressablesManager.instance.isDownloaded);

            if (PlayerPrefs.HasKey("Email")) LoadGame();

            StopAllCoroutines();
        }
    }


    private void CheckDependencies()
    {
        // FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                if (task.Result == DependencyStatus.Available)
                {
                    auth = FirebaseAuth.DefaultInstance;
                }
            }
        });
    }

    private void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {

        }
        else if (task.IsCanceled)
        {

        }
        else
        {
            string s = "Welcome: " + task.Result.DisplayName + "!";
            s += ("Email = " + task.Result.Email);
            Debug.Log("User Data: " + s);

            Loading.instance.LoadLevel(1, 2, 0, null);
            // SaveUserData(task.Result);
        }
    }

    private void SignInWithGoogleOnFirebase(string idToken)
    {
        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

        auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {
            // AggregateException ex = task.Exception;
            // Debug.Log(" ================ Getting Ex : " + ex.Message);
            // if (ex != null)
            // {
            //     if (ex.InnerExceptions[0] is FirebaseException inner && (inner.ErrorCode != 0))
            //         Debug.Log("\nError code = " + inner.ErrorCode + " Message = " + inner.Message);
            // }
            // else
            // {
            //     Debug.Log("Sign In Successful.");
            //     SaveUserData();
            // }

            if (task.IsCanceled)
            {
                // signInCompleted.SetCanceled();
                Debug.Log("======================= Sign in cancled");
            }
            else if (task.IsFaulted)
            {
                Debug.Log("======================= Sign in Faulted");
                // signInCompleted.SetException(authTask.Exception);
            }
            else
            {
                Debug.Log("======================= Sign in Sucess");
                // signInCompleted.SetResult(((Task<FirebaseUser>)authTask).Result);
            }
        });
    }

    private IEnumerator SignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;

        // GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
        var x = GoogleSignIn.DefaultInstance.SignIn();
        yield return new WaitUntil(() => x.IsCompleted || x.IsCompletedSuccessfully);

        SaveData(x.Result);
    }

    private void OnSignOut()
    {
        GoogleSignIn.DefaultInstance.SignOut();
    }

    private void LoadGame()
    {
        if (PlayerPrefs.HasKey("Email")) PlayerPrefs.SetInt("LoggedIn", 1);
        else PlayerPrefs.SetInt("LoggedIn", 0);

        Loading.instance.LoadLevel(1, 2, 0, null);
    }

    private void SaveData(GoogleSignInUser user)
    {
        PlayerPrefs.SetString("Email", user.Email);
        PlayerPrefs.SetString("Token", user.IdToken);
        PlayerPrefs.SetString("Name", user.DisplayName);
        PlayerPrefs.SetString("Image", user.ImageUrl.ToString());

        LoadGame();
    }


    public void ButtonGoogle()
    {
        if (isOffline)
        {
            return;
        }

        StartCoroutine(SignIn());
    }

    public void ButtonGuest()
    {
        PlayerPrefs.SetInt("LoggedIn", 0);
        LoadGame();
    }

    public void ButtonHelp()
    {
        string emailBody = "Describe your issue in detail.";
        string emailSubject = "JJ3D - " + PlayerPrefs.GetString("PlayerName", "Your Name");
        emailSubject = System.Uri.EscapeUriString(emailSubject);
        Application.OpenURL("mailto:helphastute@gmail.com" + "?subject=" + emailSubject + "&body=" + emailBody);
    }

    public void ButtonLink(string url)
    {
        Application.OpenURL(url);
    }
}