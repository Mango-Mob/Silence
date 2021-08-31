using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Bar : UI_Element
{
    [Range(0.0f, 1.0f)]
    [SerializeField] private float m_value;
    [SerializeField] private Image m_barImage;

    // Update is called once per frame
    void Update()
    {
        m_barImage.fillAmount = m_value;
    }

    public void SetValue(float _value)
    {
        m_value = Mathf.Clamp(_value, 0.0f, 1.0f);
    }

    #region Parent override functions
    public override bool IsContainingVector(Vector2 _pos)
    {
        return false;
    }

    public override void OnMouseDownEvent()
    {
        //Do Nothing
    }

    public override void OnMouseUpEvent()
    {
        //Do Nothing
    }
    #endregion
}
