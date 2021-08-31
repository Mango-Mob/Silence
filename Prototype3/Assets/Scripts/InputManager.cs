using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Michael Jordan
/// </summary>
/// 

#region InputEnums
public enum KeyType
{
    Q, W, E, R, T, Y, U, I, O, P,
    A, S, D, F, G, H, J, K, L,
    Z, X, C, V, B, N, M,
    ALP_ONE, ALP_TWO, ALP_THREE, ALP_FOUR, ALP_FIVE, ALP_SIX, ALP_SEVEN, ALP_EIGHT, ALP_NINE, ALP_ZERO,
    NUM_ONE, NUM_TWO, NUM_THREE, NUM_FOUR, NUM_FIVE, NUM_SIX, NUM_SEVEN, NUM_EIGHT, NUM_NINE, NUM_ZERO,
    L_SHIFT, L_CTRL, L_ALT, TAB, ESC, SPACE,
    R_SHIFT, R_CTRL, R_ALT, ENTER,
}
public enum MouseButton
{
    LEFT, MIDDLE, RIGHT
}
public enum ButtonType
{
    NORTH, SOUTH, EAST, WEST,
    START, SELECT, 
    LT, LB, LS, RT, RB, RS,
}

public enum StickType
{
    LEFT,
    RIGHT
}

#endregion

public class InputManager : MonoBehaviour
{
    #region Singleton

