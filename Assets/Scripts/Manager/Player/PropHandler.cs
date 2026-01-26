using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropHandler : MonoBehaviour
{
    public Player player => Player.Instance;
    public List<UIProp> props = new List<UIProp>();

    public void AddProp(UIProp prop)
    {
        props.Add(prop);
    }
    public void RemoveProp(UIProp prop)
    {
        props.Remove(prop);
    }

}
