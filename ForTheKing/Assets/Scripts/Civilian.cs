using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Civilian : MonoBehaviour
{
    public int gridX = 0;
    public int gridY = 0;
    public bool isAlive = true;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    public void snapToGrid()
    {
        Tilemap tiles = FindObjectOfType<Tilemap>();
        int gridSize = tiles.size.y;
        Vector3 offset = new Vector3(tiles.cellSize.x / 2, tiles.cellSize.x / 2, -1);
        transform.position = tiles.CellToWorld(new Vector3Int(gridX - gridSize / 2, gridY - gridSize / 2, 0)) + offset;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        //signal to UI to draw selection over this unit
        GameObject.Find("BattleManager").GetComponent<BattleUI>().onUnitClick(gameObject);

    }
}
