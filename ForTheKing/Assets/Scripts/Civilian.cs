using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Civilian : MonoBehaviour
{
    public int gridX = 0;
    public int gridY = 0;
    public bool isAlive = true;

    private SpriteRenderer sr;
    private System.Diagnostics.Stopwatch timer;
    private int animationPhase, animationSpeed;
    private long initialTime;
    private Direction facing;

    public Sprite[] sprites;
    private int prevGridX, prevGridY;

    // Start is called before the first frame update
    void Start()
    {
        initialTime = 0;
        facing = Direction.DOWN;
        timer = new System.Diagnostics.Stopwatch();
        timer.Start();
        sr = GetComponent<SpriteRenderer>();
        animationSpeed = 250;

        prevGridX = gridX;
        prevGridY = gridY;
    }

    public void snapToGrid()
    {
        Tilemap tiles = GameObject.Find("Tilemap").GetComponent<Tilemap>();
        int gridSize = tiles.size.y;
        Vector3 offset = new Vector3(tiles.cellSize.x / 2, tiles.cellSize.x / 2, -1);
        transform.position = tiles.CellToWorld(new Vector3Int(gridX - gridSize / 2, gridY - gridSize / 2, 0)) + offset;
        facing = Direction.DOWN;
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
            //Uncomment when sprites are hooked up.
            cycleAnimation(facing);
            initialTime = timer.ElapsedMilliseconds;
        }
    }

    private void OnMouseDown()
    {
        //signal to UI to draw selection over this unit
        GameObject.Find("BattleManager").GetComponent<BattleUI>().onUnitClick(gameObject);

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

    private void cycleAnimation(Direction dir)
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
