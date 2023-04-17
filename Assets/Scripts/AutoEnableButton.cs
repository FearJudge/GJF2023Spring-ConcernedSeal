using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoEnableButton : MonoBehaviour
{
    public UnityEngine.UI.Button automaticButton;

    void OnEnable()
    {
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(automaticButton.gameObject);
    }
}
