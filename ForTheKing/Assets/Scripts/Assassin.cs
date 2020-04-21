using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Assassin : MonoBehaviour
{
    public enum EndAction
    {
        IDLE,
        STAB,
        AIM,
        FIRE
    }

    public EndAction endAction = EndAction.IDLE;

    public int gridX = 0;
    public int gridY = 0;
    public bool isAlive = true;
    private GameObject king;

    public GameObject target;

    public GameObject taunter;
    public bool taunted;

    public int speed = 2;

    public int range = 3;

    public Vector2Int[] desiredPath;
    public Vector2Int[] nextTurnPath;

    BattleUI ui;

    private SpriteRenderer sr;
    private int prevGridX, prevGridY;
    private System.Diagnostics.Stopwatch timer;
    private int animationPhase, animationSpeed;
    private long initialTime;
    public Direction facing;
    public Sprite[] sprites, bowSprites;

    public bool drawingBow;

    // Start is called before the first frame update
    void Start()
    {
        king = GameObject.FindGameObjectWithTag("King");
        ui = FindObjectOfType<BattleUI>();

        timer = new System.Diagnostics.Stopwatch();
        sr = GetComponent<SpriteRenderer>();
        facing = Direction.DOWN;
        drawingBow = false;
        timer.Start();
        initialTime = timer.ElapsedMilliseconds;
        animationSpeed = 250;

        prevGridX = gridX;
        prevGridY = gridY;
    }

    void Update()
    {
        if(gridX != prevGridX || gridY != prevGridY)
        {
            updateDirection();
        }

        if(timer.ElapsedMilliseconds - initialTime > animationSpeed)
        {
            //Uncomment once Assassin sprites are finished.
            cycleAnimation(facing);
            initialTime = timer.ElapsedMilliseconds;
        } 
    }

    public void snapToGrid()
    {
        Tilemap tiles = GameObject.Find("Tilemap").GetComponent<Tilemap>();
        int gridSize = tiles.size.y;
        Vector3 offset = new Vector3(tiles.cellSize.x / 2, tiles.cellSize.x / 2, -1);
        transform.position = tiles.CellToWorld(new Vector3Int(gridX - gridSize / 2, gridY - gridSize / 2, -1)) + offset;
        facing = Direction.DOWN;
    }

    private void OnMouseDown()
    {
        ui.onAssassinClick(gameObject);
    }

    void updateDirection()
    {
        string x, y;

        if (gridX - prevGridX > 0)
        {
            x = "R";
        }
        else if (gridX - prevGridX < 0)
        {
            x = "L";
        }
        else
        {
            x = "";
        }

        if (gridY - prevGridY > 0)
        {
            y = "U";
        }
        else if (gridY - prevGridY < 0)
        {
            y = "D";
        }
        else
        {
            y = "";
        }

        string xy = x + y;

        switch (xy)
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
                if (Mathf.Abs(gridX - prevGridX) > Mathf.Abs(gridY - prevGridY))
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
        if(drawingBow)
        {
            switch (dir)
            {
                case Direction.UP:
                    switch (animationPhase)
                    {
                        case 0:
                            sr.sprite = bowSprites[0];
                            animationPhase = 1;
                            break;
                        case 1:
                            sr.sprite = bowSprites[1];
                            animationPhase = 2;
                            break;
                        case 2:
                            sr.sprite = bowSprites[2];
                            animationPhase = 3;
                            break;
                        case 3:
                            sr.sprite = bowSprites[1];
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
                            sr.sprite = bowSprites[3];
                            animationPhase = 1;
                            break;
                        case 1:
                            sr.sprite = bowSprites[4];
                            animationPhase = 2;
                            break;
                        case 2:
                            sr.sprite = bowSprites[5];
                            animationPhase = 3;
                            break;
                        case 3:
                            sr.sprite = bowSprites[4];
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
                            sr.sprite = bowSprites[6];
                            animationPhase = 1;
                            break;
                        case 1:
                            sr.sprite = bowSprites[7];
                            animationPhase = 2;
                            break;
                        case 2:
                            sr.sprite = bowSprites[8];
                            animationPhase = 3;
                            break;
                        case 3:
                            sr.sprite = bowSprites[7];
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
                            sr.sprite = bowSprites[9];
                            animationPhase = 1;
                            break;
                        case 1:
                            sr.sprite = bowSprites[10];
                            animationPhase = 2;
                            break;
                        case 2:
                            sr.sprite = bowSprites[11];
                            animationPhase = 3;
                            break;
                        case 3:
                            sr.sprite = bowSprites[10];
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
                            sr.sprite = sprites[0];
                            animationPhase = 1;
                            break;
                        case 1:
                            sr.sprite = sprites[1];
                            animationPhase = 2;
                            break;
                        case 2:
                            sr.sprite = sprites[2];
                            animationPhase = 3;
                            break;
                        case 3:
                            sr.sprite = sprites[1];
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
                            sr.sprite = sprites[3];
                            animationPhase = 1;
                            break;
                        case 1:
                            sr.sprite = sprites[4];
                            animationPhase = 2;
                            break;
                        case 2:
                            sr.sprite = sprites[5];
                            animationPhase = 3;
                            break;
                        case 3:
                            sr.sprite = sprites[4];
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
                            sr.sprite = sprites[6];
                            animationPhase = 1;
                            break;
                        case 1:
                            sr.sprite = sprites[7];
                            animationPhase = 2;
                            break;
                        case 2:
                            sr.sprite = sprites[8];
                            animationPhase = 3;
                            break;
                        case 3:
                            sr.sprite = sprites[7];
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
                            sr.sprite = sprites[9];
                            animationPhase = 1;
                            break;
                        case 1:
                            sr.sprite = sprites[10];
                            animationPhase = 2;
                            break;
                        case 2:
                            sr.sprite = sprites[11];
                            animationPhase = 3;
                            break;
                        case 3:
                            sr.sprite = sprites[10];
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
