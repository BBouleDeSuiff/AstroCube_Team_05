using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    [SerializeField] bool PToReturnToMenu = true;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && PToReturnToMenu)
        {
            //Cursor.lockState = CursorLockMode.None;
            EventManager.TriggerSceneEnd();
            SceneManager.LoadScene(0);
        }
    }
}
