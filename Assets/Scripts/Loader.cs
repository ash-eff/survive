using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Loader : MonoBehaviour
{
    AsyncOperation asyncLoad;
    public Image loadingImage;
    public TextMeshProUGUI loading;


    void Start()
    {
        StartCoroutine(LoadGame());
        StartCoroutine(LoadingFlash());
    }

    IEnumerator LoadingFlash()
    {
        while (true)
        {
            loading.text = "Loading";
            yield return new WaitForSeconds(.3f);

            loading.text = "Loading.";
            yield return new WaitForSeconds(.3f);

            loading.text = "Loading..";
            yield return new WaitForSeconds(.3f);

            loading.text = "Loading...";
            yield return new WaitForSeconds(.3f);
        }
    }


    IEnumerator LoadGame()
    {
        asyncLoad = SceneManager.LoadSceneAsync("Main");

        while (!asyncLoad.isDone)
        {
            Debug.Log("Loading...");
            loadingImage.fillAmount = asyncLoad.progress;
            yield return null;
        }
    }
}
