using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellConfig
{
    public ColorType colorType;
    public string scriptTypeName; // 存储组件的类名，例如 "ColorCell" 或 "SpecialBombCell"

    // 如果你有特殊属性，也可以存在这里
    public float powerMultiplier = 1f;

    public CellConfig(ColorType color, string typeName = "ColorCell")
    {
        this.colorType = color;
        this.scriptTypeName = typeName;
    }
    public CellConfig(ColorCell source)
    {
        if (source == null) return;
        this.colorType = source.colorType;
        this.scriptTypeName = source.GetType().Name;
    }
}

[CreateAssetMenu(fileName = "CellAssemblyConfig", menuName = "Config/CellAssemblyConfig", order = 1)]
public class CellAssemblyConfig : ScriptableObject
{
    [Serializable]
    public struct CellColorSpritePair
    {
        public ColorType colorType;
        public Sprite sprite;
    }

    [Serializable]
    public struct CellTypeSpritePair
    {
        public string typeName;
        public Sprite sprite;
    }


    public List<CellColorSpritePair> cellColorSpritePairs;
    public List<CellTypeSpritePair> cellTypeSpritePairs;

    public Sprite GetColorSprite(ColorType colorType)
    {
        foreach (var pair in cellColorSpritePairs)
        {
            if (pair.colorType == colorType)
            {
                return pair.sprite;
            }
        }
        return null;
    }

    public Sprite GetTypeSprite(string typeName)
    {
        foreach (var pair in cellTypeSpritePairs)
        {
            if (pair.typeName == typeName)
            {
                return pair.sprite;
            }
        }
        return null;
    }
}
