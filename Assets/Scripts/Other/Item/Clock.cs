using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Clock : MonoBehaviour
{
    [SerializeField] private Image hourHand;   // 时针
    [SerializeField] private Image minuteHand; // 分针

    [SerializeField] private float timeMultiplier = 1f; // 时间倍速，1为真实时间

    void Update()
    {
        float totalSeconds = Time.time * timeMultiplier;

        if (minuteHand != null)
        {
            // 分针：每分钟6度
            float minuteRotation = -(totalSeconds * 6f / 60f); // 每秒0.1度
            minuteHand.rectTransform.rotation = Quaternion.Euler(0, 0, minuteRotation);
        }

        if (hourHand != null)
        {
            // 时针：每小时30度，受分针影响
            float hourRotation = -(totalSeconds * 30f / 3600f); // 每秒0.00833度
            hourHand.rectTransform.rotation = Quaternion.Euler(0, 0, hourRotation);
        }
    }
}
