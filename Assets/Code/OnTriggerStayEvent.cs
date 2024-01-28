using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnTriggerStayEvent : MonoBehaviour
{
    public UnityEvent<Collider> onTriggerEnter;

    void OnTriggerEnter(Collider col)
    {
        if (onTriggerEnter != null) onTriggerEnter.Invoke(col);
    }
}