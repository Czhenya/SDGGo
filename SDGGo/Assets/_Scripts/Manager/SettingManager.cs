using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour {

    public Toggle isBGMOpen;
    public Toggle isAIPlayerFirst;
    public Toggle isKomi;
    public Text textLevel;

    private void Start()
    {
        isBGMOpen.isOn = CurrentPlayer.Ins.isBGMOn;
        isAIPlayerFirst.isOn = CurrentPlayer.Ins.isAIPlayerFisrt;
        isKomi.isOn = CurrentPlayer.Ins.isKomi;
        UpdateLevel();
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

    public void AddLevel() {
        int curlevel = CurrentPlayer.Ins.curLevel;
        if (curlevel >= 20) return;
        ++CurrentPlayer.Ins.curLevel;
        UpdateLevel();
    }
    public void SubLevel() {
        int curlevel = CurrentPlayer.Ins.curLevel;
        if (curlevel <= -20) return;
        --CurrentPlayer.Ins.curLevel;
        UpdateLevel();
    }
    void UpdateLevel() {
        textLevel.text = CurrentPlayer.Ins.curLevel + "级";
    }
}
