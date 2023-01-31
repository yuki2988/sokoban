using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMovement : MonoBehaviour
{
    [SerializeField]
    private string scene = default;
    // Update is called once per frame
    void Update()
    {
        if (Input.anyKey)
        {
            SceneMove();
        }
    }
    public void SceneMove()
    {
        SceneManager.LoadScene(scene);
    }
}
