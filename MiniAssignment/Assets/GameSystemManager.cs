using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSystemManager : MonoBehaviour
{

    GameObject inputFieldUserName, inputFieldPassword, buttonSubmit, toggleLogin, toggleCreate;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] allObjects = Unity.Object.FindObjectsOfType<GameObject>();

        foreach (GameObject go in allObjects)
        {
            if(go.name == "InputFieldUserName")
                InputFieldUserName = go;
            else if(go.name == "InputFieldPassword")
                InputFieldPassword = go;
            else if(go.name == "buttonSubmit")
                buttonSubmit = go;
            else if(go.name == "toggleLogin")
                toggleLogin = go;
            else if(go.name == "toggleCreate")
                toggleCreate = go
        }

    buttonSubmit.GetComponent
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
