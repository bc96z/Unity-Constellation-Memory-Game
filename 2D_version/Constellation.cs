using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewConstellation", menuName = "StarData/Constellation")]
public class Constellation : ScriptableObject
{
    public string constellationName;
    public List<Star> stars = new List<Star>();
    public List<Edge> edges;
}
