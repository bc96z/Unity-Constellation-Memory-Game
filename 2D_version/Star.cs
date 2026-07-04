using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Edge
{
    public int starIndex1; // 第一顆星的索引
    public int starIndex2; // 第二顆星的索引
}
[System.Serializable]
public class Star
{
    public string name;
    public Vector3 position;
}


