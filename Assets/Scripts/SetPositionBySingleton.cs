using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPositionBySingleton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (SingletonTransform.Instance.TransformStart!=null)
            transform.SetPositionAndRotation((Vector3)SingletonTransform.Instance.TransformStart,Quaternion.identity);
    }
}
