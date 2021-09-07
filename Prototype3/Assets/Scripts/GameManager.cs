using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Application_Singleton

    private static GameManager _instance = null;
    public static GameManager instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject loader = new GameObject();
                _instance = loader.AddComponent<GameManager>();
                return loader.GetComponent<GameManager>();
            }
            return _instance;
        }
    }

    public static bool HasInstance()
    {
        return _instance != null;
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        if (_instance == this)
        {
            InitialiseFunc();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("Second Instance of GameManager was created, this instance was destroyed.");
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    #endregion

    public static Vector2 m_sensitivity = new Vector2(-400.0f, -250.0f);

    public bool useGamepad = false;
    public float lootValue = 0.0f;
    internal bool enableTimer = true; // temp true

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        int gamepadID = InputManager.instance.GetAnyGamePad();
        if (InputManager.instance.IsAnyKeyDown() || InputManager.instance.IsAnyMouseButtonDown())
        {
            useGamepad = false;
        }
        if (InputManager.instance.IsAnyGamePadInput(gamepadID))
        {
            useGamepad = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //float currentTime = Time.timeScale;
        //currentTime += Time.deltaTime;
        //currentTime = Mathf.Clamp(currentTime, 0.0f, 1.0f);
        //Time.timeScale = currentTime;

        int gamepadID = InputManager.instance.GetAnyGamePad();
        if (InputManager.instance.IsAnyKeyDown() || InputManager.instance.IsAnyMouseButtonDown())
        {
            useGamepad = false;
        }
        if (InputManager.instance.IsAnyGamePadInput(gamepadID))
        {
            useGamepad = true;
        }
    }

    private void InitialiseFunc()
    {
        gameObject.name = "Game Manager";
    }
    private void TimeUpdate()
    {

    }
    public void SlowTime(float _percentage, float _duration)
    {
        StartCoroutine(SlowTimeRoutine(_percentage, _duration));
    }
    private IEnumerator SlowTimeRoutine(float _percentage, float _duration)
    {
        
        Time.timeScale = _percentage;
        AudioManager.instance.m_globalPitch = Time.timeScale;
        Time.fixedDeltaTime = 0.02f * _percentage;
        float rate = _duration / (1f - _percentage);
        while (_duration > 0)
        {
            _duration -= Time.unscaledDeltaTime;
            yield return new WaitForEndOfFrame();
            AudioManager.instance.m_globalPitch = Time.timeScale;
            Time.timeScale += rate * Time.unscaledDeltaTime;
            Time.fixedDeltaTime = 0.02f * _percentage;
        }
        Time.timeScale = 1.0f;
        AudioManager.instance.m_globalPitch = Time.timeScale;
        Time.fixedDeltaTime = 0.02f;
        yield return null;
    }
}
