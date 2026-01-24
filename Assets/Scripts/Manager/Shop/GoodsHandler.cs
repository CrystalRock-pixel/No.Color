using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGood
{
    int Price { get; }
    int SellPrice { get; }
    Transform Transform { get; }

    void OnBuy();
    void OnSell();
}

    public class GoodsHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
