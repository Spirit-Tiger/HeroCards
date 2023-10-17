using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitsList",menuName = "Assets/UnitsList")]
public class UnitsList : ScriptableObject
{
   public List<GameObject> AllUnitsList = new List<GameObject>();
}
