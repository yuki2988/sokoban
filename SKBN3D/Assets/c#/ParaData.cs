using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ParametaData", menuName = "ScriptableObjects/CreateParametaData", order = 3)]
public class ParaData : ScriptableObject
{
    [SerializeField, Header("物の大きさ（比率）をいれる（基準値: １）")]
    private float _scale = default;
    public float GetScale()
    {
        return _scale;
    }

    [SerializeField, Header("TARGETのY軸の位置")]
    private float _targetY = default;
    public float GetTargetY()
    {
        return _targetY;
    }

    [SerializeField, Header("物の大きさをいれる")]
    private float _objScale = default;
    public float GetObjScale()
    {
        return _objScale;
    }

    //座標を動かす値
    private readonly int _moveValue = 1;
    public int GetConstInt()
    {
        return _moveValue;
    }

    //物体の中心
    private readonly float _objCenter = 0.5f;
    public float GetObjCenter()
    {
        return _objCenter;
    }
}
