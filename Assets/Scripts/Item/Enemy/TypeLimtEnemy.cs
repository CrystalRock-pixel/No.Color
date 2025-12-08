using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Enemy/TypeLimit")]
public class TypeLimtEnemy : EnemyBase
{
    public enum LimitType
    {
        Struct,
        Scale,
    }
    public LimitType type;
    private ComboStruct comboStruct;
    private ComboScale comboScale;

    public override void Initialize()
    {
        base.Initialize();
        switch (type)
        {
            case LimitType.Struct:
                Array values1 = Enum.GetValues(typeof(ComboStruct));
                int randomValue1=UnityEngine.Random.Range(1,values1.Length);
                comboStruct = (ComboStruct)values1.GetValue(randomValue1);
                description = "消除 " + ScoreManager.Instance.GetComboStructName(comboStruct) + " 时，基础筹码与倍率归零";
                break;
            case LimitType.Scale:
                Array values2=Enum.GetValues(typeof(ComboScale));
                int randomValue2=UnityEngine.Random.Range(1,values2.Length);
                comboScale=(ComboScale)values2.GetValue(randomValue2);
                description = "消除 " + ScoreManager.Instance.GetComboScaleName(comboScale) + " 时，基础筹码与倍率归零";
                break;
        }
    }
    public override void SubscribeToEvents()
    {
    }

    public override void UnsubscribeFromEvents()
    {
    }
    public bool MatchCombo(ClearGeneralParameters paras)
    {
        if (comboScale == paras.comboScale || comboStruct == paras.comboStruct)
        {
            return true;
        }
        return false;
    }
}
