using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float moveSpeed;

    void Move(Vector3 dir)
    {
        Vector3 newPos = transform.position + (dir * moveSpeed * Time.deltaTime);
        Vector3 playAreaMinPos = GridManager.instance.GridToWorld(1, 1);
        Vector3 playAreaMaxPos = GridManager.instance.GridToWorld(GridManager.instance.xLength - 2, GridManager.instance.yLength - 2);

        newPos.x = Mathf.Clamp(newPos.x, playAreaMinPos.x, playAreaMaxPos.x);
        newPos.y = Mathf.Clamp(newPos.y, playAreaMinPos.y, playAreaMaxPos.y);

        //rBody.MovePosition(newPos);
        //currGridCoords = GridManager.instance.WorldToGridPos(rBody.position);
    }
}
