using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelExit : MonoBehaviour
{
    public string m_nextLevelName = "MapSelect";
    private UI_MultipleObjectives m_objectiveScript;
    private bool m_exitTriggered = false;

    // Start is called before the first frame update
    void Start()
    {
        m_objectiveScript = FindObjectOfType<UI_MultipleObjectives>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerMovement>() && m_objectiveScript.m_objectivesComplete && !m_exitTriggered)
        {
            m_exitTriggered = true;
            LevelLoader.instance.LoadNewLevel(m_nextLevelName, LevelLoader.Transition.YOUWIN);
        }
    }
}
