using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject campPrefab;
    public Player player;
    public Zone currentZone;
    public Zone selectedZone;

    public List<Zone> neighbors = new List<Zone>();

    public enum Sky { Cloudy, Clear, Sunny }
    public Sky sky;

    public enum Weather { Rain, Snow, NoPrecipitation }
    public Weather weather;

    public enum State { Island, Camp, }
    public State state = State.Island;

    private string timeOfDay;
    private int day = 1;
    public int hour;
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
    MapManager map;
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
    public GameObject campOverlay;
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI skyText;
    public TextMeshProUGUI weatherText;
    public TextMeshProUGUI temperatureText;
    #endregion

    #region Zone Info UI
    public TextMeshProUGUI waterText;
    public TextMeshProUGUI tracksText;
    public TextMeshProUGUI plantsText;
    public TextMeshProUGUI medicineText;
    public TextMeshProUGUI clothText;
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI rockText;
    public TextMeshProUGUI terrainText;
    public TextMeshProUGUI featureText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI statusText;
    #endregion

    #region Player Info UI
    public Button gatherButton;
    public Button walkButton;
    public Button sleepButton;
    public Button waterButton;
    public Button drinkButton;
    public Button fishButton;
    public Button huntButton;
    public Button buildCampButton;
    public Button enterCampButton;
    public Button exitCamp;
    public TextMeshProUGUI playerEnergyText;
    public TextMeshProUGUI playerMeatText;
    public TextMeshProUGUI playerPlantText;
    public TextMeshProUGUI playerMedText;
    public TextMeshProUGUI playerClothText;
    public TextMeshProUGUI playerWoodText;
    public TextMeshProUGUI playerRockText;
    public TextMeshProUGUI playerWaterText;
    public Toggle treated;
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

    public int Hour
    {
        get { return hour; }
    }

    public int Minutes
    {
        get { return minutes; }
    }

    public int Day
    {
        get { return day; }
    }

    #endregion

    private void Awake()
    {
        cam = FindObjectOfType<CameraController>();
        map = FindObjectOfType<MapManager>();
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
        LoadZoneInfo();
        DayNight();
        UpdatePlayerInfo();
        MouseClick();
        CheckButtons();
        UpdateItems();
        treated.isOn = player.TreatedWater;
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
                hour++;
                minutes = 00;
                int energyLoss = player.totalEnergyLoss;
                player.ReduceEnergy(energyLoss);

                if (hour > 23)
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

    void DayNight()
    {
        if (hour >= 20 || hour >= 00 && hour < 06)
        {
            dayTime = false;
        }
        else
        {
            dayTime = true;
        }
    }

    int RandomNumber(int x, int y)
    {
        return Random.Range(x, y + 1);
    }
    #endregion

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
        player.SetSleepTimer();
    }
    #endregion

    #region GUI functions
    void LoadZoneInfo()
    {
        if (!selectedZone.Explored)
        {
            waterText.text = "Water: ??";
            tracksText.text = "Tracks: ??";
            plantsText.text = "Plants: ??";
            medicineText.text = "Meds: ??";
            clothText.text = "Cloth: ??";
            woodText.text = "Wood: ??";
            rockText.text = "Rocks: ??";
            terrainText.text = "Terrain: " + selectedZone.terrainType.ToString();
            featureText.text = "Feature: ??";
            energyText.text = "Energy: " + selectedZone.ZoneEnergy.ToString();
            statusText.text = "Status: " + selectedZone.status.ToString();
        }
        else
        {
            waterText.text = "Water: " + selectedZone.Water.ToString();
            tracksText.text = "Tracks: " + selectedZone.Flora.ToString();
            plantsText.text = "Plants: " + selectedZone.Plants.ToString();
            medicineText.text = "Meds: " + selectedZone.Medicinal.ToString();
            clothText.text = "Cloth: " + selectedZone.Cloth.ToString();
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
        if(currentZone != null)
        {
            temperatureText.text = "Zone Temperature: " + currentZone.zoneTemperature.ToString() + "F";
        }
    }

    void UpdatePlayerInfo()
    {
        if(player != null)
        {
            playerEnergyText.text = "Energy: " + player.energy;
        }
    }
    #endregion

    #region player button presses
    public void GatherOnButton()
    {
        StartCoroutine(Gather());
    }

    public void WalkOnButton()
    {
        StartCoroutine(Walk());
    }

    public void SleepOnButton()
    {
        StartCoroutine(Sleep());
    }

    public void CollectWaterOnButton()
    {
        player.CollectWater();
    }

    public void BuildCamp()
    {
        StartCoroutine(BuildBasicCamp());
    }

    public void EnterCamp()
    {
        state = State.Camp;
        player.TreatedWater = true;
        campOverlay.SetActive(true);
    }

    public void ExitCamp()
    {
        state = State.Island;
        campOverlay.SetActive(false);
    }

    public void OnDrinkButton()
    {
        player.DrinkWater();
    }

    #endregion

    #region player Coroutines
    IEnumerator Gather()
    {
        Zone zoneToGather = selectedZone;;

        if (zoneToGather == currentZone)
        {        
            bool exploring = true;
            // time to gather
            int timeToGather = 3;
            // energy cost per hour
            int energyCostPerHour = zoneToGather.ZoneEnergy / 2;
            // current hour
            int currentHour = hour;
            // current minutes
            int currentMin = minutes;
            // goal hour
            int goal = currentHour + timeToGather;
            if (goal >= 24)
            {
                goal -= 24;
            }

            player.state = Player.State.Active;
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

            player.GatherItems(zoneToGather);
            // set time to normal
            hourScale = baseHourScale;
            // reduce player energy
            player.ReduceEnergy(energyCostPerHour * timeToGather);
            player.state = Player.State.Rest;
        }
    }

    IEnumerator Walk()
    {
        Zone zoneToWalkTo = selectedZone;
        // if the zone hasn't been explored yet, and we are in the the selected zone
        currentZone.ClearNeighbors();
        currentZone.Occupied = false;
        bool walking = true;
        // time to explore
        int timeToWalk = 1;
        // energy cost per hour
        int energyCostPerHour = zoneToWalkTo.ZoneEnergy;
        // current hour
        int currentHour = hour;
        // current minutes
        int currentMin = minutes;
        // goal hour
        int goal = currentHour + timeToWalk;
        if (goal >= 24)
        {
            goal -= 24;
        }

        player.state = Player.State.Active;
        // fast forward time
        hourScale = fastHourScale;

        while (walking)
        {
            player.transform.position = Vector2.MoveTowards(player.transform.position, zoneToWalkTo.transform.position, .3f * Time.deltaTime);
            if (hour == goal)
            {
                if (minutes == currentMin)
                {
                    walking = false;
                }
            }

            yield return null;
        }

        player.transform.position = zoneToWalkTo.transform.position;
        // set time to normal
        hourScale = baseHourScale;
        // reduce player energy
        player.ReduceEnergy(energyCostPerHour * timeToWalk);
        currentZone = zoneToWalkTo;
        currentZone.Occupied = true;
        currentZone.SetNeighbors();
        // set zone to explored
        zoneToWalkTo.Explored = true;
        player.state = Player.State.Rest;
    }

    IEnumerator Sleep()
    {
        player.state = Player.State.Sleep;

        bool sleeping = true;
        int timeToSleep = 8;
        // current hour
        int currentHour = hour;
        // current minutes
        int currentMin = minutes;
        // goal hour
        int goal = currentHour + timeToSleep;
        if (goal >= 24)
        {
            goal -= 24;
        }

        // fast forward time
        hourScale = fastHourScale / 2;

        while (sleeping)
        {
            if (hour == goal)
            {
                if (minutes == currentMin)
                {
                    sleeping = false;
                }
            }

            yield return null;
        }

        // set time to normal
        hourScale = baseHourScale;
        // reduce player energy
        player.state = Player.State.Rest;
        player.SetSleepTimer();
    }

    IEnumerator BuildBasicCamp()
    {
        player.Wood -= player.MaxWood;
        player.Rocks -= player.MaxRocks;
        player.Plants -= player.MaxPlants;
        player.state = Player.State.Active;
        Zone zoneToBuildOn = currentZone;
        bool building= true;
        int timeToBuild = 5;
        // energy cost per hour
        int energyCostPerHour = 150;
        // current hour
        int currentHour = hour;
        // current minutes
        int currentMin = minutes;
        // goal hour
        int goal = currentHour + timeToBuild;
        if (goal >= 24)
        {
            goal -= 24;
        }

        // fast forward time
        hourScale = fastHourScale / 2;

        while (building)
        {
            if (hour == goal)
            {
                if (minutes == currentMin)
                {
                    building = false;
                }
            }

            yield return null;
        }

        // set time to normal
        hourScale = baseHourScale;
        // reduce player energy
        player.state = Player.State.Rest;
        player.ReduceEnergy(energyCostPerHour * timeToBuild);
        Instantiate(campPrefab, zoneToBuildOn.ZonePosition, Quaternion.identity);
        zoneToBuildOn.Shelter = true;
    }
    #endregion

    public void InstantiatePlayer(Zone _zone)
    {
        GameObject playerGo = Instantiate(playerPrefab, _zone.ZonePosition, Quaternion.identity);
        player = FindObjectOfType<Player>();
        player.currentPosition = _zone.ZonePosition;

        currentZone = _zone;
        currentZone.Occupied = true;
        currentZone.SetNeighbors();

        selectedZone = _zone;
        selectedZone.Selected = true;

        levelLoaded = true;
    }

    void MouseClick()
    {
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

    void CheckButtons()
    {
        // GATHER
        gatherButton.interactable = selectedZone.Occupied && player.state == Player.State.Rest && state != State.Camp;

        if (gatherButton.interactable)
        {
            gatherButton.GetComponentInChildren<Text>().text = "Cost to Gather: " + (selectedZone.ZoneEnergy / 2 * 3) + "eph\n"
                + "Total Hours: 3 Hours";
        }
        else
        {
            gatherButton.GetComponentInChildren<Text>().text = "Gather Supplies";
        }

        // WALK
        walkButton.interactable = IsZoneInWalkingDistance() && player.state == Player.State.Rest && currentZone != selectedZone && state != State.Camp;

        if (walkButton.interactable)
        {
            walkButton.GetComponentInChildren<Text>().text = "Cost to Move: " + (selectedZone.ZoneEnergy + "eph\n" 
                + "Total Hours: 1 Hour");
        }
        else
        {
            walkButton.GetComponentInChildren<Text>().text = "Move";
        }

        // SLEEP
        sleepButton.interactable = player.state == Player.State.Rest;

        if (sleepButton.interactable)
        {
            sleepButton.GetComponentInChildren<Text>().text = "Cost to Sleep: 45eph\n"
                + "Total Hours: 8 Hour";
        }
        else
        {
            sleepButton.GetComponentInChildren<Text>().text = "Sleep";
        }

        // Collect Water
        waterButton.interactable = player.state == Player.State.Rest && currentZone.Water && player.GallonWater < 1 && state != State.Camp;

        if (waterButton.interactable)
        {
            waterButton.GetComponentInChildren<Text>().text = "Collect Water";
        }
        else
        {
            waterButton.GetComponentInChildren<Text>().text = "Find Water";
        }

        // Drink Water
        drinkButton.interactable = player.state == Player.State.Rest && player.GallonWater > 0;

        if (drinkButton.interactable)
        {
            drinkButton.GetComponentInChildren<Text>().text = "Drink Water";
        }
        else
        {
            drinkButton.GetComponentInChildren<Text>().text = "No Water";
        }

        // Fish
        fishButton.interactable = player.state == Player.State.Rest && currentZone.Water && state != State.Camp;

        if (fishButton.interactable)
        {
            fishButton.GetComponentInChildren<Text>().text = "Fish";
        }
        else
        {
            fishButton.GetComponentInChildren<Text>().text = "Can't Fish";
        }

        // Hunt
        huntButton.interactable = player.state == Player.State.Rest && currentZone.Fauna && state != State.Camp;

        if (huntButton.interactable)
        {
            huntButton.GetComponentInChildren<Text>().text = "Hunt";
        }
        else
        {
            huntButton.GetComponentInChildren<Text>().text = "Can't Hunt";
        }

        // Build Camp
        if (!currentZone.Shelter && player.state == Player.State.Rest && player.CanBuildBasicShelter() && state != State.Camp)
        {
            buildCampButton.gameObject.SetActive(true);
            buildCampButton.GetComponentInChildren<Text>().text = "Build Camp";
        }
        else
        {
            buildCampButton.gameObject.SetActive(false);
        }

        // Enter Camp
        if(currentZone.Shelter && player.state == Player.State.Rest && state != State.Camp)
        {
            enterCampButton.gameObject.SetActive(true);
            enterCampButton.GetComponentInChildren<Text>().text = "Enter Camp";
        }
        else
        {
            enterCampButton.gameObject.SetActive(false);
        }
    }

    bool IsZoneInWalkingDistance()
    {
        bool zoneInRange = false;
        foreach (Zone neighbor in neighbors)
        {
            if (selectedZone.ZonePosition == neighbor.ZonePosition)
            {
                zoneInRange = true;
            }
        }

        return zoneInRange;
    }

    void UpdateItems()
    {
        if (gameStarted)
        {
            playerMeatText.text = "Meat " + player.Meat.ToString() + "/" + player.MaxMeat.ToString();
            playerPlantText.text = "Plants " + player.Plants.ToString() + "/" + player.MaxPlants.ToString();
            playerMedText.text = "Meds " + player.Medicinal.ToString() + "/" + player.MaxMedicinal.ToString();
            playerClothText.text = "Cloth " + player.Cloth.ToString() + "/" + player.MaxCloth.ToString();
            playerWoodText.text = "Wood " + player.Wood.ToString() + "/" + player.MaxWood.ToString();
            playerRockText.text = "Rock " + player.Rocks.ToString() + "/" + player.MaxRocks.ToString();
            playerWaterText.text = "Water " + player.GallonWater.ToString() + " gal.";
        }
    }


}