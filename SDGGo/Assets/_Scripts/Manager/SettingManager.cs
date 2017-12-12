using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour {

    public Toggle isBGMOpen;
    public Toggle isAIPlayerFirst;
    public Toggle isKomi;

    private void Start()
    {
        isBGMOpen.isOn = CurrentPlayer.Ins.isBGMOn;
        isAIPlayerFirst.isOn = CurrentPlayer.Ins.isAIPlayerFisrt;
        isKomi.isOn = CurrentPlayer.Ins.isKomi;
    }

    // Update is called once per frame
    void Update () {
        CurrentPlayer.Ins.isBGMOn = isBGMOpen.isOn;
        CurrentPlayer.Ins.isAIPlayerFisrt = isAIPlayerFirst.isOn;
        CurrentPlayer.Ins.isKomi = isKomi.isOn;
    }
    public void Back() {
        GoSceneManager.Ins.EnterLoadingScene("Menu");
    }
}
