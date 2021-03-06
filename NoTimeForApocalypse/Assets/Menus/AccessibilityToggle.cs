﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AccessibilityToggle : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GetComponent<Toggle>().onValueChanged.AddListener(Changed);
        GetComponent<Toggle>().isOn = StaticSafeSystem.current.accessible;
    }

	void Changed(bool value){
        StaticSafeSystem.current.SetAccessible(value);
    }
}
