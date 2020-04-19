using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Assassin : MonoBehaviour
{
    public int gridX = 0;
    public int gridY = 0;
    private GameObject king;

    public int speed = 2;

    public Vector2Int[] desiredPath;
    public Vector2Int[] nextTurnPath;

    // Start is called before the first frame update
    void Start()
    {
        king = GameObject.FindGameObjectWithTag("King");
    }

    public void snapToGrid()
    {
        Tilemap tiles = FindObjectOfType<Tilemap>();
        int gridSize = tiles.size.y;
        Vector3 offset = new Vector3(tiles.cellSize.x / 2, tiles.cellSize.x / 2, -1);
        transform.position = tiles.CellToWorld(new Vector3Int(gridX - gridSize / 2, gridY - gridSize / 2, -1)) + offset;
    }

    // Update is called once per frame
    void Update()
    {
        //RaycastHit2D hit = shoot(king);
        //Debug.Log(hit.collider.tag);
    }

    public RaycastHit2D shoot(GameObject victim)
    {
        Vector2 origin = new Vector2(transform.position.x, transform.position.y);
        Vector2 destination = new Vector2(victim.transform.position.x, victim.transform.position.y);
        return Physics2D.Raycast(origin, destination - origin);
    }
}
