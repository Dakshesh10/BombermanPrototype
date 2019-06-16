using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BombermanCore
{
    public enum CellTypes
    {
        Grass,
        RigidWall,
        SoftWall,
        Portal,
    }

    public enum Powerups
    {
        None,
        BombPower,
        SpeedPower,
    }

    [System.Serializable]
    public class Cell : MonoBehaviour
    { 
        public CellTypes thisCellType;
        public Powerups thisCellHasPower;
        public bool isThisCellOnFire;
        private int xId, yId;
        public int X_ID
        {
            get
            {
                return xId;
            }
        }

        public int Y_ID
        {
            get
            {
                return yId;
            }
        }
        
        public void SetIDs(int x, int y)
        {
            xId = x;
            yId = y;
        }

        private void OnDestroy()
        {
            if(thisCellType == CellTypes.SoftWall && thisCellHasPower != Powerups.None && isThisCellOnFire)
            {
                GridManager.instance.SpawnPowerup(thisCellHasPower, xId, yId);
            }
        }
    }
}
