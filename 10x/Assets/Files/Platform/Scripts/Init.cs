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

    private bool isAddressableLoaded;
    private bool isOffline;
    private FirebaseAuth auth;
    private GoogleSignInConfiguration configuration;
    private readonly string webClientId = "210287035650-2npd2g67sq1qlooomsju4rdjb8cdir6c.apps.googleusercontent.com";

    protected override void Awake()
    {
        base.Awake();
        
        configuration = new GoogleSignInConfiguration { WebClientId = webClientId, RequestEmail = true, RequestIdToken = true };
        CheckDependencies();
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
            StopAllCoroutines();

            Invoke(nameof(TryAutoLogin), 5);
        }
    }


    private void CheckDependencies()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
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
            using (IEnumerator<Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                }
            }
        }
        else if (task.IsCanceled)
        {
            Debug.Log("Canceled");
        }
        else
        {
            string s = "Welcome: " + task.Result.DisplayName + "!";
            s += ("Email = " + task.Result.Email);
            // s += ("Google ID Token = " + task.Result.IdToken);

            Debug.Log("User Data : " + s);
            SignInWithGoogleOnFirebase(task.Result.IdToken);
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

    private void SignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }

    private void OnSignOut()
    {
        GoogleSignIn.DefaultInstance.SignOut();
    }

    private void LoadGame()
    {
        Loading.instance.LoadLevel(1, 2, 0, null);
    }

    private void SaveUserData()
    {
        // SaveData

        LoadGame();
    }


    internal void TryAutoLogin()
    {
        if (isAddressableLoaded) return;

        isAddressableLoaded = true;
        CancelInvoke();

        if (PlayerPrefs.HasKey("FBToken"))
        {
            // StartCoroutine(IEAutoLogin());
            // Auto Login
        }
    }


    public void ButtonGoogle()
    {
        if (isOffline)
        {
            // Msg
            Debug.Log("============== You are offline");
            return;
        }

        auth = FirebaseAuth.DefaultInstance;
        SignIn();
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



    // // [SerializeField] GameObject objEventSystem;
    // [SerializeField] GameObject objFB;
    // [SerializeField] GameObject objGuest;
    // 

    // private const string titleId = "98603";


    // private void Start()
    // {
    //     // PlayerPrefs.DeleteAll();
    //     if (!PlayerPrefs.HasKey("FBToken"))
    //     {
    //         Offline();
    //         StartCoroutine(IECheckInternet());
    //     }

    //     if (!FB.IsInitialized) FB.Init(() => FB.ActivateApp());
    // }



    // private IEnumerator IEAutoLogin()
    // {
    //     UnityWebRequest request = new UnityWebRequest("http://google.com");
    //     yield return request.SendWebRequest();

    //     if (request.error != null)
    //     {
    //         StopAllCoroutines();
    //         Offline();
    //     }
    //     else
    //     {
    //         Online();
    //     }
    // }


    // private void DefaultScreen()
    // {
    //     objFB.SetActive(true);
    //     objGuest.SetActive(true);
    // }

    // private void Online()
    // {
    //     txtStatus.color = Color.green;
    //     txtStatus.text = "Online";
    //     DefaultScreen();
    //     if (PlayerPrefs.HasKey("FBToken")) AutoLogin();
    //     else Loading.instance.Disable();
    // }

    // private void Offline()
    // {
    //     txtStatus.color = Color.red;
    //     txtStatus.text = "Offline";
    //     objFB.SetActive(true);
    //     objGuest.SetActive(true);
    //     Loading.instance.Disable();
    //     StartCoroutine(IECheckInternet());
    // }

    // private void AutoLogin()
    // {
    //     objFB.SetActive(false);
    //     objGuest.SetActive(false);
    //     LoginWithPlayfab(PlayerPrefs.GetString("FBToken"));
    // }

    // private void LoginWithPlayfab(string token)
    // {
    //     Loading.instance.Active();
    //     // Invoke(nameof(Offline), 10);
    //     PlayFabClientAPI.LoginWithFacebook(new PlayFab.ClientModels.LoginWithFacebookRequest
    //     {
    //         TitleId = titleId,
    //         AccessToken = token,
    //         CreateAccount = true,
    //         InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
    //         {
    //             GetPlayerProfile = true
    //         }
    //     }, PlayfabLoginSucess, PlafabLoginFail);
    // }

    // private void PlayfabLoginSucess(PlayFab.ClientModels.LoginResult result)
    // {
    //     if (!PlayerPrefs.HasKey("FBToken") || Application.platform == RuntimePlatform.Android)
    //     {
    //         if (FB.IsLoggedIn)
    //         {
    //             PlayerPrefs.SetString("FBToken", AccessToken.CurrentAccessToken.TokenString);
    //             UpdatePlayerData();
    //         }
    //         else FBLogin();
    //     }

    //     PlayerPrefs.SetString("PlayfabId", result.PlayFabId);
    //     PlayerPrefs.SetInt("LoggedIn", 1);
    //     LoadGame();
    // }

    // private void PlafabLoginFail(PlayFabError error)
    // {
    //     Offline();
    // }

    // private void UpdatePlayerData()
    // {
    //     FB.API("me?fields=name", Facebook.Unity.HttpMethod.GET, GetFBName);
    //     FB.API("/me/picture?redirect=false", HttpMethod.GET, GetFBPic);
    // }

    // private void GetFBPic(IGraphResult result)
    // {
    //     if (string.IsNullOrEmpty(result.Error) && !result.Cancelled)
    //     {
    //         IDictionary data = result.ResultDictionary["data"] as IDictionary;
    //         string url = data["url"] as string;

    //         if (string.IsNullOrEmpty(url)) return;
    //         PlayFabClientAPI.UpdateAvatarUrl(new UpdateAvatarUrlRequest()
    //         {
    //             ImageUrl = url
    //         }, OnSuccess => { }, OnFailed => { });
    //     }
    // }

    // private void GetFBName(Facebook.Unity.IGraphResult result)
    // {
    //     string fbName = result.ResultDictionary["name"].ToString();
    //     fbName = ClampName(fbName);
    //     var request = new UpdateUserTitleDisplayNameRequest
    //     {
    //         DisplayName = fbName,
    //     };
    //     PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnSucessUpdatePlayerData => { PlayerPrefs.SetString("PlayerName", fbName); }, OnError);
    // }

    // private string ClampName(string name)
    // {
    //     if (name.Length <= 6) return name + "    ";
    //     else if (name.Length >= 25) return name.Substring(0, 25);
    //     return name;
    // }

    // private void FBLogin()
    // {
    //     var perms = new List<string>() { "email", "gaming_user_picture" };
    //     FB.LogInWithReadPermissions(perms, OnFBLogin);
    // }

    // private void OnFBLogin(ILoginResult result)
    // {
    //     if (FB.IsLoggedIn)
    //     {
    //         PlayerPrefs.SetString("FBToken", AccessToken.CurrentAccessToken.TokenString);
    //         UpdatePlayerData();
    //     }
    //     else
    //     {
    //         DefaultScreen();
    //     }
    // }

    // private void OnError(PlayFabError error)
    // {
    //     Offline();
    // }

    // private void LoadGame()
    // {
    //     // objEventSystem.SetActive(false);
    //     Loading.instance.LoadLevel(1, 2, 0, null);
    // }




}