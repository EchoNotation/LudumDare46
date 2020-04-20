using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class King : MonoBehaviour
{
    public int gridX = 0;
    public int gridY = 0;

    public bool isAlive = true;

    public Sprite[] sprites;
    private System.Diagnostics.Stopwatch timer;
    private int animationSpeed, animationPhase;
    private long initialTime;
    private SpriteRenderer sr;

    // Start is called before the first frame update
    void Start()
    {
        timer = new System.Diagnostics.Stopwatch();
        timer.Start();
        animationPhase = 0;
        sr = GetComponent<SpriteRenderer>();
        animationSpeed = 500;
    }

    public void snapToGrid()
    {
        Tilemap tiles = GameObject.Find("Tilemap").GetComponent<Tilemap>();
        int gridSize = tiles.size.y;
        Vector3 offset = new Vector3(tiles.cellSize.x / 2, tiles.cellSize.x / 2, -1);
        transform.position = tiles.CellToWorld(new Vector3Int(gridX - gridSize / 2, gridY - gridSize / 2, 0)) + offset;
    }

    // Update is called once per frame
    void Update()
    {
        if(timer.ElapsedMilliseconds - initialTime > animationSpeed)
        {
            //Uncomment this line once King sprites are made.
            //cycleAnimation(Direction.NONE);
            initialTime = timer.ElapsedMilliseconds;
        }

    }

    void cycleAnimation(Direction dir)
    {
        switch(animationPhase)
        {
            case 0:
                animationPhase++;
                sr.sprite = sprites[0];
                break;
            case 1:
                animationPhase++;
                sr.sprite = sprites[1];
                break;
            case 2:
                animationPhase = 0;
                sr.sprite = sprites[2];
                break;
            default:
                Debug.Log("Invalid animation phase! Phase: " + animationPhase);
                animationPhase = 0;
                break;
        }
    }
}
