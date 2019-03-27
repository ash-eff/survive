using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameObject player;
    public Zone playerInZone;

    public bool gameStarted;
    public bool levelLoaded;

    public enum Sky { Cloudy, Clear, Sunny }
    public Sky sky;

    public enum Weather { Rain, Snow, NoPrecipitation }
    public Weather weather;

    public string timeOfDay;
    public int day = 1;
    public bool dayTime;

    private int hour;
    private int minutes;
    public int islandTemperature;
    private int hottestTempOfTheDay = 60;
    private int tempOffset;
    public float chanceOfRain;
    private int conditionMod = 0;
    public float hourScale;

    #region intro screen
    public GameObject startScreen;
    public GameObject blackScreen;
    public TextMeshProUGUI startingText;
    public Color inColor;
    public Color outColor;
    public Color outColor2;
    public float lerpTime;
    #endregion

    #region World Info UI
    public GameObject worldOverlay;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI skyText;
    public TextMeshProUGUI weatherText;
    public TextMeshProUGUI temperatureText;
    #endregion

    #region Zone Info UI
    public TextMeshProUGUI plantsText;
    public TextMeshProUGUI seedsText;
    public TextMeshProUGUI meatText;
    public TextMeshProUGUI furText;
    public TextMeshProUGUI medicineText;
    public TextMeshProUGUI clothText;
    public TextMeshProUGUI plasticText;
    public TextMeshProUGUI metalText;
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI rockText;
    #endregion


    CameraController cam;

    float currentLerpTime;

    public int IslandTemperature
    {
        get { return islandTemperature; }
    }

    private void Awake()
    {
        cam = FindObjectOfType<CameraController>();
        StartCoroutine(TextFadeIn());
        islandTemperature = hottestTempOfTheDay;
        sky = Sky.Clear;
        weather = Weather.NoPrecipitation;
        hour = Random.Range(23, 24);
        minutes = Random.Range(00, 60);
        timeOfDay = hour.ToString("00") + ":" + minutes.ToString("00");
        CheckTemperature();
    }

    private void Update()
    {
        UpdateWorldInfo();

        if (playerInZone != null)
        {
            LoadZoneInfo();
        }

        if (hour >= 20 || hour >= 00 && hour < 06)
        {
            dayTime = false;
        }
        else
        {
            dayTime = true;
        }
    }

    IEnumerator RunClock()
    {
        while (true)
        {
            yield return new WaitForSeconds(hourScale);
            minutes++;
            if(minutes > 59)
            {
                hour++;
                minutes = 00;

                if(hour > 23)
                {
                    hour = 00;
                    day++;
                }

                CheckTemperature();
            }
            timeOfDay = hour.ToString("00") + ":" + minutes.ToString("00");
        }
    }

    void GetNewDayTemperature()
    {
        tempOffset = Random.Range(-10, 11);

        if(hottestTempOfTheDay + tempOffset > 99)
        {
            hottestTempOfTheDay = 80;
        }
        else if(hottestTempOfTheDay + tempOffset < 25)
        {
            hottestTempOfTheDay = 40;
        }
        else
        {
            hottestTempOfTheDay += tempOffset;
        }
        
        CheckChanceOfRain();
    }

    void CheckTemperature()
    {
        if(hour == 00)
        {
            GetNewDayTemperature();
        }

        if(hour >= 00 && hour < 02) // between 12am and 2am
        {
            islandTemperature = RandomNumber(hottestTempOfTheDay - 10, hottestTempOfTheDay - 5) + conditionMod;
        }
        else if (hour >= 02 && hour < 04) // between 2am and 4am
        {
            islandTemperature = RandomNumber(hottestTempOfTheDay - 8, hottestTempOfTheDay - 4) + conditionMod;
        }
        else if (hour >= 04 && hour < 06) // between 4am and 6am
        {
            islandTemperature = RandomNumber(hottestTempOfTheDay - 6, hottestTempOfTheDay - 3) + conditionMod;
        }
        else if (hour >= 06 && hour < 08) // between 6am and 8am
        {
            islandTemperature = RandomNumber(hottestTempOfTheDay - 3, hottestTempOfTheDay - 1) + conditionMod;
        }
        else if (hour >= 08 && hour < 10) // between 8am and 10am
        {
            islandTemperature = RandomNumber(hottestTempOfTheDay - 3 , hottestTempOfTheDay - 1) + conditionMod;
        }
        else if (hour >= 10 && hour < 12) // between 10am and 12pm
        {
            islandTemperature = RandomNumber(hottestTempOfTheDay -3 , hottestTempOfTheDay) + conditionMod;
        }
        else if (hour >= 12 && hour < 14) // between 12pm and 2pm
        {
            islandTemperature = RandomNumber(hottestTempOfTheDay - 1 , hottestTempOfTheDay) + conditionMod;
        }
        else if (hour >= 14 && hour < 16) // between 2pm and 4pm
        {
            islandTemperature = RandomNumber(hottestTempOfTheDay - 2, hottestTempOfTheDay) + conditionMod;
        }
        else if (hour >= 16 && hour < 18) // between 4pm and 6pm
        {
            islandTemperature = RandomNumber(hottestTempOfTheDay - 3, hottestTempOfTheDay - 1) + conditionMod;
        }
        else if (hour >= 18 && hour < 20) // between 6pm and 8pm
        {
            islandTemperature = RandomNumber(hottestTempOfTheDay - 6, hottestTempOfTheDay - 3) + conditionMod;
        }
        else if (hour >= 20 && hour < 22) // between 8pm and 10m
        {
            islandTemperature = RandomNumber(hottestTempOfTheDay - 8 , hottestTempOfTheDay - 4) + conditionMod;
        }
        else if (hour >= 20) // beyond 10pm
        {
            islandTemperature = RandomNumber(hottestTempOfTheDay - 10, hottestTempOfTheDay - 5) + conditionMod;
        }

        CheckCurrentWeather();
    }

    void CheckChanceOfRain()
    {
        chanceOfRain = Random.value;
    }

    void CheckCurrentWeather()
    {
        conditionMod = 0;

        // if the chance of rain is above 50%, it's cloudy
        if(chanceOfRain > .5)
        {
            weather = Weather.NoPrecipitation;
            sky = Sky.Cloudy;
            conditionMod += -2;

            // determine if it's raining 
            if(Random.value < chanceOfRain)
            {
                // it's raining.
                if(islandTemperature < 32)
                {
                    weather = Weather.Snow;
                    conditionMod += -4;
                }
                else
                {
                    weather = Weather.Rain;
                    conditionMod += -2;
                }

            }
        }
        else
        {
            if (dayTime)
            {
                weather = Weather.NoPrecipitation;
                sky = Sky.Sunny;
                conditionMod += 2;
            }
            else
            {
                weather = Weather.NoPrecipitation;
                sky = Sky.Clear;    
            }

            // determine if it's raining 
            if (Random.value < chanceOfRain)
            {
                // it's raining.
                if (islandTemperature < 32)
                {
                    sky = Sky.Cloudy;
                    conditionMod += -2;
                    weather = Weather.Snow;
                    conditionMod += -4;
                }
                else
                {
                    sky = Sky.Cloudy;
                    conditionMod += -2;
                    weather = Weather.Rain;
                    conditionMod += -2;
                }
            }
        }
    }

    int RandomNumber(int x, int y)
    {
        return Random.Range(x, y + 1);
    }

    public void InstantiatePlayer(Zone _zone)
    {
        GameObject playerGo = Instantiate(player, _zone.ZonePosition, Quaternion.identity);
        playerGo.GetComponent<Player>().currentPosition = _zone.ZonePosition;
        playerInZone = _zone;
        levelLoaded = true;
    }

    IEnumerator TextFadeIn()
    {
        currentLerpTime = 0;
        while (startingText.color.a < .9f)
        {
            currentLerpTime += Time.deltaTime;

            float perc = currentLerpTime / lerpTime;
            startingText.color = Color.Lerp(startingText.color, inColor, perc);
            yield return null;
        }

        while(!levelLoaded)
        {
            yield return null;
        }

        StartCoroutine(TextFadeOut());
    }

    IEnumerator TextFadeOut()
    {
        currentLerpTime = 0;
        while (startingText.color.a > .01f)
        {
            currentLerpTime += Time.deltaTime;

            float perc = currentLerpTime / lerpTime;
            startingText.color = Color.Lerp(startingText.color, outColor, perc);
            yield return null;
        }

        StartCoroutine(FadeOutBlackScreen());
        cam.PlayerLoaded();
    }

    IEnumerator FadeOutBlackScreen()
    {
        currentLerpTime = 0;
        Image blackScreenSpr = blackScreen.GetComponent<Image>();
        while (blackScreenSpr.color.a > .01f)
        {
            currentLerpTime += Time.deltaTime;

            float perc = currentLerpTime / lerpTime;
            blackScreenSpr.color = Color.Lerp(blackScreenSpr.color, outColor2, perc);
            yield return null;
        }

        startScreen.SetActive(false);

        while (!cam.ready)
        {
            yield return null;
        }

        gameStarted = true;
        worldOverlay.SetActive(true);
        StartCoroutine(RunClock());
    }

    void LoadZoneInfo()
    {
        plantsText.text = "Plants: " + playerInZone.Plants.ToString();
        seedsText.text = "Seeds: " + playerInZone.Seeds.ToString();
        meatText.text = "Meat: " + playerInZone.Meat.ToString();
        furText.text = "Fur: " + playerInZone.Fur.ToString();
        medicineText.text = "Meds: " + playerInZone.Medicinal.ToString();
        clothText.text = "Cloth: " + playerInZone.Cloth.ToString();
        plasticText.text = "Plastic: " + playerInZone.Plastic.ToString();
        metalText.text = "Metal: " + playerInZone.Metal.ToString();
        woodText.text = "Wood: " + playerInZone.Wood.ToString();
        rockText.text = "Rocks: " + playerInZone.Rocks.ToString();
    }

    void UpdateWorldInfo()
    {
        timeText.text = "Time: " + timeOfDay;
        skyText.text = "Conditions: " + sky.ToString();
        weatherText.text = "Weather: " + weather.ToString();
        temperatureText.text = "Temperature: " + islandTemperature + "F";
    }
}
