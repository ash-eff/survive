using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone : MonoBehaviour
{
    // the position of the zone 
    public Vector2 zonePosition;

    // current closest point
    public Vector2 closestPoint;

    // the higher this number is, the further from closest point the zone is
    public float distanceToClosestPoint;

    // the higher this number is, the higher the elevation of the zone
    public float perlinFloat;
    
    public float gradientValue;

    public float finalPerlinValue;

    public float roundedPerlin;

    public bool startingPosition;

    private SpriteRenderer spr;
    private bool selfPoint;

    private void Awake()
    {
        spr = GetComponent<SpriteRenderer>();
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

    public void SetGradientValue()
    {
        float tempDist = (closestPoint - zonePosition).magnitude / 10;
        gradientValue = tempDist;
    }

    public void AssignPerlinValue(float _perlin)
    {
        perlinFloat = _perlin;

        finalPerlinValue = perlinFloat - gradientValue / 2; 
    }

    public void ActivateSprite()
    {
        roundedPerlin = Mathf.Round(finalPerlinValue * 100) / 100f;
        //spr.color = new Color(roundedPerlin, roundedPerlin, roundedPerlin);
        //spr.color = new Color(finalPerlinValue, finalPerlinValue, finalPerlinValue);
        if (Mathf.Sign(roundedPerlin) == -1)
        {
            Debug.Log(Mathf.Sign(roundedPerlin));
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
            if (roundedPerlin < .03f)
            {
                spr.color = new Color(0, .05f * 2, .05f * 8);
            }
            // edges of the island and water
            else
            {
                spr.color = new Color(0, roundedPerlin * 2, roundedPerlin * 5);
            }
        }
    }
}