using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CellDescription : MonoBehaviour
{
    public TMP_Text text;
    public void Init(string text)
    {
        this.text.text=text;
    }
}
