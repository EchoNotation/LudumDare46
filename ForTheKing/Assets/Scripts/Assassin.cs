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

    // Start is called before the first frame update
    void Start()
    {
        king = GameObject.FindGameObjectWithTag("King");
        ui = FindObjectOfType<BattleUI>();
    }

    public void snapToGrid()
    {
        Tilemap tiles = GameObject.Find("Tilemap").GetComponent<Tilemap>();
        int gridSize = tiles.size.y;
        Vector3 offset = new Vector3(tiles.cellSize.x / 2, tiles.cellSize.x / 2, -1);
        transform.position = tiles.CellToWorld(new Vector3Int(gridX - gridSize / 2, gridY - gridSize / 2, -1)) + offset;
    }

    private void OnMouseDown()
    {
        ui.onAssassinClick(gameObject);
    }
}
