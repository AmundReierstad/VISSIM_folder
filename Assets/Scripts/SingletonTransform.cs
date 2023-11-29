using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonTransform : MonoBehaviour
    //Singleton to store transform when transitioning scenes
{
    public static SingletonTransform Instance;
   [SerializeField] public Nullable<Vector3> TransformStart;
    
    // Start is called before the first frame update
    private void Awake()
    {
        CreateSingleton();
        TransformStart = null;
    }

    void CreateSingleton()
    {
        if (!Instance)
            Instance=this;
        else Destroy(gameObject);
        
        DontDestroyOnLoad(gameObject);
    }
}
