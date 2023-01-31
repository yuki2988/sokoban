using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MeshData", menuName = "ScriptableObjects/CreateMeshData", order = 2)]

public class MeshData : ScriptableObject
{
    [SerializeField]
    Mesh mesh01;
    public Mesh CubeMesh()
    {
        return mesh01;
    }
}
