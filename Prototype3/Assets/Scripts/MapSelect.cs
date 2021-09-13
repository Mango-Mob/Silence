using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSelect : MonoBehaviour
{
    public string m_gameSceneName = "MainGameScene";
    public string m_menuSceneName = "MainGameScene";
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartGame()
    {
        LevelLoader.instance.LoadNewLevel(m_gameSceneName);
    }
    public void QuitGame()
    {
        LevelLoader.instance.LoadNewLevel(m_menuSceneName);
    }
}
