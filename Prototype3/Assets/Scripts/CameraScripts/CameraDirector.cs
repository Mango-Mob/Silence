using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

class CameraDirector : MonoBehaviour
{
    public string m_name { get; private set; }
    private PlayableDirector directorAsset;

    private void Start()
    {
        directorAsset = GetComponent<PlayableDirector>();
        m_name = GetComponent<PlayableDirector>().playableAsset.name;
    }

    public void Play()
    {
        directorAsset.Play();
    }

    public void Stop()
    {
        directorAsset.Stop();
    }

    public bool IsPlaying()
    {
        return directorAsset.state == PlayState.Playing;
    }
}
