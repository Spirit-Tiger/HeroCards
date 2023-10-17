using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Deck", menuName = "Assets/Deck")]
public class DeckSO : ScriptableObject
{
   public List<GameObject> playerDeck = new List<GameObject>();
}
