﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ReloadButton : MonoBehaviour {

    public string scene;

    // Use this for initialization
    void Start () {
        GetComponent<Button>().onClick.AddListener(OnClick);
	}

    void OnClick() {
        GameObject menu = GameObject.FindWithTag("PauseMenu");
        if (menu) menu.GetComponent<PauseMenu>().Unpause();
        if (LoadingScreen.current) LoadingScreen.current.StartLoading();

        SceneManager.LoadScene(scene==""?SceneManager.GetActiveScene().name:scene);
    }
}
