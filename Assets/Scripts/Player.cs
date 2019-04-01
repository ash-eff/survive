using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player : MonoBehaviour
{
    public enum State { Rest, Active, Sleep }
    public State state = State.Rest;

    public Vector2 currentPosition;
    public int timeAwake;
    public int timeToFatigue;
    public int gametime;

    public int energy = 4000;
    public int baseEnergy = 22;
    public int fatigueVal = 0;
    public int fatigueBaseVal = 15;
    public int sickVal = 0;
    public int sickBaseVal = 20;
    public int totalEnergyLoss;

    private bool treatedWater;
    private bool cookedFood = true;
    private bool refinedMeds;
    private bool spear;
    private bool fishingPole;
    private bool boat;

    public float fatigueTimer;
    public float thirstTimer;

    #region items
    private int meat = 2;
    private int maxMeat = 2;
    private int plants;
    private int maxPlants = 40;
    private int medicinal;
    private int maxMedicinal = 2;
    private int cloth;
    private int maxCloth = 5;
    private int wood;
    private int maxWood = 15;
    private int rocks;
    private int maxRocks = 10;
    private float gallonWater;
    private float maxWater = 1;
    public int relics;
    #endregion

    private bool fatigued;
    private bool sick;

    GameManager gm;

    #region getters/setters

    public int Meat
    {
        get { return meat; }
        set { meat = value; }
    }

    public int Medicinal
    {
        get { return medicinal; }
    }

    public int Cloth
    {
        get { return cloth; }
        set { cloth = value; }
    }

    public int Wood
    {
        set { wood = value; }
        get { return wood; }
    }

    public int Rocks
    {
        set { rocks = value; }
        get { return rocks; }
    }

    public float GallonWater
    {
        get { return gallonWater; }
    }

    public int MaxMeat
    {
        get { return maxMeat; }
    }

    public int MaxMedicinal
    {
        get { return maxMedicinal; }
    }

    public int MaxCloth
    {
        get { return maxCloth; }
    }

    public int MaxWood
    {
        get { return maxWood; }
    }

    public int MaxRocks
    {
        get { return maxRocks; }
    }

    public int Plants
    {
        set { plants = value; }
        get { return plants; }
    }

    public int MaxPlants
    {
        get { return maxPlants; }
    }

    public float MaxWater
    {
        get { return maxWater; }
    }

    public int Relics
    {
        get { return relics; }
        set { relics = value; }
    }

    public bool TreatedWater
    {
        set { treatedWater = value; }
        get { return treatedWater; }
    }

    public bool CookedFood
    {
        set { cookedFood = value; }
        get { return cookedFood; }
    }

    public bool RefinedMeds
    {
        set { refinedMeds = value; }
        get { return refinedMeds; }
    }

    public bool Spear
    {
        get { return spear; }
        set { spear = value; }
    }

    public bool FishingPole
    {
        get { return fishingPole; }
        set { fishingPole = value; }
    }

    public bool Boat
    {
        get { return boat; }
        set { boat = value; }
    }

    #endregion

    private void Awake()
    {
        gm = FindObjectOfType<GameManager>();

    }

    private void Update()
    {
        if (gm.gameOver) { return; }

        if (fatigued)
        {
            fatigueVal = fatigueBaseVal;
        }
        else
        {
            fatigueVal = 0;
        }

        if (sick)
        {
            sickVal = sickBaseVal;
        }
        else
        {
            sickVal = 0;
        }

        totalEnergyLoss = baseEnergy + fatigueVal + sickVal;
        gametime = int.Parse(gm.Hour.ToString("00") + gm.Minutes.ToString("00"));

        if (timeToFatigue == gametime && !fatigued)
        {
            fatigued = true;
            gm.LogFeedback("You're Fatigued");
            ReduceEnergy(100);
        }

        if(state == State.Sleep)
        {
            fatigued = false;
        }

        if(meat == 0)
        {
            cookedFood = false;
        }

        if(medicinal == 0)
        {
            refinedMeds = false;
        }

        if(gallonWater == 0)
        {
            treatedWater = false;
        }
    }

    public void ReduceEnergy(int val) 
    {
        if(state != State.Sleep)
        {
            energy -= val;
        }

        //gm.LogFeedback("You Lost " + val + " Energy.");
    }

    public void IncreaseEnergy(int val)
    {
        energy += val;
        //gm.LogFeedback("You Gained " + val + " Energy.");
    }

    public void SetSleepTimer()
    {
        string futureTime = HourPlusTen();
        string currentTime = gm.Hour.ToString("00") + gm.Minutes.ToString("00");     
        timeAwake = int.Parse(currentTime);
        timeToFatigue = int.Parse(futureTime);
        fatigueTimer = 0;
    }

    public void GatherItems(Zone _zone)
    {
        int totalPlants = 0;
        int totalWood = 0;
        int totalCloth = 0;
        int totalMeds = 0;
        int totalRocks = 0;

        // god this is gross
        for(int i = _zone.Plants; i > 0; i--)
        {
            if(plants < MaxPlants)
            {
                totalPlants++;
                plants += 1;
                _zone.Plants -= 1;
            }
        }

        for (int i = _zone.Cloth; i > 0; i--)
        {
            if(cloth < maxCloth)
            {
                totalCloth++;
                cloth += 1;
                _zone.Cloth -= 1;
            }
        }

        for (int i = _zone.Medicinal; i > 0; i--)
        {
            if(medicinal < maxMedicinal)
            {
                totalMeds++;
                medicinal += 1;
                _zone.Medicinal -= 1;
            }
        }

        for (int i = _zone.Wood; i > 0; i--)
        {
            if(wood < maxWood)
            {
                totalWood++;
                wood += 1;
                _zone.Wood -= 1;
            }
        }

        for (int i = _zone.Rocks; i > 0; i--)
        {
            if(rocks < maxRocks)
            {
                totalRocks++;
                rocks += 1;
                _zone.Rocks -= 1;
            }
        }

        if(totalCloth > 0)
        {
            gm.LogFeedback("You Gathered " + totalCloth + " Cloth.");
        }
        if (totalWood > 0)
        {
            gm.LogFeedback("You Gathered " + totalWood + " Wood.");
        }
        if (totalRocks > 0)
        {
            gm.LogFeedback("You Gathered " + totalRocks + " Rocks.");
        }
        if (totalPlants > 0)
        {
            gm.LogFeedback("You Gathered " + totalPlants + " Plants.");
        }
        if (totalMeds > 0)
        {
            gm.LogFeedback("You Gathered " + totalMeds + " Medicinal Plants.");
        }
    }

    public void CollectWater()
    {
        gm.LogFeedback("You Collect Water.");
        treatedWater = false;
        gallonWater = maxWater;
    }

    string HourPlusTen()
    {
        int hourAndTen = gm.Hour + 10;
        string futureString = "";
        if (hourAndTen >= 24)
        {
            hourAndTen -= 24;
        }

        futureString = hourAndTen.ToString("00") + gm.Minutes.ToString("00");

        return futureString;
    }

    public bool CanBuildShelter()
    {
        if(wood >= 12 && rocks >= 8 && plants >= 25)
        {
            return true;
        }
        else
        {
          return false;
        }
    }

    public bool CanBuildSpear()
    {
        if (wood >= 2 && rocks >= 1 && plants >= 10)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CanBuildFishingPole()
    {
        if (wood >= 2 && cloth >= 1 && plants >= 10)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CanBuildBoat()
    {
        if (wood >= 12 && cloth >= 5 && plants >= 25)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void DrinkWater()
    {
        if(gallonWater > 0)
        {
            gallonWater -= .25f;

            if (TreatedWater)
            {
                gm.LogFeedback("You Drink Water.");
                thirstTimer = 0;
            }
            else
            {
                gm.LogFeedback("You Don't Feel So Well.");
                sick = true;
                ReduceEnergy(100);
            }
        }
    }

    public void EatFood()
    {
        if (meat > 0)
        {
            meat -= 1;

            if (cookedFood)
            {
                gm.LogFeedback("You Eat Food.");
                IncreaseEnergy(600);
            }
            else
            {
                gm.LogFeedback("You Don't Feel So Well.");
                sick = true;
                ReduceEnergy(100);
            }
        }
    }

    public void TakeMeds()
    {
        if (medicinal > 0)
        {
            medicinal -= 1;

            if (refinedMeds)
            {
                gm.LogFeedback("You Feel Better.");
                sick = false;
            }
            else
            {
                gm.LogFeedback("Nothing Happened.");
            }
        }
    }

    public void Ascend()
    {
        StartCoroutine(AscendCo());
    }

    IEnumerator AscendCo()
    {
        float timer = 8;
        bool called = false;
        Vector3 upPos = transform.position + new Vector3(0, 100, 0);
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, upPos, 10 * Time.deltaTime);

            yield return null;

            if(timer < 4 && !called)
            {
                called = true;
                gm.FinishGame();
            }
        }
    }
}
