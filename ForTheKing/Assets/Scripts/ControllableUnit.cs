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
        Vector3 offset = new Vector3(tiles.cellSize.x / 2, tiles.cellSize.x / 2, -1);
        transform.position = tiles.CellToWorld(new Vector3Int(gridX - gridSize/2, gridY - gridSize/2, 0)) + offset;
    }

    public void updateMoveablePositions()
    {
        moveablePositions.Clear();

        BattleManager bmanager = FindObjectOfType<BattleManager>();

        for(int i = -speed; i <= speed; i++)
        {
            for(int j = -speed; j <= speed; j++)
            {
                //gridX to tileSpace
                 
                Vector2Int offset = new Vector2Int(i, j);
                Vector2Int current = boardToTileSpace(gridX, gridY);
                Vector2Int dest = current + offset;
                Debug.Log("testing to " + dest + "an offset of " + offset);
                int dist = bmanager.pathDistance(current, dest);
                if (dist == -1) continue;
                if (true /* dist <= speed */)
                {
                    moveablePositions.Add(dest);
                    Debug.Log("can move to " + dest);
                }
            }
        }

        Debug.Log("Found " + moveablePositions.Count + " spaces to move to");
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

