using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideWebGL : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
#if UNITY_WEBGL
        gameObject.SetActive(false);
#endif
    }

    // Update is called once per frame
    void Update()
    {

    }
}
