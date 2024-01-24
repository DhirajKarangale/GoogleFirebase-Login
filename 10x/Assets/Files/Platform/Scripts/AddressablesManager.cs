using UnityEngine;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


public class AddressablesManager : PersistentSingleton<AddressablesManager>
{
    [SerializeField] AssetReference[] references;
    private AsyncOperationHandle<GameObject>[] datas;


    private void Start()
    {
        // datas = new AsyncOperationHandle<GameObject>[references.Length];
        // StartCoroutine(IEDownload());
    }


    private IEnumerator IEDownload()
    {
        for (int i = 0; i < references.Length; i++)
        {
            datas[i] = references[i].LoadAssetAsync<GameObject>();

            while (!datas[i].IsDone)
            {
                string msg = $"Downloading resourses {i + 1}/{references.Length}";
                float progress = datas[i].GetDownloadStatus().Percent / (references.Length - i);
                Loading.instance.UpdateProgress(progress, msg);
                yield return null;
            }

            yield return datas[i];
        }

        Loading.instance.Disable();
        // Init.instance.TryAutoLogin();
    }

    // private IEnumerator IELoad(int game)
    // {
    //     // AsyncOperation loadOperation = SceneManager.LoadSceneAsync(2);

    //     // float progress = 0;
    //     // string msg = $"Loading game...";

    //     // while (!loadOperation.isDone)
    //     // {
    //     //     progress = Mathf.Clamp01(loadOperation.progress / 0.9f) / 2;
    //     //     Loading.instance.UpdateProgress(progress, msg);
    //     //     yield return null;
    //     // }

    //     // while (progress <= 1)
    //     // {
    //     //     progress += (0.5f / 2);
    //     //     Loading.instance.UpdateProgress(progress, msg);
    //     //     yield return new WaitForSecondsRealtime(1);
    //     // }

    //     // Loading.instance.UpdateProgress(1, msg);
    //     // yield return new WaitForSecondsRealtime(0.5f);
    //     // Loading.instance.Disable();

    //     Load(game);
    // }


    private void Load(int game)
    {
        if (datas[game].Status == AsyncOperationStatus.Succeeded)
        {
            Instantiate(datas[game].Result);
        }
        else
        {
            references[game].LoadAssetAsync<GameObject>().Completed += (operation) =>
            {
                if (operation.Status == AsyncOperationStatus.Succeeded)
                {
                    Instantiate(operation.Result);
                }
            };
        }
    }


    internal void ButtonGame(int game)
    {
        Loading.instance.LoadLevel(2, 3, game, Load);        
    }
}

// https://100-x.s3.ap-south-1.amazonaws.com/