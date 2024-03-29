﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BombermanCore;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;
    public int xLength, yLength;
    
    [Range(0.0f,1.0f)]
    public float rigidWallFillRate = 0.25f;

    [Range(0.0f, 1.0f)]
    public float softWallFillRate = 0.25f;

    private Vector2 cellScale;
    private Vector2 cellSize;
    private Vector2 gridOffset;

    Cell[,] Grid;

    Dictionary<CellTypes, GameObject> cellTypesDictionary;
    Dictionary<Powerups, GameObject> powerupsPrefabsDict;
    List<Cell> emptyTiles;      //These are the tiles on which the enemies will spawn.
    List<Cell> allSoftWalls;
    List<PlayerController> enemies;
    PlayerController player;
    List<GameObject> powerUpPrefabs;

    int noOfPowerUps;

    /// <summary>
    /// Delegate for Game start event.
    /// </summary>
    /// <param name="pos"></param>
    public delegate void OnGameStart(Vector3? pos = null);
    public static event OnGameStart onGameStart;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
            
        else
            Destroy(gameObject);
        cellTypesDictionary = new Dictionary<CellTypes, GameObject>();
        powerupsPrefabsDict = new Dictionary<Powerups, GameObject>();

        //Load prefabs from the resources and set them to the dictionary.
        int n = System.Enum.GetNames(typeof(CellTypes)).Length;
        for (int i = 0; i < n; i++)
        {
            CellTypes tileType = (CellTypes)i;
            GameObject prefab = Instantiate(Resources.Load("Prefabs/" + tileType.ToString())) as GameObject;
            cellTypesDictionary.Add(tileType, prefab);
            prefab.SetActive(false);
        }

       
        CreatePlayerAndEnemies();

        CreatePowerupsDictionary();
    }

    void CreatePowerupsDictionary()
    {
        noOfPowerUps = System.Enum.GetNames(typeof(Powerups)).Length;
        for (int i = 1; i < noOfPowerUps; i++)
        {
            Powerups powerType = (Powerups)i;
            GameObject prefab = Instantiate(Resources.Load("Prefabs/" + powerType.ToString())) as GameObject;
            powerupsPrefabsDict.Add(powerType, prefab);
            prefab.SetActive(false);
        }
    }

    void InitializeGrid()
    {
        //Destroy all child objects for new Grid.
        if(transform.childCount > 0)
        {
            for(int i=0;i<transform.childCount;i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
        Grid = new Cell[xLength, yLength];
        emptyTiles = new List<Cell>();
        allSoftWalls = new List<Cell>();
        
    }

    private void CreatePlayerAndEnemies()
    {
        enemies = new List<PlayerController>();
        GameObject playerInstance = Instantiate(Resources.Load("Prefabs/Player")) as GameObject;
        player = playerInstance.GetComponent<PlayerController>();
        playerInstance.SetActive(false);

        for(int i=0;i<GameManager.instance.noOfEnemies;i++)
        {
            GameObject enemy = Instantiate(Resources.Load("Prefabs/Enemy")) as GameObject;
            enemies.Add(enemy.GetComponent<PlayerController>());
            enemy.SetActive(false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //StartGame();
    }

    public void StartGame()
    {
        InitializeGrid();
        GenerateGrid();
        GenerateBoundryWalls();
        FillPlayAreaWithRigidWalls();
        FillWithSoftWalls();
        SetupPowerup();
        player.gameObject.SetActive(true);
        for (int i = 0; i < GameManager.instance.noOfEnemies; i++)
        {
            enemies[i].gameObject.SetActive(true);
        }

        if (onGameStart != null)
            onGameStart(GridToWorld(1, yLength-2));     //Player should start from top left block.
    }

    void SetupPowerup()
    {
        Cell powerUpCell = allSoftWalls[Random.Range(0, allSoftWalls.Count)];
        powerUpCell.thisCellHasPower = (Powerups)Random.Range(1, noOfPowerUps);
        Debug.Log(powerUpCell.name + ": has " + powerUpCell.thisCellHasPower);
    }

    void GenerateGrid()
    {
        for(int i=0;i<xLength;i++)
        {
            for(int j=0;j<yLength;j++)
            {
                SpawnTile(i, j, CellTypes.Grass);
            }
        }
    }

    void GenerateBoundryWalls()
    {
        //First spawn boundaries.

        //bottom and top rows of boundary
        for (int i = 0; i < yLength; i++)
        {
            //Spawn rigid walls on the edges.
            SpawnTile(0, i, CellTypes.RigidWall);   
            SpawnTile(xLength-1, i, CellTypes.RigidWall);  
        }

        //first and last column of boundary
        for (int j = 0; j < xLength; j++)
        {
            //Spawn rigid walls on the edges.
            SpawnTile(j, 0, CellTypes.RigidWall);
            SpawnTile(j, yLength-1, CellTypes.RigidWall);
        }

        //int noOfRigidWalls = Random.Range(noOfRigidWallsRange.x, noOfRigidWallsRange.y);

        
    }

    void FillPlayAreaWithRigidWalls()
    {
        //Since all edge tiles are already rigid wall, this loop will iterate through the remaining ones.
        for (int i = 2; i < xLength - 2; i++)
        {
            for (int j = 2; j < yLength - 2; j++)
            {
                //SpawnTile(i, j, CellTypes.RigidWall);
                if (Random.Range(0, 100) > (1 - rigidWallFillRate) * 100)
                {
                    SpawnTile(i, j, CellTypes.RigidWall);
                }
            }
        }
    }

    private void FillWithSoftWalls()
    {
        //After all rigid walls are spawned, we can use remaining space for soft walls
        for (int i = 1; i < xLength - 1; i ++)
        {
            for (int j = 1; j < yLength - 2; j ++)
            {
                //SpawnTile(i, j, CellTypes.RigidWall);
                if (Grid[i, j].thisCellType == CellTypes.Grass)
                {
                    if ((Random.Range(0, 100) > (1 - softWallFillRate) * 100))
                    {
                        SpawnTile(i, j, CellTypes.SoftWall);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Spawns tile at requested x and y matrix indeces. It overrides the previous entry and destroys the stale object.
    /// </summary>
    /// <param name="x">current row to spawn on</param>
    /// <param name="y">curreny column to spawn on</param>
    /// <param name="cellType">Cell type thet needs to be spawn</param>
    void SpawnTile(int x, int y, CellTypes cellType)
    {
        //if(cellType == CellTypes.RigidWall)
        //{
        //    Debug.Log("x: " + x + " - y:" + y);
        //}
        if(Grid[x,y] != null)   //If there is already a cell in the given pos.
        {
            if (Grid[x,y].thisCellType == CellTypes.Grass)  //And if it is grass 
            {
                //DestroyImmediate(Grid[x, y].gameObject);
                emptyTiles.Remove(Grid[x, y]);
                Grid[x, y] = null;
            }
        }
        GameObject tile = Instantiate(cellTypesDictionary[cellType]);
        Sprite prefabSprite = tile.GetComponent<SpriteRenderer>().sprite;
        Cell cell = tile.GetComponent<Cell>();
        tile.name = cellType.ToString() + "_" + x + "-" + y;
        cell.SetIDs(x, y);
        cell.isThisCellOnFire = false;
        Grid[x, y] = cell;

        cell.thisCellHasPower = Powerups.None;

        if (cellType == CellTypes.Grass && y < yLength-3 && x > 2)  //If it is grass, and avoids spawnign bots near player at game start.
        {
            emptyTiles.Add(cell);
        }
        
        if(cellType == CellTypes.SoftWall)
        {
            allSoftWalls.Add(cell);
        }

        gridOffset.x = -(xLength / 2);
        gridOffset.y = -(yLength / 2);

        if(yLength % 2 == 0)
            gridOffset.y += 0.5f;

        if (xLength % 2 == 0)
            gridOffset.x += 0.5f;

        tile.transform.position = GridToWorld(x, y);
        //Debug.Log(tile.name + " at: " + tile.transform.position);
        tile.transform.parent = transform;

        if(!tile.activeSelf)
        {
            tile.SetActive(true);
        }
    }

    public Vector3 GridToWorld(int x, int y)
    {
        return new Vector3(transform.position.x + x + gridOffset.x, transform.position.y + y + gridOffset.y, 0f);
    }

    public Vector2Int WorldToGridPos(Vector3 pos)
    {
        float xOffset = 0;
        float yOffset = 0;
            
        if (yLength % 2 == 0)
            yOffset += 0.5f;

        if (xLength % 2 == 0)
            xOffset += 0.5f;
        //Debug.Log(xOffset + " " + yOffset);
        Vector3 posCell00 = GridToWorld(0, 0) - new Vector3(0.25f, 0.25f, 0);
        return new Vector2Int(Mathf.Abs((int)((pos - posCell00).x + xOffset)), Mathf.Abs((int)((pos - posCell00).y + yOffset)));
    }

    /// <summary>
    /// Outs a list of neighboring cells which are grass and soft walls. This skips the rigid walls.
    /// </summary>
    /// <param name="x">Grid x coordinate of the cell.</param>
    /// <param name="y">Grid y coordinate of the cell</param>
    /// <param name="includeSoftWall">include soft walls in the list or not.</param>
    /// <param name="neighbors">List to out to.</param>
    public void FindNeighbors(int x, int y, bool includeSoftWall, out List<Cell> neighbors)
    {
        neighbors = new List<Cell>();
        int xStart = x - 1, xEnd = x + 1;
        int yStart = y - 1, yEnd = y + 1;

        for(int i=xStart;i<=xEnd;i++)
        {
            for(int j=yStart;j<=yEnd;j++)
            {
                if (i < 0 || j < 0 || i > xLength - 1 || j > yLength - 1)
                    break;

                if(Grid[i,j].thisCellType == CellTypes.Grass || (includeSoftWall && Grid[i, j].thisCellType == CellTypes.SoftWall)) 
                    neighbors.Add(Grid[i,j]);
            }
        }
    }

    public void FindEgdeNeighbors(int x, int y, bool includeSoftWall, out List<Cell> neighbors)
    {
        neighbors = new List<Cell>();
        int xStart = x - 1, xEnd = x + 1;
        int yStart = y - 1, yEnd = y + 1;

        //Edge neighbors in row.
        for (int i = xStart; i <= xEnd; i++)
        {
            if(i!=x)        //Exclude the curr tile
            {
                if (Grid[i, y].thisCellType == CellTypes.Grass || (includeSoftWall && Grid[i, y].thisCellType == CellTypes.SoftWall))
                    neighbors.Add(Grid[i, y]);
            }
        }

        //Edge neighbors in cols.
        for (int i = yStart; i <= yEnd; i++)
        {
            if (i != y)
            {
                if (Grid[x, i].thisCellType == CellTypes.Grass || (includeSoftWall && Grid[x, i].thisCellType == CellTypes.SoftWall))
                    neighbors.Add(Grid[x, i]);
            }
        }
    }

    public Cell RandomEmptyTile()
    {
        int index = Random.Range(0, emptyTiles.Count);
        Cell emptyTile = emptyTiles[index];
        emptyTiles.RemoveAt(index);
        return emptyTile;
    }

    public Cell CellAt(int x, int y)
    {
        return Grid[x, y];
    }

    public void Restart()
    {
        InitializeGrid();
        StartGame();
    }

    public bool IsPlayerCollidingWithAnyEnemy(Vector2Int playerGridCoords)
    {
        bool isColliding = false;

        for(int i=0;i<enemies.Count;i++)
        {
            if(enemies[i].currGridCoords == playerGridCoords && !enemies[i].isDead())
            {
                return true;
            }
        }
        return isColliding;
    }

    public void SpawnPowerup(Powerups type, int atGridX, int atGridY)
    {
        GameObject powerupInstance = Instantiate(powerupsPrefabsDict[type], GridToWorld(atGridX, atGridY), Quaternion.identity);
        powerupInstance.transform.parent = transform;
        powerupInstance.SetActive(true);
    }
}
