using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

//Michael Jordan
public class CameraManager : MonoBehaviour
{
    #region Scene_Singleton

    public static CameraManager instance = null;

    public static bool HasInstance()
    {
        return instance != null;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        if (instance == this)
        {
            InitialiseFunc();
        }
        else
        {
            Debug.LogWarning("Second Instance of CameraManager was created, this instance was destroyed.");
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    #endregion

    [SerializeField] private List<CameraDirector> m_directors;

    public void InitialiseFunc()
    {
        gameObject.name = $"Camera Manager ({gameObject.name})";
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool PlayDirector(string directorName)
    {
        if(m_directors != null && m_directors.Count != 0)
        {
            CameraDirector director = null;
            if (TryGetDirector(directorName, out director) && !IsADirectorPlaying())
            {
                director.Play();
                return true;
            }
        }
        return false;
    }

    private bool TryGetDirector(string directorName, out CameraDirector _director)
    {
        foreach (var director in m_directors)
        {
            if(director.m_name == directorName)
            {
                _director = director;
                return true;
            }
        }
        _director = null;
        return false;
    }

    public bool StopDirector(string directorName)
    {
        if (m_directors != null && m_directors.Count != 0)
        {
            CameraDirector director = null;
            if (TryGetDirector(directorName, out director))
            {
                director.Stop();
                return true;
            }
        }
        return false;
    }

    public bool IsDirectorPlaying(string directorName)
    {
        CameraDirector director = null;
        if (TryGetDirector(directorName, out director))
        {
            return director.IsPlaying();
        }
        return false;
    }

    public bool IsADirectorPlaying()
    {
        foreach (var item in m_directors)
        {
            if(item.IsPlaying())
            {
                return true;
            }
        }
        return false;
    }
}
