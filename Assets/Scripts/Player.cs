using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public enum State { Rest, Exploring, }
    public State state = State.Rest;

    public Vector2 currentPosition;

    public int energy = 3000;

    GameManager gm;

    public void Awake()
    {
        gm = FindObjectOfType<GameManager>();
    }

    public void ReduceEnergy(int val) 
    {
        energy -= val;
    }
}
