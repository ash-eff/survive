using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public Zone zone;
    public GameObject startingPoint;
    public int maxPoints;
    public int minXPointRange;
    public int maxXPointRange;
    public int minYPointRange;
    public int maxYPointRange;
    public int maxX;
    public int maxY;
    public int xOffset;
    public int yOffset;
    public int scale;

    private List<Vector2> gridPos = new List<Vector2>();
    private Dictionary<Zone, Vector2> zones = new Dictionary<Zone, Vector2>();
    public List<Zone> startingPos = new List<Zone>();
    public List<Vector2> points = new List<Vector2>();
    private float width;
    private float height;
    private float maxDistance;

    void Awake()
    {
        minXPointRange = maxX / 4;
        maxXPointRange = maxX / 4;
        minYPointRange = maxY / 4;
        maxYPointRange = maxY / 4;
        width = zone.transform.localScale.x;
        height = zone.transform.localScale.y;
        maxDistance = (Vector2.zero - new Vector2(maxX / 2 * width, maxY / 2 *height)).magnitude;
        xOffset = Random.Range(0, 99999);
        yOffset = Random.Range(0, 99999);
        BuildGrid();
    }

    private void BuildGrid()
    {
        for(int i = 0; i < maxX; i++)
        {
            for(int j = 0; j < maxY; j++)
            {
                float xPos = (i - maxX / 2) * width;
                float yPos = (j - maxY / 2) * height;
                gridPos.Add(new Vector2(xPos, yPos));
            }
        }

        PopulateGrid();
    }

    private void PopulateGrid()
    {
        foreach(Vector2 pos in gridPos)
        {
            Zone zoneObj = Instantiate(zone, pos, Quaternion.identity);
            zoneObj.zonePosition = pos;    
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
            z.Key.SetGradientValue();
        }

        CreateMap();
    }

    private void CreateMap()
    {
        foreach(KeyValuePair<Zone, Vector2> z in zones)
        {
            float perlin = Mathf.PerlinNoise(z.Value.x / scale + xOffset, z.Value.y / scale + yOffset);
            z.Key.AssignPerlinValue(perlin);
            z.Key.ActivateSprite();
            if (z.Key.roundedPerlin > .09f && z.Key.roundedPerlin < .15f)
            {
                startingPos.Add(z.Key);
            }
        }

        int startingIndex = Random.Range(0, startingPos.Count);
        Instantiate(startingPoint, startingPos[startingIndex].zonePosition, Quaternion.identity);
        startingPos[startingIndex].startingPosition = true;

        //for (float i = 0; i < maxX; i++)
        //{
        //    for (float j = 0; j < maxY; j++)
        //    {
        //        float xPos = (i - maxX / 2) * width;
        //        float yPos = (j - maxY / 2) * height;
        //        float distance = (Vector2.zero - new Vector2(xPos, yPos)).magnitude;
        //        distance = Mathf.Clamp(distance, 0, 1);
        //        float perlin = Mathf.PerlinNoise(i / scale, j / scale);
        //
        //        Zone zoneObj = Instantiate(zone, new Vector3(xPos, yPos, 0), Quaternion.identity);
        //        zoneObj.perlinFloat = perlin;
        //        zoneObj.distanceToCenter = distance;
        //        //SpriteRenderer spr = zoneObj.GetComponent<SpriteRenderer>();
        //        //
        //        //if(perlin <= 0.3f)
        //        //{
        //        //    spr.color = new Color(0, 0, perlin);
        //        //}
        //        //else
        //        //{
        //        //    spr.color = new Color(0, perlin, 0);
        //        //}
        //    }
        //}
    }
}
