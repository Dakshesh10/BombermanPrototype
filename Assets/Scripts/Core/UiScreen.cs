using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BombermanCore
{
    [System.Serializable]
    public class UiScreen
    {
        public GameObject rootPanel;

        public virtual void SetActive(bool flag)
        {
            rootPanel.SetActive(flag);
        }
    }

    public enum GameOverReasons
    {
        PlayerDead,
        AllEnemiesDead,
    };
}
