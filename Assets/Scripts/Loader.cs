using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Loader : MonoBehaviour
{
    public bool sceneLoaded;
    public TextMeshProUGUI text;
    public Color inColor;
    public Color outColor;
    public float lerpTime;
    float currentLerpTime;
    AsyncOperation asyncLoad;

    void Start()
    {
        asyncLoad = SceneManager.LoadSceneAsync("Main");
        asyncLoad.allowSceneActivation = false;
        StartCoroutine(TextFadeIn());
    }


    IEnumerator TextFadeIn()
    {
        Debug.Log("Fade In");
        while (text.color.a < .9)
        {
            currentLerpTime += Time.deltaTime;

            float perc = currentLerpTime / lerpTime;
            text.color = Color.Lerp(text.color, inColor,perc);
            yield return null;
        }

        StartCoroutine(TextFadeOut());
    }

    IEnumerator TextFadeOut()
    {
        Debug.Log("Fade Out");
        while (text.color.a > .01)
        {
            currentLerpTime += Time.deltaTime;

            float perc = currentLerpTime / lerpTime;
            text.color = Color.Lerp(text.color, outColor, perc);
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;
    }
}
