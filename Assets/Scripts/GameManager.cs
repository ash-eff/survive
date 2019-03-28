using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public Player player;
    public Zone playerInZone;
    public Zone selectedZone;

    public enum Sky { Cloudy, Clear, Sunny }
    public Sky sky;

    public enum Weather { Rain, Snow, NoPrecipitation }
    public Weather weather;

    private string timeOfDay;
    private int day = 1;
    private int hour;
    private int minutes;
    private int islandTemperature;
    private int hottestTempOfTheDay = 60;
    private int tempOffset;
    private int conditionMod = 0;

    private bool gameStarted;
    private bool levelLoaded;
    private bool dayTime;

    private float chanceOfRain;
    private float currentLerpTime;

    public float baseHourScale;
    public float fastHourScale;
    public float hourScale;

    CameraController cam;
    Vector3 mousePos;
    RaycastHit2D hit;

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
    public TextMeshProUGUI dayText;
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
    public TextMeshProUGUI terrainText;
    public TextMeshProUGUI featureText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI statusText;
    #endregion

    #region getters/setters
    public int IslandTemperature
    {
        get { return islandTemperature; }
    }

    public bool GameStarted
    {
        get { return gameStarted; }
    }
    #endregion

    private void Awake()
    {
        cam = FindObjectOfType<CameraController>();
        StartCoroutine(TextFadeIn());
        islandTemperature = hottestTempOfTheDay;
        sky = Sky.Clear;
        weather = Weather.NoPrecipitation;
        hour = Random.Range(23, 24);
        minutes = 00;
        timeOfDay = hour.ToString("00") + ":" + minutes.ToString("00");
        hourScale = baseHourScale;
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

        if (gameStarted)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // if this isn't a UI element
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    hit = Physics2D.Raycast(mousePos, Vector3.zero);
                    if (hit.transform.tag == "Zone")
                    {
                        selectedZone.Selected = false;
                        selectedZone = hit.transform.GetComponent<Zone>();
                        selectedZone.Selected = true;
                    }
                }
            }
        }
    }

    #region island stat functions
    IEnumerator RunClock()
    {
        while (true)
        {
            yield return new WaitForSeconds(hourScale);
            minutes++;
            if(minutes > 59)
            {
                player.ReduceEnergy(45);
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
        tempOffset = Random.Range(-18, 15);

        if(hottestTempOfTheDay + tempOffset > 99)
        {
            hottestTempOfTheDay = 80;
        }
        else if(hottestTempOfTheDay + tempOffset < 10)
        {
            hottestTempOfTheDay = 25;
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
    #endregion

    public void InstantiatePlayer(Zone _zone)
    {
        GameObject playerGo = Instantiate(playerPrefab, _zone.ZonePosition, Quaternion.identity);
        player = FindObjectOfType<Player>();
        player.currentPosition = _zone.ZonePosition;
        playerInZone = _zone;
        selectedZone = _zone;
        selectedZone.Selected = true;
        levelLoaded = true;
    }

    #region intro functions
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
    #endregion

    #region GUI functions
    void LoadZoneInfo()
    {
        if (!selectedZone.Explored)
        {
            plantsText.text = "Plants: ??";
            seedsText.text = "Seeds: ??";
            meatText.text = "Meat: ??";
            furText.text = "Fur: ??";
            medicineText.text = "Meds: ??";
            clothText.text = "Cloth: ??";
            plasticText.text = "Plastic: ??";
            metalText.text = "Metal: ??";
            woodText.text = "Wood: ??";
            rockText.text = "Rocks: ??";
            terrainText.text = "Terrain: " + selectedZone.terrainType.ToString();
            featureText.text = "Feature: ??";
            energyText.text = "Energy: " + selectedZone.ZoneEnergy.ToString();
            statusText.text = "Status: " + selectedZone.status.ToString();
        }
        else
        {
            plantsText.text = "Plants: " + selectedZone.Plants.ToString();
            seedsText.text = "Seeds: " + selectedZone.Seeds.ToString();
            meatText.text = "Meat: " + selectedZone.Meat.ToString();
            furText.text = "Fur: " + selectedZone.Fur.ToString();
            medicineText.text = "Meds: " + selectedZone.Medicinal.ToString();
            clothText.text = "Cloth: " + selectedZone.Cloth.ToString();
            plasticText.text = "Plastic: " + selectedZone.Plastic.ToString();
            metalText.text = "Metal: " + selectedZone.Metal.ToString();
            woodText.text = "Wood: " + selectedZone.Wood.ToString();
            rockText.text = "Rocks: " + selectedZone.Rocks.ToString();
            terrainText.text = "Terrain: " + selectedZone.terrainSubType.ToString() + " " + selectedZone.terrainType.ToString();
            featureText.text = "Feature: " + selectedZone.zoneFeature.ToString();
            energyText.text = "Energy: " + selectedZone.ZoneEnergy.ToString();
            statusText.text = "Status: " + selectedZone.status.ToString();
        }
    }

    void UpdateWorldInfo()
    {
        dayText.text = "Day: " + day.ToString();
        timeText.text = "Time: " + timeOfDay;
        skyText.text = "Conditions: " + sky.ToString();
        weatherText.text = "Weather: " + weather.ToString();
        if(playerInZone != null)
        {
            temperatureText.text = "Zone Temperature: " + playerInZone.zoneTemperature.ToString() + "F";
        }
    }
    #endregion

    #region player button presses
    public void ExploreAreaOnButton()
    {
        
        StartCoroutine(ExploreArea());
    }

    #endregion

    #region player Coroutines
    IEnumerator ExploreArea()
    {
        // if the zone hasn't been explored yet, and we are in the the selected zone
        if (!selectedZone.Explored && selectedZone == playerInZone)
        {
            bool exploring = true;
            // time to explore
            int timeToExplore = 3;
            // energy cost per hour
            int energyCostPerHour = 100;
            // current hour
            int currentHour = hour;
            // current minutes
            int currentMin = minutes;
            // goal hour
            int goal = currentHour + timeToExplore;
            if (goal > 23)
            {
                goal -= 23;
            }

            player.state = Player.State.Exploring;
            // fast forward time
            hourScale = fastHourScale;

            while (exploring)
            {
                if (hour == goal)
                {
                    if (minutes == currentMin)
                    {
                        exploring = false;
                    }
                }

                yield return null;
            }
            // set time to normal
            hourScale = baseHourScale;
            // reduce player energy
            player.ReduceEnergy(energyCostPerHour * timeToExplore);
            // set zone to explored
            selectedZone.Explored = true;
        }
    }

    #endregion
}
