using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone : MonoBehaviour
{
    public enum TerrainType { Water, Beach, Clearing, Forrest, }
    public TerrainType terrainType;

    public enum TerrainSubType { Shallow, Deep, OverGrown, Clear, Standard, Bare, }
    public TerrainSubType terrainSubType;

    public enum ZoneFeature { None, Bog, Lake, Stream, Wreckage, }
    public ZoneFeature zoneFeature;

    private SpriteRenderer spr;

    private Vector2 zonePosition; // the position of the zone 
    private Vector2 closestPoint; // current closest point 
    
    private float gradientValue; // the higher this number is, the further from the closest point the zone is
    private float roundedPerlin; // final perlin value, rounded to two decimals;
    private float gradientPerlin;
    private float perlinFloat;
    private float distanceToClosestPoint = 0;

    public int zoneTemperature;
    private int baseEnergy = 100;
    public int zoneEnergy;
    private int increasedMultiplier = 2;
    private float decreasedMultiplier = .5f;

    private bool startingPosition; // is this zone the starting position of the player?
    private bool mainShelter; // is the player's main shelter in this zone?
    private bool tempShelter; // does the player have a temporary shelter here?
    private bool selfPoint; // is this zone one of the points set by the map?

    GameManager gm;

    #region item bools
    private bool flora; // plants and seeds
    private bool fauna; // meat and fur
    private bool medicine; // medicinal plants
    private bool fabric; // cloth
    private bool scrap; // plastic and metal
    private bool lumber; // logs and branches
    private bool water; // water
    private bool fish; // meat
    private bool rock; // rocks
    #endregion

    #region item numbers
    private int plants;
    private int basePlants = 10;
    private int seeds;
    private int baseSeeds = 5;
    private int meat;
    private int baseMeat = 2;
    private int fur;
    private int baseFur = 1;
    private int medicinal;
    private int baseMedicinal = 2;
    private int cloth;
    private int baseCloth = 1;
    private int plastic;
    private int basePlastic = 1;
    private int metal;
    private int baseMetal = 1;
    private int wood;
    private int basewood = 5;
    private int rocks;
    private int baseRocks = 10;
    #endregion

    #region getters and setters
    public Vector2 ZonePosition
    {
        get { return zonePosition; }
        set { zonePosition = value; }
    }

    public float RoundedPerlin
    {
        get { return roundedPerlin; }
    }

    public bool StartingPosition
    {
        set { startingPosition = value; zoneFeature = ZoneFeature.Wreckage;
            lumber = true;
            scrap = true;
            fabric = true;
            wood += basewood;
            plastic += basePlastic;
            metal += baseMetal;
            cloth += baseCloth;
        }
    }

    public bool MainShelter
    {
        get { return mainShelter; }
        set { mainShelter = value; }
    }

    public bool TempShelter
    {
        get { return tempShelter; }
        set { tempShelter = value; }
    }

    public int ZoneEnergy
    {
        get { return zoneEnergy; }
    }
    public int Plants
    {
        get { return plants; }
    }

    public int Seeds
    {
        get { return seeds; }
    }

    public int Meat
    {
        get { return meat; }
    }

    public int Fur
    {
        get { return fur; }
    }

    public int Medicinal
    {
        get { return medicinal; }
    }

    public int Cloth
    {
        get { return cloth; }
    }

    public int Plastic
    {
        get { return plastic; }
    }

    public int Metal
    {
        get { return metal; }
    }

    public int Wood
    {
        get { return wood; }
    }

    public int Rocks
    {
        get { return rocks; }
    }

    #endregion

    private void OnEnable()
    {
        spr = GetComponent<SpriteRenderer>();
        gm = FindObjectOfType<GameManager>();
    }

    private void Update()
    {
        zoneTemperature = GetZoneTemperature(gm.IslandTemperature);
    }

    public void CalculateClosestPoint(Vector2 _point)
    {
        if (zonePosition == _point)
        {
            selfPoint = true;
            closestPoint = _point;
            distanceToClosestPoint = 0;
            return;
        }

        float tempDist = (_point - new Vector2(transform.position.x, transform.position.y)).magnitude;

        if (tempDist < distanceToClosestPoint || distanceToClosestPoint == 0)
        {
            if (!selfPoint)
            {
                distanceToClosestPoint = tempDist;
                closestPoint = _point;
            }
        }
    }

    public void CalculateGradientValue()
    {
        float tempDist = (closestPoint - zonePosition).magnitude / 10;
        gradientValue = tempDist;
    }

    public void CalculatePerlinValue(float _perlin)
    {
        perlinFloat = _perlin;
        gradientPerlin = perlinFloat - gradientValue / 2;
    }

    public void ActivateSprite()
    {
        roundedPerlin = Mathf.Round(gradientPerlin * 100) / 100f;
        if (Mathf.Sign(roundedPerlin) == -1)
        {
            roundedPerlin = .03f;
        }
        
        // green area
        if(roundedPerlin > 0.15)
        {
            spr.color = new Color(0, roundedPerlin, 0);
        }
        // yellow/brown area
        else if(roundedPerlin < 0.15 && roundedPerlin > 0.10)
        {
            spr.color = new Color(roundedPerlin * 2, roundedPerlin * 2, 0);
        }
        else
        {
            // all other water
            if (roundedPerlin <= .03f)
            {
                roundedPerlin = .03f;
                spr.color = new Color(0, roundedPerlin * 2, roundedPerlin * 8);
            }
            // edges of the island and water
            else
            {
                spr.color = new Color(0, roundedPerlin * 2, roundedPerlin * 5);
            }
        }

        SetZoneType();
    }

    private void SetZoneType()
    {
        if(roundedPerlin <= .09f)
        {
            terrainType = TerrainType.Water;
        }
        else if(roundedPerlin > .09f && roundedPerlin <= .14f)
        {
            terrainType = TerrainType.Beach;
        }
        else
        {
            if(Random.value > 0.8)
            {
                terrainType = TerrainType.Clearing;
            }
            else
            {
                terrainType = TerrainType.Forrest;
            }
        }

        SetZoneSubType();
    }

    private void SetZoneSubType()
    {
        // water
        if(terrainType == TerrainType.Water)
        {
            fish = true;
            water = true;
            if (roundedPerlin <= .03f)
            {
                terrainSubType = TerrainSubType.Deep;
                zoneEnergy = GetEnergyCost(0);
                meat += baseMeat * increasedMultiplier;
            }
            else
            {
                terrainSubType = TerrainSubType.Shallow;
                zoneEnergy = GetEnergyCost(0);
                meat += baseMeat;
            }
        }

        // beach
        if(terrainType == TerrainType.Beach)
        {
            zoneEnergy = GetEnergyCost(0);
            rock = true;
            float chance = Random.value;

            if (chance > 0.7)
            {
                terrainSubType = TerrainSubType.Bare;
                rocks += Mathf.RoundToInt(baseRocks * decreasedMultiplier);
            }
            else if(chance < 0.7 && chance > 0.4)
            {
                terrainSubType = TerrainSubType.Standard;
                rocks += baseRocks;
            }
            else
            {
                terrainSubType = TerrainSubType.Clear;
                rocks += baseRocks;
            }

            SetZoneFeature();
        }

        // clearing
        if(terrainType == TerrainType.Clearing)
        {
            zoneEnergy = GetEnergyCost(0);
            flora = true;
            fauna = true;

            float chance = Random.value;
            if (chance > 0.7)
            {
                terrainSubType = TerrainSubType.Bare;
                meat += Mathf.RoundToInt(baseMeat * decreasedMultiplier);
                fur += Mathf.RoundToInt(baseFur * decreasedMultiplier);
                plants += Mathf.RoundToInt(basePlants * decreasedMultiplier);
                seeds += Mathf.RoundToInt(baseSeeds * decreasedMultiplier);
            }
            else
            {
                terrainSubType = TerrainSubType.Clear;
                meat += baseMeat;
                fur += baseFur;
                plants += basePlants;
                seeds += baseSeeds;
            }

            SetZoneFeature();
        }

        // forrest
        if (terrainType == TerrainType.Forrest)
        {
            float chance = Random.value;
            flora = true;
            fauna = true;
            lumber = true;
            if (chance > 0.5)
            {
                terrainSubType = TerrainSubType.Standard;
                meat += baseMeat;
                fur += baseFur;
                plants += basePlants;
                seeds += baseSeeds;
                wood += basewood;
                zoneEnergy = GetEnergyCost(.5f);
            }
            else
            {
                terrainSubType = TerrainSubType.OverGrown;
                meat += baseMeat * increasedMultiplier;
                fur += baseFur * increasedMultiplier;
                plants += Mathf.RoundToInt(basePlants * decreasedMultiplier);
                seeds += Mathf.RoundToInt(baseSeeds * decreasedMultiplier);
                wood += basewood * increasedMultiplier;
                zoneEnergy = GetEnergyCost(1);
            }

            SetZoneFeature();
        }
    }

    private void SetZoneFeature()
    {
        float featChance = Random.value;

        // beach
        if (terrainType == TerrainType.Beach)
        {
            if (featChance > 0.95)
            {
                zoneFeature = ZoneFeature.Wreckage;
                lumber = true;
                scrap = true;
                fabric = true;
                wood += basewood;
                plastic += basePlastic;
                metal += baseMetal;
                cloth += baseCloth;
            }
        }

        // clearing
        if (terrainType == TerrainType.Clearing)
        {
            if (featChance > 0.80)
            {
                float waterType = Random.value;
                if (waterType > .5)
                {
                    zoneFeature = ZoneFeature.Lake;
                    fish = true;
                    water = true;
                    meat += baseMeat;
                    meat += 1; // one extra fauna
                    fur += 1; // one extra fauna
                    return;
                }
                else
                {
                    zoneFeature = ZoneFeature.Stream;
                    meat += 1; // one extra fauna
                    fur += 1; // one extra fauna
                    water = true;
                }
            }
        }

        // forrest
        if (terrainType == TerrainType.Forrest)
        {
            if (featChance > 0.80)
            {
                float waterType = Random.value;
                if (waterType > .90)
                {
                    zoneFeature = ZoneFeature.Bog;
                    medicinal += baseMedicinal * increasedMultiplier;
                    return;
                }
                else if (waterType > .45)
                {
                    zoneFeature = ZoneFeature.Lake;
                    fish = true;
                    water = true;
                    meat += baseMeat;
                    meat += 1; // one extra fauna
                    fur += 1; // one extra fauna
                    return;
                }
                else
                {
                    zoneFeature = ZoneFeature.Stream;
                    meat += 1; // one extra fauna
                    fur += 1; // one extra fauna
                    water = true;
                }
            }
        }
    }

    private int GetEnergyCost(float subtypeMod)
    {
        float energyMultiplier = 0;
        if (roundedPerlin <= .03f)
        {
            energyMultiplier = 6;
        }
        else if (roundedPerlin > .03f && roundedPerlin <= .09f)
        {
            energyMultiplier = 3;
        }
        else if (roundedPerlin > .09f && roundedPerlin <= .24f)
        {
            energyMultiplier = 1;
        }
        else if (roundedPerlin > .24f && roundedPerlin <= .40f)
        {
            energyMultiplier = 2;
        }
        else if (roundedPerlin > .40f)
        {
            energyMultiplier = 3;
        }

        energyMultiplier += subtypeMod;

        return Mathf.RoundToInt(baseEnergy * energyMultiplier);
    }

    private int GetZoneTemperature(int islandTemp)
    {
        int temp = 0;

        if(roundedPerlin < .09f)
        {
            temp = islandTemp - 2;
        }
        else if (roundedPerlin > .09f && roundedPerlin <= .24f)
        {
            temp = islandTemp;
        }
        else if (roundedPerlin > .24f && roundedPerlin <= .40f)
        {
            temp = islandTemp - 3;
        }
        else if (roundedPerlin > .40f)
        {
            temp = islandTemp - 4;
        }

        return temp;
    }

    // coroutine to replenish items over time
}