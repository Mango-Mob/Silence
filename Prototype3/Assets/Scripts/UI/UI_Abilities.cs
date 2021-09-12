using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Abilities : UI_Element
{
    public CanvasGroup m_canvasGroup;
    public float m_fadeSpeed = 0.2f;
    public float m_alphaMin = 0.3f;

    [Header("Cooldown Sprites")]
    public Image m_headCD;
    public Image m_armCD;

    [Header("Duration Sprites")]
    public Image m_headDuration;

    [Header("Ability Sprites")]
    public Image m_head;
    public Image m_arm;
    public Image m_legs;

    public PlayerMovement m_playerMovement;

    // Start is called before the first frame update
    void Start()
    {
        m_head.enabled = (m_head.sprite != null);
        m_arm.enabled = (m_arm.sprite != null);
        m_legs.enabled = (m_legs.sprite != null);
    }

    // Update is called once per frame
    void Update()
    {

        if (m_playerMovement.m_invisibilityTimer > 0.0f || 
            m_playerMovement.m_invisibilityCDTimer > 0.0f || 
            m_playerMovement.m_grappleCDTimer > 0.0f)
        {
            m_canvasGroup.alpha = 1.0f;
        }
        else
        {
            float newAlpha = m_canvasGroup.alpha - Time.deltaTime * m_fadeSpeed;
            newAlpha = Mathf.Clamp(newAlpha, m_alphaMin, 1.0f);
            m_canvasGroup.alpha = newAlpha;
        }

        m_headDuration.fillAmount = m_playerMovement.m_invisibilityTimer / m_playerMovement.m_invisibilityDuration;
        m_headCD.fillAmount = m_playerMovement.m_invisibilityCDTimer / m_playerMovement.m_invisibilityCD;
        m_armCD.fillAmount = m_playerMovement.m_grappleCDTimer / m_playerMovement.m_grappleCD;
    }

    public void SetHeadSprite(Sprite _sprite)
    {
        m_head.sprite = _sprite;
        m_head.enabled = (m_head.sprite != null);
        m_canvasGroup.alpha = 1.0f;
    }
    public void SetArmSprite(Sprite _sprite)
    {
        m_arm.sprite = _sprite;
        m_arm.enabled = (m_arm.sprite != null);
        m_canvasGroup.alpha = 1.0f;
    }
    public void SetLegsSprite(Sprite _sprite)
    {
        m_legs.sprite = _sprite;
        m_legs.enabled = (m_legs.sprite != null);
        m_canvasGroup.alpha = 1.0f;
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
