using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ParametaData", menuName = "ScriptableObjects/CreateParametaData", order = 3)]
public class ParaData : ScriptableObject
{
    [SerializeField, Header("���̑傫���i�䗦�j�������i��l: �P�j")]
    private float _scale = default;
    public float GetScale()
    {
        return _scale;
    }

    [SerializeField, Header("TARGET��Y���̈ʒu")]
    private float _targetY = default;
    public float GetTargetY()
    {
        return _targetY;
    }

    [SerializeField, Header("���̑傫���������")]
    private float _objScale = default;
    public float GetObjScale()
    {
        return _objScale;
    }

    //���W�𓮂����l
    private readonly int _moveValue = 1;
    public int GetConstInt()
    {
        return _moveValue;
    }

    //���̂̒��S
    private readonly float _objCenter = 0.5f;
    public float GetObjCenter()
    {
        return _objCenter;
    }
}
