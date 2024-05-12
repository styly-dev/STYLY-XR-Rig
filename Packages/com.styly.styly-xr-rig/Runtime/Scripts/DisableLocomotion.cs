using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableLocomotion : MonoBehaviour
{
    [SerializeField]
    private GameObject locomotionObject;

    void Start()
    {
        locomotionObject.SetActive(false);
    }
}
