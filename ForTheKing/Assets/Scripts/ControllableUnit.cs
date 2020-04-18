using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ControllableUnit : MonoBehaviour
{

    public int gridX = 0;
    public int gridY = 0;

    public bool hasMoved = false;
    public bool hasTakenAction = false;
    public int unitID;

    public enum UnitType
    {
        KNIGHT,
        JESTER,
        NOBLE
    }

    public UnitType unitType;
    public Sprite knightSprite, jesterSprite, nobleSprite;
    private int speed;

    // Start is called before the first frame update
    void Start()
    {
        switch(unitType)
        {
            case UnitType.KNIGHT:
                speed = 1;
                this.GetComponent<SpriteRenderer>().sprite = knightSprite;
                break;
            case UnitType.JESTER:
                this.GetComponent<SpriteRenderer>().sprite = jesterSprite;
                speed = 3;
                break;
            case UnitType.NOBLE:
                this.GetComponent<SpriteRenderer>().sprite = nobleSprite;
                speed = 2;
                break;
        }

        ////align the unit with the tile on Start
        Tilemap tiles = FindObjectOfType<Tilemap>();
        int gridSize = tiles.size.y;
        Vector3 offset = new Vector3(tiles.cellSize.x / 2, tiles.cellSize.x / 2, 0);
        transform.position = tiles.CellToWorld(new Vector3Int(gridX - gridSize/2, gridY - gridSize/2, 0)) + offset;
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

    public UnitType getUnitType()
    {
        return unitType;
    }

    public int getSpeed()
    {
        return speed;
    }
}
