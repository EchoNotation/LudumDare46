using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ControllableUnit : MonoBehaviour
{

    //position in board array
    public int gridX = 0;
    public int gridY = 0;

    public bool hasMoved = false;
    public bool hasTakenAction = false;
    public bool isAlive = true;
    public int unitID;

    public List<Vector2Int> moveablePositions;

    public enum UnitType
    {
        KNIGHT,
        JESTER,
        NOBLE
    }

    public UnitType unitType;
    public Sprite knightSprite, jesterSprite, nobleSprite;
    private int speed;

    private SpriteRenderer sr;
    private System.Diagnostics.Stopwatch timer;
    private int animationSpeed, animationPhase;
    private long initialTime;
    public Sprite[] sprites;
    private Sprite[] currentSprites, blockingSprites;
    private Direction facing;
    public bool blocking;

    private int prevGridX, prevGridY;

    // Start is called before the first frame update
    void Start()
    {
        currentSprites = new Sprite[12];
        blockingSprites = new Sprite[12];

        //blocking = false;
        //First 24 sprites are for the Knight, following 12 are for the jester, 12 after that are for the noble.
        
        switch(unitType)
        {
            case UnitType.KNIGHT:
                speed = 1;
                this.GetComponent<SpriteRenderer>().sprite = knightSprite;

                for(int i = 0; i < currentSprites.Length; i++)
                {
                    currentSprites[i] = sprites[i];
                    blockingSprites[i] = sprites[i + 12];
                }
                break;
            case UnitType.JESTER:
                this.GetComponent<SpriteRenderer>().sprite = jesterSprite;

                for(int i = 0; i < currentSprites.Length; i++)
                {
                    currentSprites[i] = sprites[i + 24];
                }
                speed = 3;
                break;
            case UnitType.NOBLE:
                this.GetComponent<SpriteRenderer>().sprite = nobleSprite;

                for(int i = 0; i < currentSprites.Length; i++)
                {
                    currentSprites[i] = sprites[i + 36];
                }
                speed = 2;
                break;
        }

        ////align the unit with the tile on Start
        Tilemap tiles = GameObject.Find("Tilemap").GetComponent<Tilemap>();
        int gridSize = tiles.size.y;
        Vector3 offset = new Vector3(tiles.cellSize.x / 2, tiles.cellSize.x / 2, -1);
        transform.position = tiles.CellToWorld(new Vector3Int(gridX - gridSize/2, gridY - gridSize/2, 0)) + offset;

        prevGridX = gridX;
        prevGridY = gridY;

        timer = new System.Diagnostics.Stopwatch();
        timer.Start();
        facing = Direction.DOWN;
        sr = GetComponent<SpriteRenderer>();
        initialTime = timer.ElapsedMilliseconds;
        animationSpeed = 250;
    }

    public void updateMoveablePositions()
    {
        moveablePositions.Clear();

        BattleManager bmanager = FindObjectOfType<BattleManager>();

        moveablePositions = new List<Vector2Int>();
        Vector2Int[] moves = bmanager.passableInRadius(new Vector2Int(gridX - 5, gridY - 5), speed);
        for(int i = 0; i < moves.Length; i++)
        {
            moveablePositions.Add(moves[i]);
        }

        /*
        for(int i = -speed; i <= speed; i++)
        {
            for(int j = -speed; j <= speed; j++)
            {
                //gridX to tileSpace
                 
                Vector2Int offset = new Vector2Int(i, j);
                Vector2Int current = boardToTileSpace(gridX, gridY);
                Vector2Int dest = current + offset;
                //Debug.Log("testing to " + dest + "an offset of " + offset);
                int dist = bmanager.pathDistance(current, dest);
                if (dist == -1) continue;
                if (dist <= speed + 1)
                {
                    moveablePositions.Add(dest);
                    //Debug.Log("can move to " + dest);
                }
            }
        }
        */

        //Debug.Log("Found " + moveablePositions.Count + " spaces to move to");
    }

    private Vector2Int boardToTileSpace(int i, int j)
    {
        int offset = FindObjectOfType<Tilemap>().size.y / 2;
        return new Vector2Int(i - offset, j - offset);
    }

    private void OnDrawGizmos()
    {
        /*
        for(int i = 0; i < moveablePositions.Count; i++)
        {
            float offset = FindObjectOfType<Tilemap>().cellSize.x / 2;
            Vector3 offset3D = new Vector3(offset, offset, 0);
            Vector3 pos = new Vector3(moveablePositions[i].x, moveablePositions[i].y, 0);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(pos + offset3D, 0.25f);
        }
        */
    }

    // Update is called once per frame
    void Update()
    {
        if(gridX != prevGridX || gridY != prevGridY)
        {
            updateDirection();
        }

        if(timer.ElapsedMilliseconds - initialTime > animationSpeed)
        {
            //Uncomment this line once all animations are made.
            cycleAnimation(facing);
            initialTime = timer.ElapsedMilliseconds;
        }
    }

    private void OnMouseDown()
    {
        //signal to UI to draw selection over this unit
        GameObject.Find("BattleManager").GetComponent<BattleUI>().onUnitClick(gameObject);

    }

    public UnitType getUnitType()
    {
        return unitType;
    }

    public int getSpeed()
    {
        return speed;
    }

    void updateDirection()
    {
        string x, y;

        if(gridX - prevGridX > 0)
        {
            x = "R";
        }
        else if(gridX - prevGridX < 0)
        {
            x = "L";
        }
        else
        {
            x = "";
        }

        if(gridY - prevGridY > 0)
        {
            y = "U";
        }
        else if(gridY - prevGridY < 0)
        {
            y = "D";
        }
        else
        {
            y = "";
        }

        string xy = x + y;

        switch(xy)
        {
            case "":
                break;
            case "L":
                facing = Direction.LEFT;
                break;
            case "R":
                facing = Direction.RIGHT;
                break;
            case "U":
                facing = Direction.UP;
                break;
            case "D":
                facing = Direction.DOWN;
                break;
            case "LU":
                if(Mathf.Abs(gridX - prevGridX) > Mathf.Abs(gridY - prevGridY))
                {
                    facing = Direction.LEFT;
                }
                else
                {
                    facing = Direction.UP;
                }
                break;
            case "LD":
                if (Mathf.Abs(gridX - prevGridX) > Mathf.Abs(gridY - prevGridY))
                {
                    facing = Direction.LEFT;
                }
                else
                {
                    facing = Direction.DOWN;
                }
                break;
            case "RU":
                if (Mathf.Abs(gridX - prevGridX) > Mathf.Abs(gridY - prevGridY))
                {
                    facing = Direction.RIGHT;
                }
                else
                {
                    facing = Direction.UP;
                }
                break;
            case "RD":
                if (Mathf.Abs(gridX - prevGridX) > Mathf.Abs(gridY - prevGridY))
                {
                    facing = Direction.RIGHT;
                }
                else
                {
                    facing = Direction.DOWN;
                }
                break;
            default:
                Debug.Log("Invalid facing string! String: " + xy);
                break;
        }

        prevGridX = gridX;
        prevGridY = gridY;
    }

    void cycleAnimation(Direction dir)
    {
        if(blocking)
        {
            switch (dir)
            {
                case Direction.UP:
                    switch (animationPhase)
                    {
                        case 0:
                            sr.sprite = blockingSprites[0];
                            animationPhase = 1;
                            break;
                        case 1:
                            sr.sprite = blockingSprites[1];
                            animationPhase = 2;
                            break;
                        case 2:
                            sr.sprite = blockingSprites[2];
                            animationPhase = 3;
                            break;
                        case 3:
                            sr.sprite = blockingSprites[1];
                            animationPhase = 0;
                            break;
                        default:
                            Debug.Log("Invalid animation phase! Phase: " + animationPhase);
                            animationPhase = 0;
                            break;
                    }
                    break;
                case Direction.LEFT:
                    switch (animationPhase)
                    {
                        case 0:
                            sr.sprite = blockingSprites[3];
                            animationPhase = 1;
                            break;
                        case 1:
                            sr.sprite = blockingSprites[4];
                            animationPhase = 2;
                            break;
                        case 2:
                            sr.sprite = blockingSprites[5];
                            animationPhase = 3;
                            break;
                        case 3:
                            sr.sprite = blockingSprites[4];
                            animationPhase = 0;
                            break;
                        default:
                            Debug.Log("Invalid animation phase! Phase: " + animationPhase);
                            animationPhase = 0;
                            break;
                    }
                    break;
                case Direction.RIGHT:
                    switch (animationPhase)
                    {
                        case 0:
                            sr.sprite = blockingSprites[6];
                            animationPhase = 1;
                            break;
                        case 1:
                            sr.sprite = blockingSprites[7];
                            animationPhase = 2;
                            break;
                        case 2:
                            sr.sprite = blockingSprites[8];
                            animationPhase = 3;
                            break;
                        case 3:
                            sr.sprite = blockingSprites[7];
                            animationPhase = 0;
                            break;
                        default:
                            Debug.Log("Invalid animation phase! Phase: " + animationPhase);
                            animationPhase = 0;
                            break;
                    }
                    break;
                case Direction.DOWN:
                    switch (animationPhase)
                    {
                        case 0:
                            sr.sprite = blockingSprites[9];
                            animationPhase = 1;
                            break;
                        case 1:
                            sr.sprite = blockingSprites[10];
                            animationPhase = 2;
                            break;
                        case 2:
                            sr.sprite = blockingSprites[11];
                            animationPhase = 3;
                            break;
                        case 3:
                            sr.sprite = blockingSprites[10];
                            animationPhase = 0;
                            break;
                        default:
                            Debug.Log("Invalid animation phase! Phase: " + animationPhase);
                            animationPhase = 0;
                            break;
                    }
                    break;
                default:
                    Debug.Log("Invalid direction! Direction: " + dir.ToString());
                    break;
            }
        }
        else
        {
            switch (dir)
            {
                case Direction.UP:
                    switch (animationPhase)
                    {
                        case 0:
                            sr.sprite = currentSprites[0];
                            animationPhase = 1;
                            break;
                        case 1:
                            sr.sprite = currentSprites[1];
                            animationPhase = 2;
                            break;
                        case 2:
                            sr.sprite = currentSprites[2];
                            animationPhase = 3;
                            break;
                        case 3:
                            sr.sprite = currentSprites[1];
                            animationPhase = 0;
                            break;
                        default:
                            Debug.Log("Invalid animation phase! Phase: " + animationPhase);
                            animationPhase = 0;
                            break;
                    }
                    break;
                case Direction.LEFT:
                    switch (animationPhase)
                    {
                        case 0:
                            sr.sprite = currentSprites[3];
                            animationPhase = 1;
                            break;
                        case 1:
                            sr.sprite = currentSprites[4];
                            animationPhase = 2;
                            break;
                        case 2:
                            sr.sprite = currentSprites[5];
                            animationPhase = 3;
                            break;
                        case 3:
                            sr.sprite = currentSprites[4];
                            animationPhase = 0;
                            break;
                        default:
                            Debug.Log("Invalid animation phase! Phase: " + animationPhase);
                            animationPhase = 0;
                            break;
                    }
                    break;
                case Direction.RIGHT:
                    switch (animationPhase)
                    {
                        case 0:
                            sr.sprite = currentSprites[6];
                            animationPhase = 1;
                            break;
                        case 1:
                            sr.sprite = currentSprites[7];
                            animationPhase = 2;
                            break;
                        case 2:
                            sr.sprite = currentSprites[8];
                            animationPhase = 3;
                            break;
                        case 3:
                            sr.sprite = currentSprites[7];
                            animationPhase = 0;
                            break;
                        default:
                            Debug.Log("Invalid animation phase! Phase: " + animationPhase);
                            animationPhase = 0;
                            break;
                    }
                    break;
                case Direction.DOWN:
                    switch (animationPhase)
                    {
                        case 0:
                            sr.sprite = currentSprites[9];
                            animationPhase = 1;
                            break;
                        case 1:
                            sr.sprite = currentSprites[10];
                            animationPhase = 2;
                            break;
                        case 2:
                            sr.sprite = currentSprites[11];
                            animationPhase = 3;
                            break;
                        case 3:
                            sr.sprite = currentSprites[10];
                            animationPhase = 0;
                            break;
                        default:
                            Debug.Log("Invalid animation phase! Phase: " + animationPhase);
                            animationPhase = 0;
                            break;
                    }
                    break;
                default:
                    Debug.Log("Invalid direction! Direction: " + dir.ToString());
                    break;
            }
        }    
    }
}

