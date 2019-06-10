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

    private void OnEnable()
    {
        neighbors = new List<Cell>();
        StartCoroutine(Explode(explosionTimeout));
        
    }

    public void SetIDs(int xId, int yId)
    {
        _Xid = xId;
        _Yid = yId;
    }

    IEnumerator Explode(float delay)
    {
        yield return new WaitForEndOfFrame();

        GridManager.instance.FindNeighbors(_Xid, _Yid, true, out neighbors);

        yield return new WaitForSeconds(delay);

        for(int i=0;i<neighbors.Count;i++)
        {
            GameObject flameInstance = Instantiate(FlamePrefab) as GameObject;
            flameInstance.GetComponent<FlameController>().SetOriginOfThisFlame(_Xid, _Yid);
            flameInstance.transform.position = GridManager.instance.GridToWorld(neighbors[i].X_ID, neighbors[i].Y_ID);
        }

        yield return new WaitForEndOfFrame();
        PlayerController.OnBombExploded();
        Destroy(gameObject);
    }
}
