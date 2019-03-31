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

    public int energy = 3000;
    public int baseEnergy = 45;
    public int fatigueVal = 30;
    public int totalEnergyLoss;

    private bool treatedWater;

    #region items
    private int meat;
    private int maxMeat = 2;
    private int plants;
    private int maxPlants = 20;
    private int medicinal;
    private int maxMedicinal = 2;
    private int cloth;
    private int maxCloth = 5;
    private int wood;
    private int maxWood = 6;
    private int rocks;
    private int maxRocks = 4;
    private float gallonWater;
    private float maxWater = 1;
    #endregion

    public bool fatigued;

    GameManager gm;

    #region getters/setters

    public int Meat
    {
        get { return meat; }
    }

    public int Medicinal
    {
        get { return medicinal; }
    }

    public int Cloth
    {
        get { return cloth; }
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

    public bool TreatedWater
    {
        set { treatedWater = value; }
        get { return treatedWater; }
    }

    #endregion

    private void Awake()
    {
        gm = FindObjectOfType<GameManager>();

    }

    private void Update()
    {
        if (fatigued)
        {
            totalEnergyLoss = baseEnergy + fatigueVal;
        }
        else
        {
            totalEnergyLoss = baseEnergy;
        }

        gametime = int.Parse(gm.Hour.ToString("00") + gm.Minutes.ToString("00"));

        if (timeToFatigue == gametime)
        {
            fatigued = true;
        }

        if(state == State.Sleep)
        {
            fatigued = false;
        }

    }

    public void ReduceEnergy(int val) 
    {
        energy -= val;
    }

    public void SetSleepTimer()
    {
        string futureTime = HourPlusTen();
        string currentTime = gm.Hour.ToString("00") + gm.Minutes.ToString("00");
        timeAwake = int.Parse(currentTime);
        timeToFatigue = int.Parse(futureTime);
    }

    public void GatherItems(Zone _zone)
    {
        // god this is gross
        for(int i = _zone.Plants; i > 0; i--)
        {
            if(plants < MaxPlants)
            {
                plants += 1;
                _zone.Plants -= 1;
            }
        }

        for (int i = _zone.Cloth; i > 0; i--)
        {
            if(cloth < maxCloth)
            {
                cloth += 1;
                _zone.Cloth -= 1;
            }
        }

        for (int i = _zone.Medicinal; i > 0; i--)
        {
            if(medicinal < maxMedicinal)
            {
                medicinal += 1;
                _zone.Medicinal -= 1;
            }
        }

        for (int i = _zone.Wood; i > 0; i--)
        {
            if(wood < maxWood)
            {
                wood += 1;
                _zone.Wood -= 1;
            }
        }

        for (int i = _zone.Rocks; i > 0; i--)
        {
            if(rocks < maxRocks)
            {
                rocks += 1;
                _zone.Rocks -= 1;
            }
        }
    }

    public void CollectWater()
    {
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

    public bool CanBuildBasicShelter()
    {
        if(wood == maxWood && rocks == maxRocks && plants == maxPlants)
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
                Debug.Log("You Drank Water");
            }
            else
            {
                Debug.Log("You don't feel so well.");
            }
        }
    }
}
