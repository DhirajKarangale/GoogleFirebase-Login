using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class Loading : PersistentSingleton<Loading>
{
    [SerializeField] GameObject obj;
    [SerializeField] Slider slider;
    [SerializeField] TMP_Text txtMsg;
    [SerializeField] TMP_Text txtProgress;


    private void Start()
    {
        Disable();
    }


    private IEnumerator IELoadLevel(int scene, int extraTime, int param, Action<int> callBack)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(scene);

        float progress = 0;
        string msg = $"Loading game...";

        while (!loadOperation.isDone)
        {
            progress = Mathf.Clamp01(loadOperation.progress / 0.9f) / 2;
            UpdateProgress(progress, msg);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.1f);
        callBack?.Invoke(param);

        while (progress < 1)
        {
            progress += (0.5f / extraTime);
            UpdateProgress(progress, msg);
            yield return new WaitForSecondsRealtime(1);
        }

        UpdateProgress(1, msg);
        yield return new WaitForSecondsRealtime(0.5f);
        Disable();
    }


    internal void UpdateProgress(float val, string msg)
    {
        obj.SetActive(true);
        txtMsg.text = msg;
        slider.value = val;
        txtProgress.text = (val * 100).ToString("F0") + "%";
    }

    internal void Disable()
    {
        txtMsg.text = "";
        txtProgress.text = "";
        slider.value = 0;
        obj.SetActive(false);
    }

    internal void Active()
    {
        txtMsg.text = "";
        txtProgress.text = "";
        slider.value = 0;
        obj.SetActive(true);
    }

    internal void LoadLevel(int scene, int extraTime, int param, Action<int> callBack)
    {
        StartCoroutine(IELoadLevel(scene, extraTime, param, callBack));
    }
}