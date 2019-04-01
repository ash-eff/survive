using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone : MonoBehaviour
{
    public LayerMask zoneLayer;

    public enum TerrainType { Water, Beach, Clearing, Forrest, }
    public TerrainType terrainType;

    public enum TerrainSubType { Shallow, Deep, OverGrown, Clear, Standard, Bare, }
    public TerrainSubType terrainSubType;

    public enum ZoneFeature { None, Bog, Lake, Stream, Wreckage, Trash }
    public ZoneFeature zoneFeature;

    public enum Status { Explored, Unexplored, }
    public Status status = Status.Unexplored;

    public int numOfDayUntilReplenish;

    public GameObject outline1;
    public GameObject outline2;
    public GameObject yellow;
    public GameObject relic;
    public Color A;
    public Color B;
    public float speed = 1.0f;

    public int numberOfRelics;

    private SpriteRenderer spr;

    private Vector2 zonePosition; // the position of the zone 
    private Vector2 closestPoint; // current closest point 
    public Zone parentRelicZone;
    private Vector2[] directions = new[] { Vector2.up, Vector2.right, Vector2.down, Vector2.left };
    private List<Zone> relicArea = new List<Zone>();

    private float gradientValue; // the higher this number is, the further from the closest point the zone is
    private float roundedPerlin; // final perlin value, rounded to two decimals;
    private float gradientPerlin;
    private float perlinFloat;
    private float distanceToClosestPoint = 0;

    public int zoneTemperature;
    private int baseEnergy = 50;
    private int zoneEnergy;
    private int increasedMultiplier = 2;
    private float decreasedMultiplier = .5f;

    private bool startingPosition; // is this zone the starting position of the player?
    private bool shelter; // does the player have a shelter here?
    private bool selfPoint; // is this zone one of the points set by the map?
    private bool selected;
    public bool neighbor;
    private bool occupied;
    private bool explored;
    private bool replenished;
    private bool isRelicZone;
    public bool inRelicZone;
    private bool hasTheRelic;
    private bool relicsSet;

    GameManager gm;
    MapManager map;
    BoxCollider2D coll2d;

    #region item bools
    private bool fauna; // meat 
    private bool flora; // plants
    private bool medicine; // medicinal plants
    private bool fabric; // cloth
    private bool lumber; // logs and branches
    private bool water; // water
    private bool rock; // rocks
    private bool fish; // meat
    #endregion

    #region item numbers
    private int plants;
    private int basePlants = 15;
    public int startPlants;
    private int medicinal;
    private int baseMedicinal = 2;
    public int startMedicinal;
    private int cloth;
    private int baseCloth = 1;
    public int startCloth;
    private int wood;
    private int basewood = 5;
    public int startWood;
    private int rocks;
    private int baseRocks = 10;
    public int startRocks;
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
        set
        {
            startingPosition = value; 
            zoneFeature = ZoneFeature.Wreckage;
            lumber = true;
            fabric = true;
            wood = basewood;
            cloth = baseCloth;
            Debug.Log("Starting Position Cloth Count: " + cloth.ToString());
        }
    }

    public bool Shelter
    {
        get { return shelter; }
        set { shelter = value; }
    }

    public int ZoneEnergy
    {
        get { return zoneEnergy; }
    }

    public bool Water
    {
        get { return water; }
    }

    public int Plants
    {
        get { return plants; }
        set { plants = value; }
    }

    public int Medicinal
    {
        get { return medicinal; }
        set { medicinal = value; }
    }

    public int Cloth
    {
        get { return cloth; }
        set { cloth = value; }
    }

    public int Wood
    {
        get { return wood; }
        set { wood = value; }
    }

    public int Rocks
    {
        get { return rocks; }
        set { rocks = value; }
    }

    public bool Selected
    {
        set { selected = value; }
    }

    public bool Explored
    {
        get { return explored; }
        set { explored = value; }
    }

    public bool Neighbor
    {
        get { return neighbor; }
        set { neighbor = value; }
    }

    public bool Occupied
    {
        get { return occupied; }
        set { occupied = value; }
    }

    public bool Flora
    {
        get { return flora; }
    }

    public bool Fauna
    {
        get { return fauna; }
    }

    public bool HasTheRelic
    {
        get { return hasTheRelic; }
        set { hasTheRelic = false; }
    }

    #endregion

    private void OnEnable()
    {
        coll2d = FindObjectOfType<BoxCollider2D>();
        spr = GetComponent<SpriteRenderer>();
        gm = FindObjectOfType<GameManager>();
    }

    private void Update()
    {
        zoneTemperature = GetZoneTemperature(gm.IslandTemperature);

        if (!gm.GameStarted)
        {
            return;
        }

        if (startingPosition)
        {
            explored = true;
        }

        if (explored)
        {
            status = Status.Explored;
        }

        if (gm.player.Boat)
        {
            if(terrainType == TerrainType.Water)
            {
                zoneEnergy = 50;
            }
        }

        if(isRelicZone && gm.GameStarted && !relicsSet)
        {
            relicsSet = true;
            PlaceRelic();
        }

        if (inRelicZone)
        {
            yellow.GetComponent<SpriteRenderer>().color = Color.Lerp(A, B, Mathf.PingPong(Time.time * speed, 1.0f));
        }

        outline1.SetActive(neighbor);
        outline2.SetActive(selected);

        ReplenishZone();
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
            }
            else
            {
                terrainSubType = TerrainSubType.Shallow;
                zoneEnergy = GetEnergyCost(0);
            }
        }

        // beach
        if(terrainType == TerrainType.Beach)
        {
            zoneEnergy = GetEnergyCost(0);
            rock = true;
            water = true;
            fish = true;
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

            float chance = Random.value;
            if (chance > 0.7)
            {
                terrainSubType = TerrainSubType.Bare;
            }
            else
            {
                terrainSubType = TerrainSubType.Clear;
            }

            SetZoneFeature();
        }

        // forrest
        if (terrainType == TerrainType.Forrest)
        {
            float chance = Random.value;
            lumber = true;
            if (chance > 0.5)
            {
                terrainSubType = TerrainSubType.Standard;
                wood += basewood;
                zoneEnergy = GetEnergyCost(.5f);
            }
            else
            {
                terrainSubType = TerrainSubType.OverGrown;
                plants += basePlants * increasedMultiplier;
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
                fabric = true;
                wood += basewood;
                cloth += baseCloth;
            }
            else if (featChance < 0.1)
            {
                zoneFeature = ZoneFeature.Trash;
                fabric = true;
                cloth += baseCloth * 2;
            }
        }

        // clearing
        if (terrainType == TerrainType.Clearing)
        {
            plants += basePlants;
            flora = true;
            if (featChance > 0.80)
            {
                float waterType = Random.value;
                if (waterType > .5)
                {
                    zoneFeature = ZoneFeature.Lake;
                    fish = true;
                    water = true;
                    
                    return;
                }
                else
                {
                    zoneFeature = ZoneFeature.Stream;
                    water = true;
                }
            }
        }

        // forrest
        if (terrainType == TerrainType.Forrest)
        {
            flora = true;
            plants += basePlants;
            if (featChance > 0.80)
            {
                float waterType = Random.value;
                if (waterType > .90)
                {
                    zoneFeature = ZoneFeature.Bog;
                    medicinal += baseMedicinal * increasedMultiplier;
                    plants += Mathf.RoundToInt(basePlants * decreasedMultiplier);
                    return;
                }
                else if (waterType > .45)
                {
                    zoneFeature = ZoneFeature.Lake;
                    fish = true;
                    water = true;
                    plants += basePlants;
                    return;
                }
                else
                {
                    zoneFeature = ZoneFeature.Stream;
                    plants += basePlants;
                    water = true;
                }
            }
        }

        SetStartItems();
    }

    public void SetStartItems()
    {
        startPlants = plants;
        startCloth = cloth;
        startMedicinal = medicinal;
        startWood = wood;
        startRocks = rocks;
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

    public void SetNeighbors()
    {
        foreach (Vector2 dir in directions)
        {
            coll2d.enabled = false;
            RaycastHit2D hit = Physics2D.Raycast(ZonePosition, dir * 2);
            if (hit)
            {
                gm.neighbors.Add(hit.transform.GetComponent<Zone>());
                hit.transform.GetComponent<Zone>().Neighbor = true;
            }
            coll2d.enabled = true;
        }
    }

    public void ClearNeighbors()
    {
        foreach (Vector2 dir in directions)
        {
            gm.neighbors.Clear();
            coll2d.enabled = false;
            RaycastHit2D hit = Physics2D.Raycast(ZonePosition, dir * 2);
            if (hit)
            {
                hit.transform.GetComponent<Zone>().Neighbor = false;
            }
            coll2d.enabled = true;
        }
    }

    void ReplenishZone()
    {

        // this needs to only replenish once when it does replenish
        int currentDay = gm.Day;
        if(currentDay % numOfDayUntilReplenish == 0 && !replenished)
        {
            if(Random.value > .4 && terrainType != TerrainType.Water)
            {
                fauna = true;
            }
            else
            {
                fauna = false;
            }
            medicinal = startMedicinal;
            cloth = startCloth;
            wood = startWood;
            rocks = startRocks;
            replenished = true;
        }

        if(currentDay % numOfDayUntilReplenish != 0)
        {
            if (Random.value > .4 && terrainType != TerrainType.Water)
            {
                fauna = true;
            }
            else
            {
                fauna = false;
            }
            replenished = false;
        }
    }

    public void SetRelicZone()
    {
        isRelicZone = true;
        parentRelicZone = this;
        relicArea.Add(this);
        inRelicZone = true;
        yellow.SetActive(true);
        Collider2D[] hit = Physics2D.OverlapCircleAll(transform.position, 5f, zoneLayer);
        foreach(Collider2D c in hit)
        {
            Zone _zone = c.GetComponent<Zone>();

            if (!_zone.isRelicZone && _zone.terrainType != Zone.TerrainType.Water)
            {
                relicArea.Add(_zone);
                _zone.SetInRelicZone(this);
            }
        }
    }

    public void SetInRelicZone(Zone _parent)
    {
        parentRelicZone = _parent;
        inRelicZone = true;
        yellow.SetActive(true);
    }

    public void RelicFound()
    {
        foreach(Zone z in relicArea)
        {
            if(z.parentRelicZone == this)
            {
                z.yellow.SetActive(false);
            }
        }
    }

    public void PlaceRelic()
    {      
        int randIndex = Random.Range(0, relicArea.Count);
        relicArea[randIndex].hasTheRelic = true;
        //relicArea[randIndex].relic.SetActive(true);
        relicArea[randIndex].numberOfRelics++;
    }
}