    private static InputManager _instance = null;
    public static InputManager instance 
    {
        get 
        {
            if (_instance == null)
            {
                GameObject loader = new GameObject();
                _instance = loader.AddComponent<InputManager>();
                loader.name = "Input Manager";
                return loader.GetComponent<InputManager>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        if (_instance == this)
        {
            InitialFunc();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("Second Instance of InputManager was created, this instance was destroyed.");
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }
    #endregion

    protected Mouse mouse;
    protected Keyboard keyboard = Keyboard.current;
    protected int gamepadCount;
    private void InitialFunc()
    {
        mouse = Mouse.current;
        Debug.Log($"{mouse.displayName} has connected as a PRIMARY_MOUSE to the InputManager.");
        keyboard = Keyboard.current;
        Debug.Log($"{keyboard.displayName} has connected as a PRIMARY_KEYBOARD to the InputManager.");
        gamepadCount = Gamepad.all.Count;

        if(gamepadCount > 0)
        {
            for (int i = 0; i < gamepadCount; i++)
            {
                Debug.Log($"{Gamepad.all[i].displayName} has connected as a GAMEPAD (ID: {i}) to the InputManager.");
            }
        }
    }

    private void Update()
    {
        if(gamepadCount < Gamepad.all.Count)
        {
            for (int i = gamepadCount; i < Gamepad.all.Count; i++)
            {
                Debug.Log($"{Gamepad.all[i].displayName} has connected as a GAMEPAD (ID: {i}) to the InputManager.");
            }
            gamepadCount = Gamepad.all.Count;
        }
    }

    public int GetAnyGamePad()
    {
        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            if(IsAnyGamePadInput(i))
            {
                return i;
            }
        }
        return -1;
    }

    public bool IsAnyGamePadInput(int padID)
    {
        for (int i = 0; i < Enum.GetNames(typeof(ButtonType)).Length; i++)
        {
            if (IsGamepadButtonPressed((ButtonType)i, padID))
            {
                return true;
            }
        }

        for (int j = 0; j < Enum.GetNames(typeof(StickType)).Length; j++)
        {
            if (GetGamepadStick((StickType)j, padID) != Vector2.zero)
            {
                return true;
            }
        }

        return false;
    }

    public ButtonControl GetGamepadButton(ButtonType button, int padID)
    {
        Gamepad pad = (padID < 0 || padID >= Gamepad.all.Count) ? Gamepad.current : Gamepad.all[padID];
        
        if (pad == null)
            return null;

        switch (button)
        {
            case ButtonType.NORTH:     { return pad.buttonNorth; }
            case ButtonType.SOUTH:     { return pad.buttonSouth; }
            case ButtonType.EAST:      { return pad.buttonEast; }
            case ButtonType.WEST:      { return pad.buttonWest; }
            case ButtonType.START:     { return pad.startButton; }
            case ButtonType.SELECT:    { return pad.selectButton; }
            case ButtonType.LT:        { return pad.leftTrigger; }
            case ButtonType.LB:        { return pad.leftShoulder; }
            case ButtonType.LS:        { return pad.leftStickButton; }
            case ButtonType.RT:        { return pad.rightTrigger; }
            case ButtonType.RB:        { return pad.rightShoulder; }
            case ButtonType.RS:        { return pad.rightStickButton; }

            default:
                return null;
        }
    }

    public Vector2 GetGamepadStick(StickType stick, int padID)
    {
        Gamepad pad = (padID < 0 || padID >= Gamepad.all.Count) ? Gamepad.current : Gamepad.all[padID];

        if (pad == null)
            return Vector2.zero;

        switch (stick)
        {
            case StickType.LEFT: { return new Vector2(pad.leftStick.x.ReadValue(), pad.leftStick.y.ReadValue()); }
            case StickType.RIGHT: { return new Vector2(pad.rightStick.x.ReadValue(), pad.rightStick.y.ReadValue()); }

            default:
                return Vector2.zero;
        }
    }

    public bool IsGamepadButtonDown(ButtonType button, int padID)
    {
        ButtonControl control = GetGamepadButton(button, padID);

        if (control == null)
            return false;

        return control.wasPressedThisFrame;
    }

    public bool IsGamepadButtonUp(ButtonType button, int padID)
    {
        ButtonControl control = GetGamepadButton(button, padID);

        if (control == null)
            return false;

        return control.wasReleasedThisFrame;
    }

    public bool IsGamepadButtonPressed(ButtonType button, int padID)
    {
        ButtonControl control = GetGamepadButton(button, padID);

        if (control == null)
            return false;

        return control.isPressed;
    }

    public bool IsGamepadButtonReleased(ButtonType button, int padID)
    {
        ButtonControl control = GetGamepadButton(button, padID);

        if (control == null)
            return false;

        return !(control.isPressed);
    }

    public bool IsAnyKeyPressed()
    {
        return keyboard.anyKey.isPressed;
    }
    public bool IsAnyKeyDown()
    {
        foreach (var keyType in Enum.GetValues(typeof(KeyType)).Cast<KeyType>())
        {
            if(IsKeyDown(keyType))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsAnyMouseButtonDown()
    {
        foreach (var mouseButton in Enum.GetValues(typeof(MouseButton)).Cast<MouseButton>())
        {
            if (GetMouseButtonDown(mouseButton))
            {
                return true;
            }
        }
        return false;
    }

    public KeyControl GetKey(KeyType key)
    {
        switch (key)
        {
            case KeyType.Q: { return keyboard.qKey; }
            case KeyType.W: { return keyboard.wKey; }
            case KeyType.E: { return keyboard.eKey; }
            case KeyType.R: { return keyboard.rKey; }
            case KeyType.T: { return keyboard.tKey; }
            case KeyType.Y: { return keyboard.yKey; }
            case KeyType.U: { return keyboard.uKey; }
            case KeyType.I: { return keyboard.iKey; }
            case KeyType.O: { return keyboard.oKey; }
            case KeyType.P: { return keyboard.pKey; }

            case KeyType.A: { return keyboard.aKey; }
            case KeyType.S: { return keyboard.sKey; }
            case KeyType.D: { return keyboard.dKey; }
            case KeyType.F: { return keyboard.fKey; }
            case KeyType.G: { return keyboard.gKey; }
            case KeyType.H: { return keyboard.hKey; }
            case KeyType.J: { return keyboard.jKey; }
            case KeyType.K: { return keyboard.kKey; }
            case KeyType.L: { return keyboard.lKey; }

            case KeyType.Z: { return keyboard.zKey; }
            case KeyType.X: { return keyboard.xKey; }
            case KeyType.C: { return keyboard.cKey; }
            case KeyType.V: { return keyboard.vKey; }
            case KeyType.B: { return keyboard.bKey; }
            case KeyType.N: { return keyboard.nKey; }
            case KeyType.M: { return keyboard.mKey; }

            case KeyType.NUM_ONE:     { return keyboard.numpad1Key; }
            case KeyType.NUM_TWO:     { return keyboard.numpad2Key; }
            case KeyType.NUM_THREE:   { return keyboard.numpad3Key; }
            case KeyType.NUM_FOUR:    { return keyboard.numpad4Key; }
            case KeyType.NUM_FIVE:    { return keyboard.numpad5Key; }
            case KeyType.NUM_SIX:     { return keyboard.numpad6Key; }
            case KeyType.NUM_SEVEN:   { return keyboard.numpad7Key; }
            case KeyType.NUM_EIGHT:   { return keyboard.numpad8Key; }
            case KeyType.NUM_NINE:    { return keyboard.numpad9Key; }
            case KeyType.NUM_ZERO: { return keyboard.numpad0Key; }

            case KeyType.ALP_ONE: { return keyboard.digit1Key; }
            case KeyType.ALP_TWO: { return keyboard.digit2Key; }
            case KeyType.ALP_THREE: { return keyboard.digit3Key; }
            case KeyType.ALP_FOUR: { return keyboard.digit4Key; }
            case KeyType.ALP_FIVE: { return keyboard.digit5Key; }
            case KeyType.ALP_SIX: { return keyboard.digit6Key; }
            case KeyType.ALP_SEVEN: { return keyboard.digit7Key; }
            case KeyType.ALP_EIGHT: { return keyboard.digit8Key; }
            case KeyType.ALP_NINE: { return keyboard.digit9Key; }
            case KeyType.ALP_ZERO: { return keyboard.digit0Key; }

            case KeyType.L_SHIFT: { return keyboard.leftShiftKey; }
            case KeyType.L_CTRL: { return keyboard.leftCtrlKey; }
            case KeyType.L_ALT: { return keyboard.leftAltKey; }
            case KeyType.TAB: { return keyboard.tabKey; }
            case KeyType.ESC: { return keyboard.escapeKey; }
            case KeyType.SPACE: { return keyboard.spaceKey; }

            case KeyType.R_SHIFT: { return keyboard.rightShiftKey; }
            case KeyType.R_CTRL: { return keyboard.rightCtrlKey; }
            case KeyType.R_ALT: { return keyboard.rightAltKey; }
            case KeyType.ENTER: { return keyboard.endKey; }

            default:
                return null;
        }
    }

    public bool IsKeyDown(KeyType key)
    {
        return GetKey(key).wasPressedThisFrame;
    }

    public bool IsKeyUp(KeyType key)
    {
        return GetKey(key).wasReleasedThisFrame;
    }

    public bool IsKeyPressed(KeyType key)
    {
        return GetKey(key).isPressed;
    }
    public bool IsKeyReleased(KeyType key)
    {
        return !(GetKey(key).isPressed);
    }

    public bool IsAnyMousePressed()
    {
        return mouse.press.isPressed;
    }

    public ButtonControl GetMouseButton(MouseButton button)
    {
        switch (button)
        {
            case MouseButton.LEFT:      { return mouse.leftButton; }
            case MouseButton.MIDDLE:    { return mouse.middleButton; }
            case MouseButton.RIGHT:     { return mouse.rightButton; }
           
            default:
                return null;
        }
    }

    public bool GetMouseButtonDown(MouseButton button)
    {
        return GetMouseButton(button).wasPressedThisFrame;
    }

    public bool GetMouseButtonUp(MouseButton button)
    {
        return GetMouseButton(button).wasReleasedThisFrame;
    }

    public bool GetMouseButtonPressed(MouseButton button)
    {
        return GetMouseButton(button).isPressed;
    }

    public bool GetMouseButtonReleased(MouseButton button)
    {
        return !(GetMouseButton(button).isPressed);
    }

    /*
    * GetMousePositionInScreen by Michael Jordan
    * Description:
    *  Returns the position of the mouse where 0,0 is the bottem left of the screen.
    *
    * Return: 
    *  Vector2 - Mouse position
    */
    public Vector2 GetMousePositionInScreen()
    {
        //Convert to 0,0 middle
        //Vector3 screenPoint = new Vector2(mouse.position.x.ReadValue(), mouse.position.y.ReadValue());
        //new Vector2(screenPoint.x - Camera.main.scaledPixelWidth / 2, screenPoint.y - Camera.main.scaledPixelHeight / 2);
        
        return new Vector2(mouse.position.x.ReadValue(), mouse.position.y.ReadValue());
    }

    public Vector2 GetMouseDelta()
    {
        return new Vector2(mouse.delta.x.ReadValue(), mouse.delta.y.ReadValue());
    }

    public float GetMouseScrollDelta()
    {
        return mouse.scroll.y.ReadValue();
    }

    /// <summary>
    /// Checking if the mouse button is down
    /// </summary>
    /// <param name="button"></param>
    /// <returns></returns>
    public bool GetMouseDown(MouseButton button)
    {
        switch (button)
        {
            default:
                Debug.LogWarning($"Unsupported mouse button type in GetMouseDown.");
                return false;
            case MouseButton.LEFT:
                return mouse.rightButton.wasPressedThisFrame;

            case MouseButton.RIGHT:
                return mouse.leftButton.wasPressedThisFrame;
        }

    }
    /// <summary>
    /// Checking if the mouse button is pressed
    /// </summary>
    /// <param name="button"></param>
    /// <returns></returns>
    public bool GetMousePress(MouseButton button)
    {

        switch (button)
        {
            default:
                Debug.LogWarning($"Unsupported mouse button type in GetMouseDown.");
                return false;
            case MouseButton.LEFT:
                return mouse.rightButton.isPressed;

            case MouseButton.RIGHT:
                return mouse.leftButton.isPressed;
        }
    }
}
