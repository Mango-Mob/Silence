using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public abstract class UI_Element : MonoBehaviour
{
    public abstract bool IsContainingVector(Vector2 _pos);
    public abstract void OnMouseDownEvent();
    public abstract void OnMouseUpEvent();
}
