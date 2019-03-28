using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Player target;
    public float speed;
    public float minSize;
    public float maxSize;
    public bool ready;
    Vector2 velocity;

    GameManager gm;
    MapManager map;
    float mapX;
    float mapY;

    private void Awake()
    {
        gm = FindObjectOfType<GameManager>();
        map = FindObjectOfType<MapManager>();
        mapX = map.maxMapSizeX / 2;
        mapY = map.maxMapSizeY / 2;
    }

    public void Update()
    {
        if (!gm.GameStarted)
        {
            return;
        }

        UpdatePosition();
        Zoom();
    }

    public void PlayerLoaded()
    {
        target = FindObjectOfType<Player>();
        StartCoroutine(TargetPlayer());
    }

    void UpdatePosition()
    {
        velocity.x = Input.GetAxisRaw("Horizontal");
        velocity.y = Input.GetAxisRaw("Vertical");
        transform.Translate(velocity.normalized * (speed * 4) * Time.deltaTime);
        Vector2 clampedPos = transform.position;
        clampedPos.x = Mathf.Clamp(transform.position.x, -mapX, mapX);
        clampedPos.y = Mathf.Clamp(transform.position.y, -mapY, mapY);
        transform.position = new Vector3(clampedPos.x, clampedPos.y, -10);
    }

    void Zoom()
    {
        if(Input.mouseScrollDelta.y > 0)
        {
            if(Camera.main.orthographicSize > minSize)
            {
                Camera.main.orthographicSize -= 1;
            }
            else
            {
                Camera.main.orthographicSize = minSize;
            }
        }
        else if(Input.mouseScrollDelta.y < 0)
        {
            if (Camera.main.orthographicSize < maxSize)
            {
                Camera.main.orthographicSize += 1;
            }
            else
            {
                Camera.main.orthographicSize = maxSize;
            }
        }
    }

    IEnumerator TargetPlayer()
    {
        float step = speed * Time.deltaTime;
        while(transform.position.x != target.transform.position.x && transform.position.y != target.transform.position.y)
        {
            Vector3 camPos = new Vector3(transform.position.x, transform.position.y, -10f);
            Vector3 targetPos = new Vector3(target.transform.position.x, target.transform.position.y, -10f);
            transform.position = Vector3.MoveTowards(camPos, targetPos, step);
            yield return null;
        }

        ready = true;
    }
}
