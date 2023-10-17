using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class CardSO : ScriptableObject
{
    public string Name;
    public int ManaCost;
    public enum CardRace
    {
        Human,
        MEcha,
        Nature,
        Power_of_Light,
        Demons
    };
    public CardRace race;
    public string[] Affiliations;
    public string Description;
}
