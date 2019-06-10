using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameController : MonoBehaviour
{
    public Vector2Int originOfFlame;

    private void OnEnable()
    {
        Destroy(gameObject, 1.0f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ProcessCollision(collision);
    }

    //private void OnTriggerStay2D(Collider2D collision)
    //{
    //    ProcessCollision(collision);
    //}

    private void OnTriggerExit2D(Collider2D collision)
    {
        ProcessCollision(collision);
    }

    void ProcessCollision(Collider2D other)
    {
        //Debug.Log("Collided with: " + other.name);
        if(other.CompareTag("SoftWall"))
        {
            Destroy(other.gameObject);
        }
    }

    public void SetOriginOfThisFlame(int x, int y)
    {
        originOfFlame.x = x;
        originOfFlame.y = y;
    }
}
