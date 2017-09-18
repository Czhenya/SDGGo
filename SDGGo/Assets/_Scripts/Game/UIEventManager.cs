using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SDG {
    public class UIEventManager : MonoBehaviour
    {

        void Start() {
        }

        void Update() {
            if (Input.GetMouseButtonDown(0)) { 
                // 选子
                Panel.Ins.SelectMove(Input.mousePosition);
            }

            if (Input.GetMouseButtonDown(1))
            {
                Panel.Ins.MoveBack();
            }
        }

        // 落子
        public void ConfirmMove()
        {
            Panel.Ins.SetMove();
        }

        public void BackHome() {
            SceneManager.LoadScene("Menu");
        }
    }
}
