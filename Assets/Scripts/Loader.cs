using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Loader : MonoBehaviour
{
    public TextMeshProUGUI title;
    public Color A = Color.magenta;
    public Color B = Color.blue;
    public float speed = 1.0f;
    bool started;

    void Start()
    {
        StartCoroutine(FlashTitle());
        StartCoroutine(ChangeText());
    }

    IEnumerator FlashTitle()
    {
        while (!started)
        {
            title.color = Color.Lerp(A, B, Mathf.PingPong(Time.time * speed, 1.0f));

            yield return null;
        }     
    }

    IEnumerator ChangeText()
    {
        while (!started)
        {
            title.text = "EAT";

            yield return new WaitForSeconds(.3f);

            title.text = "SLEEP";

            yield return new WaitForSeconds(.3f);

            title.text = "HUNT";

            yield return new WaitForSeconds(.3f);

            title.text = "GATHER";

            yield return new WaitForSeconds(.3f);

            title.text = "DIG";

            yield return new WaitForSeconds(.3f);
        }
    }

    public void StartGame()
    {
        started = true;
        title.text = "LOADING...";
        SceneManager.LoadScene(1);
    }
}
