using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BombermanCore;

public class PlayerController : MonoBehaviour
{
    public enum Types
    {
        Player,
        Enemy,
    };

    public Types thisObjType;
    public float moveSpeed;
    public int noOfBombsAllowed = 1;
    public GameObject bombPrefab;
    public LayerMask obstaclesForBots;

    private Animator animator;
    private Rigidbody2D rBody;
    private Vector2 movementInput = Vector2.zero;
    private Vector2Int currGridCoords;
    private bool dead;
    private Transform currTarget;
    private Vector2Int currGridTarget;

    private static int currNoOfBombs;

    private void OnEnable()
    {
        GridManager.onGameStart += OnGameStart;
        GameManager.onGameOver += GameManager_onGameOver;
    }

    

    private void GameManager_onGameOver()
    {
        
    }

    private void OnGameStart(Vector3? pos = null)
    {
        if (thisObjType == Types.Player)
        {
            if (pos != null)
            {
                //Debug.Log("Pos: " + pos);
                SetupPlayer((Vector3)pos);
            }
        }
        else
        {
            SetupEnemy();
        }
    }

    void SetupPlayer(Vector3 pos)
    {
        transform.position = pos;
        currGridCoords = GridManager.instance.WorldToGridPos(transform.position);
        dead = false;
        currNoOfBombs = noOfBombsAllowed;
    }

    void SetupEnemy()
    {
        Cell potentialCell;
        do
        {
            potentialCell = GridManager.instance.RandomEmptyTile();//.transform.position;
                                                                        //transform.position = pos;
            
            dead = false;
            moveSpeed = Random.Range(2.0f, 4.5f);

            //Set target now-
        } while (!SetNewTarget(potentialCell.X_ID, potentialCell.Y_ID));

        //Set the players position to potential cell.
        transform.position = GridManager.instance.GridToWorld(potentialCell.X_ID, potentialCell.Y_ID);
        currGridCoords = new Vector2Int(potentialCell.X_ID, potentialCell.Y_ID);
    }

    /// <summary>
    /// Returns true if the start cell has path to choose from. If not try finding another potential start cell.
    /// </summary>
    /// <param name="startCell"></param>
    /// <returns></returns>
    bool SetNewTarget(int currCellXid, int currCellYid)
    {
        List<Cell> possibleTargets = new List<Cell>();

        GridManager.instance.FindEgdeNeighbors(currCellXid, currCellYid, false, out possibleTargets);
        //Debug.Log(gameObject.name + " possible targets - " + possibleTargets.Count);
                    
        if(possibleTargets.Count > 0)
        {
            
            currTarget = null;

            while (currTarget == null)
            {
                int randIndex = Random.Range(0, possibleTargets.Count);
                Vector2Int dir = new Vector2Int(possibleTargets[randIndex].X_ID, possibleTargets[randIndex].Y_ID) - new Vector2Int(currCellXid, currCellYid);
                //dir.Normalize();
                //Vector2Int targetGridCoords = GridManager.instance.WorldToGridPos((Vector2)EndPointInThisDir(dir).position + (-1 * dir));
                currTarget = possibleTargets[randIndex].transform;

                if (currTarget == null)
                {
                    possibleTargets.RemoveAt(randIndex);
                    if (possibleTargets.Count <= 0)
                    {
                        Debug.LogError("Ran out of options..");
                    }
                }
                else
                {
                    movementInput = dir;
                }
            }
            return true;
        }
        else    //If there are no possible paths, try finding another.
        {
            return false;
            //SetNewTarget();
        }
        //while (possibleTargets.Count <= 0);

        
    }

    private void OnDisable()
    {
        GridManager.onGameStart -= OnGameStart;
        GameManager.onGameOver -= GameManager_onGameOver;
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rBody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (GameManager.instance.gameBeingPlayed)
        {
            if (!dead)
            {
                if (thisObjType == Types.Player)
                    ProcessInput();
                else
                    Navigate();

                Animate();
            }
        }
    }

    void ProcessInput()
    {
        //This allows input to be processed only on one axis which is necessary for bomberman as one cannot navigate diagonally.
        Vector2 movementInputNew = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));   
        if(movementInputNew.x != 0 && movementInputNew.y != 0)
        {
            Move(movementInput);
        }
        else
        {
            if(movementInputNew.x == 0 && movementInputNew.y == 0)
            {
                movementInput = Vector2.zero;
            }
            else
            {
                movementInput = movementInputNew;
                Move(movementInput);
            }
        }

        if(Input.GetButtonDown("DropBomb"))
        {
            DropBomb();
        }
    }

    void Navigate()
    {
        
        if(Vector3.Distance(transform.position, currTarget.position) <= 0.05f)
        {
            //Debug.Log("Decide new target");
            SetNewTarget(currGridCoords.x, currGridCoords.y);
        }
        else
        {
            Move(movementInput);
        }
    }

    void Move(Vector3 input)
    {
        Vector3 newPos = transform.position + (input * moveSpeed * Time.deltaTime);
        Vector3 playAreaMinPos = GridManager.instance.GridToWorld(1, 1);
        Vector3 playAreaMaxPos = GridManager.instance.GridToWorld(GridManager.instance.xLength - 2, GridManager.instance.yLength - 2);

        newPos.x = Mathf.Clamp(newPos.x, playAreaMinPos.x, playAreaMaxPos.x);
        newPos.y = Mathf.Clamp(newPos.y, playAreaMinPos.y, playAreaMaxPos.y);

        rBody.MovePosition(newPos);
        currGridCoords = GridManager.instance.WorldToGridPos(rBody.position);
    }

    void Animate()
    {
        animator.SetFloat("Horizontal", movementInput.x);
        animator.SetFloat("Vertical", movementInput.y);
    }

    void DropBomb()
    {
        if (currNoOfBombs > 0)
        {
            currNoOfBombs--;
            GameObject bombInstance = Instantiate(bombPrefab) as GameObject;
            bombInstance.GetComponent<BombController>().SetIDs(currGridCoords.x, currGridCoords.y);
            bombInstance.transform.position = GridManager.instance.GridToWorld(currGridCoords.x, currGridCoords.y);
        }
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessCollision(collision);
    }

    void ProcessCollision(Collider2D other)
    {
        if (other.CompareTag("Flame"))
        {   
            Die();
        }

        if (thisObjType == Types.Player)
        {
            if (other.CompareTag("Enemy"))
                Die();
        }
    }

    void ProcessCollision(Collision2D collision)
    {
        if (thisObjType == Types.Player)
        {
            if (collision.otherCollider.CompareTag("Enemy"))
                Die();
        }
    }

    void Die()
    {
        if (!dead)
        {
            dead = true;
            //GameManager.instance.GameOver();
            animator.SetTrigger("Death");
            Destroy(gameObject, 1.25f);
        }
    }

    private void OnDestroy()
    {
        if (thisObjType == Types.Player)
            GameManager.instance.GameOver(GameOverReasons.PlayerDead);
        else
            GameManager.instance.OnEnemyKilled();
    }

    /// <summary>
    /// Returns the transform up to which is walkable in the 'dir' direction.
    /// </summary>
    /// <param name="dir">Direction to follow.</param>
    /// <returns></returns>
    public Transform EndPointInThisDir(Vector3 dir)
    {
        Transform hit;
        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, dir, 100, obstaclesForBots);
        hit =  hitInfo.transform;
        
        return hit;
    }

    public static void OnBombExploded()
    {
        currNoOfBombs++;
    }
}
