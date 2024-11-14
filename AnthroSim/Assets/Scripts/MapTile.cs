using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile : MonoBehaviour
{
    [SerializeField]
    MeshFilter _planeMesh;

    public MeshFilter PlaneMesh { get { return _planeMesh; } set { _planeMesh = value; } }
}
