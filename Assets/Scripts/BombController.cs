using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BombermanCore;

public class BombController : MonoBehaviour
{
    public float explosionTimeout = 3.0f;
    public GameObject FlamePrefab;

    private int _Xid, _Yid;
    List<Cell> neighbors;
    bool exploded;

    private void OnEnable()
    {
        exploded = false;
        neighbors = new List<Cell>();
    }

    public void SetIDs(int xId, int yId)
    {
        _Xid = xId;
        _Yid = yId;

        GridManager.instance.FindNeighbors(_Xid, _Yid, true, out neighbors);

        if (!exploded)
            StartCoroutine(WaitAndExplode(explosionTimeout));
    }

    private void Update()
    {
        if (GridManager.instance.CellAt(_Xid, _Yid).isThisCellOnFire && !exploded)
        {
            StopAllCoroutines();
            StartCoroutine(WaitAndExplode(0.025f));
        }
    }

    IEnumerator WaitAndExplode(float delay)
    {
        exploded = true;
        yield return new WaitForSeconds(delay);

        Explode();
        yield return new WaitForEndOfFrame();
        PlayerController.OnBombExploded();
        Destroy(gameObject);
    }

    //INstantiate flames on neighbour cells.
    void Explode()
    {
        for (int i = 0; i < neighbors.Count; i++)
        {
            GameObject flameInstance = Instantiate(FlamePrefab) as GameObject;
            FlameController flameController = flameInstance.GetComponent<FlameController>();
            flameController.SetOriginOfThisFlame(_Xid, _Yid);
            flameController.SetIDs(neighbors[i].X_ID, neighbors[i].Y_ID);
            neighbors[i].isThisCellOnFire = true;
            flameInstance.transform.position = GridManager.instance.GridToWorld(neighbors[i].X_ID, neighbors[i].Y_ID);
        }
    }
}
