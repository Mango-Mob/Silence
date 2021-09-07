using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static NoiseManager;

public class NoiseListener
{
    public NoiseListener(GameObject _owner, float _range, LayerMask _recieverLayer)
    {
        owner = _owner;
        newLocations = new List<Vector3>();
        range = _range;
        recieverLayer = _recieverLayer;
    }

    public void Notify(Noise noise)
    {
        if (recieverLayer == (recieverLayer | (1 << noise.m_layer)))
        {
            newLocations.Add(noise.m_position);
        }
    }

    public void Clear()
    {
        newLocations.Clear();
    }

    public float range;
    public GameObject owner;
    public LayerMask recieverLayer;
    public List<Vector3> newLocations;
}
