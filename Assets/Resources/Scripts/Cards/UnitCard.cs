using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unit", menuName = "Assets/Unit")]
public class UnitCard : CardSO
{
    public enum CardType
    {
        Ground,
        Flight
    };
    public CardType Type;
    public enum CardClass
    {
        Hero = 0,
        Unit = 1,
        Invention = 2
    };

    public CardClass Class;
    
    public int Hp;
    public int Damage;
    public int Speed;
    public int Range;
    public int ActionPoints;
    public string PassiveDescription;
    public string ActiveDescription;
}
