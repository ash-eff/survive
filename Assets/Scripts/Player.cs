using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public bool fatigued;

    GameManager gm;

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

    public void GetFatigueTimer(int n)
    {
        int hourTimesTen = gm.Hour * n; // 8 for sleep + 10 until fatigue
 
        timeAwake = int.Parse(gm.Hour.ToString("00") + gm.Minutes.ToString("00"));
        timeToFatigue = int.Parse(hourTimesTen.ToString("00") + gm.Minutes.ToString("00"));
    }
}
