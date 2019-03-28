using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    Vector3 mousePos;
    RaycastHit2D hit;

    // Update is called once per frame
    void Update()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        hit = Physics2D.Raycast(mousePos, Vector3.zero);
        if (hit)
        {
            Debug.Log("Hit: " + hit.collider.transform.name);
        }
    }
}
