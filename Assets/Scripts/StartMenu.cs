using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class StartMenu : MonoBehaviour
{
    #region Members
    GameObject ball;
    private string x;
    private string z;
    private GameObject inputfieldX;
    private GameObject inputfieldZ;
    #endregion
    // Start is called before the first frame update


    // Update is called once per frame

    #region Methods
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.End))
        {
            // Debug.Log("end pressed");
            inputfieldX=GameObject.Find("TextInputX");
            inputfieldZ=GameObject.Find("TextInputZ");
            x = inputfieldX.GetComponent<TMP_InputField>().text;
            z =inputfieldZ.GetComponent<TMP_InputField>().text;
            SceneManager.LoadScene("Simulation");
            
            ball=GameObject.Find("ball");
            float a;
            float b;
            if (float.TryParse(x, out a) && float.TryParse(z, out b))
            {
                // Debug.Log("String conversion successful");
            Vector3 tmp=new Vector3(float.Parse(x),60,float.Parse(z));
            SingletonTransform.Instance.TransformStart = tmp;
            }
            else Debug.Log("Unsuccessful string conversion, ball is set to default position");

            // SceneManager.UnloadSceneAsync("Menu");
        }
    }
    #endregion
}
