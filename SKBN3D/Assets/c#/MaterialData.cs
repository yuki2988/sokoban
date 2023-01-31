using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MaterialData", menuName = "ScriptableObjects/CreateMaterialData", order = 1)]
public class MaterialData : ScriptableObject
{
    [SerializeField]
    private Material material01;
    public Material WallMaterial()
    {
        return material01;
    }
    [SerializeField]
    private Material material02;
    public Material GroundMaterial()
    {
        return material02;
    }
    [SerializeField]
    private Material material03;
    public Material TargetMaterial()
    {
        return material03;
    }
    [SerializeField]
    private Material material04;
    public Material PlayerMaterial()
    {
        return material04;
    }
    [SerializeField]
    private Material material05;
    public Material BlockMaterial()
    {
        return material05;
    }
}
