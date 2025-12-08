using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalRelicCard : RelicCardData
{
    public ComboScale comboScale;
    public ComboStruct comboStruct;
    public ColorType color;
    protected ClearGeneralParameters paras;

    /// <summary>
    /// 是否无条件限制
    /// </summary>
    protected bool isUnrestricted => comboScale == ComboScale.None && comboStruct == ComboStruct.None && color == ColorType.None;

    /// <summary>
    /// 是否满足条件
    /// </summary>
    protected bool constraintsMet =>(comboScale == ComboScale.None || comboScale == paras.comboScale) &&(comboStruct == ComboStruct.None || comboStruct == paras.comboStruct) &&
        (color == ColorType.None || color == paras.colorType);


}
