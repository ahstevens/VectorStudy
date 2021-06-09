using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TabSelect : MonoBehaviour, IUpdateSelectedHandler
{
    public Selectable nextField;

    public void OnUpdateSelected(BaseEventData data)
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
            nextField.Select();
    }
}
