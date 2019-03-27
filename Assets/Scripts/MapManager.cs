using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public Zone zone;
    public GameObject startingPoint;

    public int maxPoints; // number of points. helps spread the island out more randomly
    public int maxMapSizeX;
    public int maxMapSizeY;
    public int scale;

    public int xOffset; // adds more randomness to the perlin noise
    public int yOffset;
    public int minXPointRange; // how far from center each point can reach
    public int maxXPointRange;
    public int minYPointRange;
    public int maxYPointRange;

    private float width;
    private float height;

    private Dictionary<Zone, Vector2> zones = new Dictionary<Zone, Vector2>();
    private List<Vector2> gridPos = new List<Vector2>();
    private List<Zone> startingPos = new List<Zone>();
    private List<Vector2> points = new List<Vector2>();

    GameManager gm;

    void Awake()
    {
        gm = FindObjectOfType<GameManager>();
        minXPointRange = maxMapSizeX / 4;
        maxXPointRange = maxMapSizeX / 4;
        minYPointRange = maxMapSizeY / 4;
        maxYPointRange = maxMapSizeY / 4;
        width = zone.transform.localScale.x;
        height = zone.transform.localScale.y;
        xOffset = Random.Range(0, 99999);
        yOffset = Random.Range(0, 99999);
        BuildGrid();
    }

    private void BuildGrid()
    {
        for(int i = 0; i < maxMapSizeX; i++)
        {
            for(int j = 0; j < maxMapSizeY; j++)
            {
                float xPos = (i - maxMapSizeX / 2) * width;
                float yPos = (j - maxMapSizeY / 2) * height;
                gridPos.Add(new Vector2(xPos, yPos));
            }
        }

        PopulateGrid();
    }

    private void PopulateGrid()
    {
        foreach(Vector2 pos in gridPos)
        {
            GameObject zoneParent = GameObject.Find("Zones");
            Zone zoneObj = Instantiate(zone, pos, Quaternion.identity);
            zoneObj.ZonePosition = pos;
            zoneObj.transform.parent = zoneParent.transform;
            zones.Add(zoneObj, pos);      
        }

        SetPoints();
    }

    public void SetPoints()
    {
        for (int i = 0; i < maxPoints; i++)
        {
            int randomX = Random.Range(-minXPointRange, maxXPointRange);
            int randomY = Random.Range(-minYPointRange, maxYPointRange);

            foreach (KeyValuePair<Zone, Vector2> z in zones)
            {
                if (z.Value == new Vector2(randomX, randomY))
                {
                    points.Add(z.Value);
                }
            }
        }

        AssignClosestPoint();
    }

    private void AssignClosestPoint()
    {
        foreach(Vector2 point in points)
        {
            foreach (KeyValuePair<Zone, Vector2> z in zones)
            {
                z.Key.CalculateClosestPoint(point);
            }
        }
        
        foreach(KeyValuePair<Zone, Vector2> z in zones)
        {
            z.Key.CalculateGradientValue();
        }

        CreateMap();
    }

    private void CreateMap()
    {
        foreach(KeyValuePair<Zone, Vector2> z in zones)
        {
            float perlin = Mathf.PerlinNoise(z.Value.x / scale + xOffset, z.Value.y / scale + yOffset);
            z.Key.CalculatePerlinValue(perlin);
            z.Key.ActivateSprite();
            if (z.Key.RoundedPerlin > .09f && z.Key.RoundedPerlin < .15f)
            {
                startingPos.Add(z.Key);
            }
        }

        int startingIndex = Random.Range(0, startingPos.Count);
        gm.InstantiatePlayer(startingPos[startingIndex]);
        startingPos[startingIndex].StartingPosition = true;
    }
}
