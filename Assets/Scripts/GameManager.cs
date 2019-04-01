using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject campPrefab;
    public GameObject beamPrefab;
    public Player player;
    public Zone currentZone;
    public Zone selectedZone;
    public LayerMask mouseMask;
    public TextMeshProUGUI feedbackText;
    public Button exitButton;

    private GameObject beamGO;

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
    public bool gameOver;
    private bool callGo = true;

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
    public TextMeshProUGUI endText;
    public Color inColor;
    public Color outColor;
    public Color outColor2;
    public Color deathInColor;
    public Color startInColor;
    public float lerpTime;
    #endregion

    #region World Info UI
    public GameObject worldOverlay;
    public GameObject campOverlay;
    public GameObject deathScreen;
    public GameObject deathScreenText;
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
    public Button eatButton;
    public Button medButton;
    public Button fishButton;
    public Button huntButton;
    public Button excavateButton;
    public Button buildCampButton;
    public Button buildSpearButton;
    public Button buildFishingPoleButton;
    public Button buildBoatButton;
    public Button enterCampButton;
    public Button exitCampButton;
    public TextMeshProUGUI playerEnergyText;
    public TextMeshProUGUI playerRestingText;
    public TextMeshProUGUI playerMeatText;
    public TextMeshProUGUI playerPlantText;
    public TextMeshProUGUI playerMedText;
    public TextMeshProUGUI playerClothText;
    public TextMeshProUGUI playerWoodText;
    public TextMeshProUGUI playerRockText;
    public TextMeshProUGUI playerWaterText;
    public TextMeshProUGUI playerRelicText;
    public Toggle treated;
    public Toggle cooked;
    public Toggle refined;
    public Toggle spear;
    public Toggle fishingPole;
    public Toggle boat;
    public Image thirstBar;
    public Image fatigueBar;
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
        cooked.isOn = player.CookedFood;
        refined.isOn = player.RefinedMeds;
        spear.isOn = player.Spear;
        fishingPole.isOn = player.FishingPole;
        boat.isOn = player.Boat;

        if (player.Relics == map.numberOfRelics && callGo)
        {
            callGo = false;
            GameOver(true);
        }

        if(player.energy <= 0 && callGo)
        {
            callGo = false;
            GameOver(false);
        }

        CheckThirst();
        CheckFatigue();
    }

    #region island stat functions
    IEnumerator RunClock()
    {
        while (true)
        {
            yield return new WaitForSeconds(hourScale);
            minutes++;
            player.fatigueTimer++;
            if (minutes > 59)
            {
                hour++;
                player.thirstTimer++;                
                minutes = 00;
                int energyLoss = player.totalEnergyLoss;
                player.ReduceEnergy(energyLoss);
                if (hourScale == fastHourScale)
                {
                    LogFeedback("...");
                }

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
            playerRestingText.text = "REL: " + player.totalEnergyLoss;
        }
    }
    #endregion

    #region player button presses
    public void OnGatherButton()
    {
        StartCoroutine(Gather());
    }

    public void OnWalkButton()
    {
        StartCoroutine(Walk());
    }

    public void OnSleepButton()
    {
        StartCoroutine(Sleep());
    }

    public void OnCollectWaterButton()
    {
        player.CollectWater();
    }

    public void OnBuildCampButton()
    {
        StartCoroutine(BuildBasicCamp());
    }

    public void OnBuildSpear()
    {
        StartCoroutine(BuildSpear());
    }

    public void OnBuildFishingPole()
    {
        StartCoroutine(BuildFishingPole());
    }

    public void OnBuildBoat()
    {
        StartCoroutine(BuildBoat());
    }

    public void OnEnterCampButton()
    {
        state = State.Camp;
        LogFeedback("You Entered Camp.");

        if (player.GallonWater > 0)
        {
            LogFeedback("You Treated Your Water.");
            player.TreatedWater = true;
        }

        if(player.Meat > 0)
        {
            LogFeedback("You Cooked Your Food.");
            player.CookedFood = true;
        }

        if (player.Medicinal > 0)
        {
            LogFeedback("You Refined Your Medicinal Plants.");
            player.RefinedMeds = true;
        }

        campOverlay.SetActive(true);
    }

    public void OnLeaveCampButton()
    {
        state = State.Island;
        campOverlay.SetActive(false);
        LogFeedback("You Left Camp.");
    }

    public void OnDrinkButton()
    {
        player.DrinkWater();
    }

    public void OnHuntButton()
    {
        StartCoroutine(Hunt());
    }

    public void OnFishButton()
    {
        StartCoroutine(Fish());
    }

    public void OnEatButton()
    {
        player.EatFood();
    }

    public void OnMedicineButton()
    {
        player.TakeMeds();
    }

    public void OnExcavateButton()
    {
        StartCoroutine(Excavate());
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
            int energyCostPerHour = zoneToGather.ZoneEnergy / 4;
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

            LogFeedback("You Begin Gathering Items.");

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

            LogFeedback("You Finish Gathering Complete.");

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

        LogFeedback("Walking.");

        while (walking)
        {
            player.transform.position = Vector2.MoveTowards(player.transform.position, zoneToWalkTo.transform.position, 1f * Time.deltaTime);
            if (hour == goal)
            {
                if (minutes == currentMin)
                {
                    walking = false;
                }
            }

            yield return null;
        }

        LogFeedback("You Have Arrived.");

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

        LogFeedback("You Fell Asleep.");

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

        LogFeedback("You Wake Up.");

        // set time to normal
        hourScale = baseHourScale;
        // reduce player energy
        player.state = Player.State.Rest;
        player.SetSleepTimer();
        player.fatigueTimer = 0;
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
        int energyCostPerHour = 50;
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

        LogFeedback("You Start Building Camp.");

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

        LogFeedback("You Finish Building Camp.");

        // set time to normal
        hourScale = baseHourScale;
        // reduce player energy
        player.state = Player.State.Rest;
        player.ReduceEnergy(energyCostPerHour * timeToBuild);
        Instantiate(campPrefab, zoneToBuildOn.ZonePosition, Quaternion.identity);
        zoneToBuildOn.Shelter = true;
    }

    IEnumerator BuildSpear()
    {
        player.Wood -= 2;
        player.Rocks -= 1;
        player.Plants -= 10;
        player.state = Player.State.Active;
        bool building = true;
        int timeToBuild = 1;
        // energy cost per hour
        int energyCostPerHour = 20;
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

        LogFeedback("You Start Building A Spear.");

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

        LogFeedback("You Finish Building A Spear.");

        // set time to normal
        hourScale = baseHourScale;
        // reduce player energy
        player.state = Player.State.Rest;
        player.ReduceEnergy(energyCostPerHour * timeToBuild);
    }

    IEnumerator BuildFishingPole()
    {
        player.Wood -= 2;
        player.Cloth -= 1;
        player.Plants -= 10;
        player.state = Player.State.Active;
        bool building = true;
        int timeToBuild = 1;
        // energy cost per hour
        int energyCostPerHour = 20;
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

        LogFeedback("You Start Building A Fishing Pole.");

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

        LogFeedback("You Finish Building A Fishing Pole.");

        // set time to normal
        hourScale = baseHourScale;
        // reduce player energy
        player.state = Player.State.Rest;
        player.ReduceEnergy(energyCostPerHour * timeToBuild);
    }

    IEnumerator BuildBoat()
    {
        player.Wood -= player.MaxWood;
        player.Cloth -= player.MaxCloth;
        player.Plants -= player.MaxPlants;
        player.state = Player.State.Active;
        bool building = true;
        int timeToBuild = 6;
        // energy cost per hour
        int energyCostPerHour = 50;
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

        LogFeedback("You Start Building A Boat.");

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

        LogFeedback("You Finish Building A Fishing Pole.");

        // set time to normal
        hourScale = baseHourScale;
        // reduce player energy
        player.state = Player.State.Rest;
        player.ReduceEnergy(energyCostPerHour * timeToBuild);
        player.Boat = true;
    }

    IEnumerator Hunt()
    {
        Zone zoneToHunt = selectedZone; ;

        if (zoneToHunt == currentZone)
        {
            bool hunt = true;
            // time to gather
            int timeToHunt = 4;
            // energy cost per hour
            int energyCostPerHour = 20;
            // current hour
            int currentHour = hour;
            // current minutes
            int currentMin = minutes;
            // goal hour
            int goal = currentHour + timeToHunt;
            if (goal >= 24)
            {
                goal -= 24;
            }

            player.state = Player.State.Active;
            // fast forward time
            hourScale = fastHourScale;

            LogFeedback("You Begin Hunting.");

            while (hunt)
            {
                if (hour == goal)
                {
                    if (minutes == currentMin)
                    {
                        hunt = false;
                    }
                }

                yield return null;
            }

            float successfulHunt = Random.value;
            if(successfulHunt > 0.8)
            {
                float breakTool = Random.value;
                if(player.Meat < 0)
                {
                    LogFeedback("You Caught An Animal.");
                    player.CookedFood = false;
                    player.Meat += 1;
                }

                if(breakTool > .7)
                {
                    player.Spear = false;
                    LogFeedback("You Broke Your Spear.");
                }
            }
            else
            {
                LogFeedback("You Caught Nothing.");
            }

            // set time to normal
            hourScale = baseHourScale;
            // reduce player energy
            player.ReduceEnergy(energyCostPerHour * timeToHunt);
            player.state = Player.State.Rest;
        }
    }

    IEnumerator Fish()
    {
        Zone zoneToFish = selectedZone; ;

        if (zoneToFish == currentZone)
        {
            bool fish = true;
            // time to gather
            int timeToFish = 4;
            // energy cost per hour
            int energyCostPerHour = 20;
            // current hour
            int currentHour = hour;
            // current minutes
            int currentMin = minutes;
            // goal hour
            int goal = currentHour + timeToFish;
            if (goal >= 24)
            {
                goal -= 24;
            }

            player.state = Player.State.Active;
            // fast forward time
            hourScale = fastHourScale;

            LogFeedback("You Begin Fishing.");

            while (fish)
            {
                if (hour == goal)
                {
                    if (minutes == currentMin)
                    {
                        fish = false;
                    }
                }

                yield return null;
            }

            float successfulHunt = Random.value;
            if (successfulHunt > 0.8)
            {
                float breakTool = Random.value;
                if (player.Meat < 0)
                {
                    LogFeedback("You Caught A Fish.");
                    player.CookedFood = false;
                    player.Meat += 1;
                }

                if (breakTool > .7)
                {
                    player.FishingPole = false;
                    LogFeedback("You Broke Your Fishing Pole.");
                }
            }
            else
            {
                LogFeedback("You Caught Nothing.");
            }

            // set time to normal
            hourScale = baseHourScale;
            // reduce player energy
            player.ReduceEnergy(energyCostPerHour * timeToFish);
            player.state = Player.State.Rest;
        }
    }

    IEnumerator Excavate()
    {
        Zone zoneToExcavate = selectedZone; ;
       
        if (zoneToExcavate == currentZone)
        {
            bool excavating = true;
            // time to gather
            int timeToExcavate = 5;
            // energy cost per hour
            int energyCostPerHour = zoneToExcavate.ZoneEnergy / 4;
            // current hour
            int currentHour = hour;
            // current minutes
            int currentMin = minutes;
            // goal hour
            int goal = currentHour + timeToExcavate;
            if (goal >= 24)
            {
                goal -= 24;
            }

            player.state = Player.State.Active;
            // fast forward time
            hourScale = fastHourScale;

            LogFeedback("You Begin An Excavation.");

            while (excavating)
            {
                if (hour == goal)
                {
                    if (minutes == currentMin)
                    {
                        excavating = false;
                    }
                }

                yield return null;
            }

            if (zoneToExcavate.HasTheRelic)
            {
                player.Relics += zoneToExcavate.numberOfRelics;
                zoneToExcavate.HasTheRelic = false;
                zoneToExcavate.parentRelicZone.RelicFound();
                LogFeedback("You Found " + zoneToExcavate.numberOfRelics.ToString() + " Relics");
            }
            else
            {
                zoneToExcavate.yellow.SetActive(false);
                zoneToExcavate.inRelicZone = false;
                LogFeedback("You Found Nothing.");
            }
            // set time to normal
            hourScale = baseHourScale;
            // reduce player energy
            player.ReduceEnergy(energyCostPerHour * timeToExcavate);
            player.state = Player.State.Rest;
        }
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
                    hit = Physics2D.Raycast(mousePos, Vector3.zero, Mathf.Infinity, mouseMask);
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
                + "Total Hours: 8 Hours";
        }
        else
        {
            sleepButton.GetComponentInChildren<Text>().text = "Sleep";
        }

        // COLLECT WATER
        waterButton.interactable = player.state == Player.State.Rest && currentZone.Water && player.GallonWater < 1 && state != State.Camp;

        if (waterButton.interactable)
        {
            waterButton.GetComponentInChildren<Text>().text = "Collect Water";
        }
        else
        {
            waterButton.GetComponentInChildren<Text>().text = "Find Water";
        }

        // DRINK WATER
        drinkButton.interactable = player.state == Player.State.Rest && player.GallonWater > 0;

        if (drinkButton.interactable)
        {
            drinkButton.GetComponentInChildren<Text>().text = "Drink Water";
        }
        else
        {
            drinkButton.GetComponentInChildren<Text>().text = "No Water";
        }

        // EAT FOOD
        eatButton.interactable = player.state == Player.State.Rest && player.Meat > 0;

        if (eatButton.interactable)
        {
            eatButton.GetComponentInChildren<Text>().text = "Eat Food";
        }
        else
        {
            eatButton.GetComponentInChildren<Text>().text = "No Food";
        }

        // MEDICINE
        medButton.interactable = player.state == Player.State.Rest && player.Medicinal > 0;

        if (medButton.interactable)
        {
            medButton.GetComponentInChildren<Text>().text = "Take Medicine";
        }
        else
        {
            medButton.GetComponentInChildren<Text>().text = "No Medicine";
        }

        // FISH
        fishButton.interactable = player.state == Player.State.Rest && currentZone.Water && state != State.Camp && player.FishingPole;

        if (fishButton.interactable)
        {
            fishButton.GetComponentInChildren<Text>().text = "Cost to Fish: " + ("50eph\n"
                + "Total Hours: 4 Hours");
        }
        else
        {
            fishButton.GetComponentInChildren<Text>().text = "Can't Fish";
        }

        //HUNT
        huntButton.interactable = player.state == Player.State.Rest && currentZone.Fauna && state != State.Camp && player.Spear;

        if (huntButton.interactable)
        {
            huntButton.GetComponentInChildren<Text>().text = "Cost to Hunt: " + ("50eph\n"
                + "Total Hours: 4 Hours");
        }
        else
        {
            huntButton.GetComponentInChildren<Text>().text = "Can't Hunt";
        }

        // BUILD CAMP
        buildCampButton.interactable = !currentZone.Shelter && player.state == Player.State.Rest && player.CanBuildShelter() && state != State.Camp;
        if (buildCampButton.interactable)
        {
            buildCampButton.GetComponentInChildren<Text>().text = "Cost To Build Camp: 150eph\n Total Hours: 5 Hours";
        }
        else
        {
            buildCampButton.GetComponentInChildren<Text>().text = "Build Camp\n Wood: 12 | Rocks: 8 | Plants: 25";
        }

        // ENTER CAMP
        enterCampButton.interactable = currentZone.Shelter && player.state == Player.State.Rest && state != State.Camp;
        if (enterCampButton.interactable)
        {
            enterCampButton.GetComponentInChildren<Text>().text = "Enter Camp";
        }
        else
        {
            enterCampButton.GetComponentInChildren<Text>().text = "No Camp Here";
        }

        // BUILD SPEAR
        buildSpearButton.interactable = player.state == Player.State.Rest && state == State.Camp && player.CanBuildSpear() && !player.Spear;
        if (buildSpearButton.interactable)
        {
            buildSpearButton.GetComponentInChildren<Text>().text = "Cost to Build: " + ("50eph\n"
                + "Total Hours: 1 Hours");
        }
        else
        {
            buildSpearButton.GetComponentInChildren<Text>().text = "Can't Build Spear";
        }

        // BUILD FISHINGPOLE
        buildFishingPoleButton.interactable = player.state == Player.State.Rest && state == State.Camp && player.CanBuildFishingPole() && !player.FishingPole;
        if (buildFishingPoleButton.interactable)
        {
            buildFishingPoleButton.GetComponentInChildren<Text>().text = "Cost to Build: " + ("50eph\n"
                + "Total Hours: 1 Hours");
        }
        else
        {
            buildFishingPoleButton.GetComponentInChildren<Text>().text = "Can't Build Fishing Pole";
        }

        // BUILD BOAT
        buildBoatButton.interactable = player.state == Player.State.Rest && state == State.Camp && player.CanBuildBoat() && !player.Boat;
        if (buildBoatButton.interactable)
        {
            buildBoatButton.GetComponentInChildren<Text>().text = "Cost to Build: " + ("150eph\n"
                + "Total Hours: 6 Hours");
        }
        else
        {
            buildBoatButton.GetComponentInChildren<Text>().text = "Can't Build Boat";
        }

        // EXCAVATE
        excavateButton.interactable = player.state == Player.State.Rest && state == State.Island && selectedZone == currentZone && currentZone.inRelicZone;
        if (excavateButton.interactable)
        {
            excavateButton.GetComponentInChildren<Text>().text = "Cost to Excavate: " + ("100eph\n"
                + "Total Hours: 5 Hours");
        }
        else
        {
            excavateButton.GetComponentInChildren<Text>().text = "Can't Excavate";
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
            playerRelicText.text = "Relic " + player.Relics.ToString() + "/5";
        }
    }

    public void GameOver(bool b)
    {
        bool win = b;
        StopAllCoroutines();
        startScreen.SetActive(false);
        worldOverlay.SetActive(false);
        campOverlay.SetActive(false);
        if (win)
        {
            Instantiate(beamPrefab, player.transform.position + new Vector3(0, 50, 0), Quaternion.identity);
            player.Ascend();
        }
        else
        {
            StartCoroutine(FadeInDeathScreen());
        }
    }

    IEnumerator FadeInDeathScreen()
    {
        currentLerpTime = 0;
        deathScreen.SetActive(true);
        Image deathScreenImg = deathScreen.GetComponent<Image>();
        while (deathScreenImg.color.a < .5f)
        {
            currentLerpTime += Time.deltaTime;

            float perc = currentLerpTime / 2;
            deathScreenImg.color = Color.Lerp(deathScreenImg.color, deathInColor, perc);
            yield return null;
        }

        deathScreenText.SetActive(true);
        exitButton.gameObject.SetActive(true);
    }

    IEnumerator FadeInStartScreen()
    {
        currentLerpTime = 0;
        startScreen.SetActive(true);
        Image startScreenImg = blackScreen.GetComponent<Image>();
        while (startScreenImg.color.a < 1f)
        {
            currentLerpTime += Time.deltaTime;

            float perc = currentLerpTime / 4;
            startScreenImg.color = Color.Lerp(startScreenImg.color, startInColor, perc);
            yield return null;
        }

        endText.gameObject.SetActive(true);
        endText.text = "You return to your waiting spaceship where your memory quickly returns. After " + day +
        " days on the island, you managed to recover the " + map.numberOfRelics.ToString() +
        " lost relics of your alien civilization. When you return home, you will surely be honored as a hero. Good Job!";
        exitButton.gameObject.SetActive(true);
    }

    public void FinishGame()
    {
        StartCoroutine(FadeInStartScreen());
    }

    public void LoadIntroScene()
    {
        SceneManager.LoadScene(0);
    }

    public void LogFeedback(string message)
    {
        feedbackText.text += System.Environment.NewLine + message;
    }

    void CheckThirst()
    {
        if(player.thirstTimer == 0)
        {
            thirstBar.fillAmount = 0;
        }
        else
        {
            thirstBar.fillAmount = player.thirstTimer / 24;
        }
    }

    void CheckFatigue()
    {
        if(player.fatigueTimer == 0)
        {
            fatigueBar.fillAmount = 0;
        }
        else
        {
            fatigueBar.fillAmount = player.fatigueTimer / 600;
        }
    }
}