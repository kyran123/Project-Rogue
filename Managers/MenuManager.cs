using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour {

    public GameObject MenuScreen;
    public GameObject LossScreen;

    void Update() {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            MenuScreen.SetActive(!MenuScreen.activeSelf);
        }
    }

    public void Quit() {
        Application.Quit();
    }

}
