using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllableUnit : MonoBehaviour
{
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        //signal to UI to draw selection over this unit

        //show option menu
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
