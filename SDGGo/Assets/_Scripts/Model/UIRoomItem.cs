using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRoomItem : MonoBehaviour {
    public static float margin = 10.0f;
    public static float height = 100.0f;
    /// <summary>
    /// 房间信息UI
    /// </summary>
    public int room_index;
    public Text room_name;
    public Text room_occupy;
    public Text room_info;
    public Button button_enter;

    void Start()
    {
        // 通知manager请求进入指定房间
        button_enter.onClick.AddListener(() => {
            RoomSelectManager.Ins.EnterRoom(room_index);
        });
    }
}
