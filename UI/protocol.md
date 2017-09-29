# 协议

## 注册

Client Request

MsgName: ReqSignUp

```json
{
    "name" : "zhangsan"
}
```

Server Response

MsgName: RetSignUp

```json
{
    "code" : 0,
    "msg" : "OK"
    "userid" : 1,
    "token" : "abc123"
}
```

## 登录

Client Request

MsgName: ReqSignIn

```json
{
    "name" : "zhangsan"
}
```

Server Response

MsgName: ReqSignIn

```json
{
    "code" : 0,
    "msg" : "OK"
    "userid" : 1,
    "token" : "abc123"
}
```

## 获取房间列表

Client Request

MsgName: ReqGetRoomList

```json
{
    "userid" : 1,
    "token" : "abc123",
}
```

Server Respons

MsgName: RetGetRoomList

```json
{
    "code" : 0,
    "msg" : "OK",
    "roomlist" : [
        {
            "roomid" : 1,
            "owner" : "zhangsan"
        },
        {
            "roomid" : 2,
            "owner" : "lisi"
        }
    ]
}
```

## 创建房间

Client Request

MsgName: ReqCreateRoom

```json
{
    "userid" : 1,
    "token" : "abc123"
}
```

Server Response

MsgName: RetCreateRoom

```json
{
    "code" : 0,
    "msg" : "OK",
    "roomid" : 1
}
```

## 加入某房间

Client Request

MsgName: ReqJoinRoom

```json
{
    "userid" : 1,
    "token" : "abc123",
    "roomid" : 1
}
```

Server Response

MsgName: RetJoinRoom

```json
{
    "code" : 0,
    "msg" : "OK",
    "roomid" : 1,
    "userid" : 1,
    "name" : "zhangsan"
}
```

## 游戏开始

房间人员一满，游戏就开始，服务器主动推送给房间里的所有玩家。

Server Response

MsgName: RetGameStart

```json
{
    "roomid" : 1,
}
```

## 游戏结束

房主的客户端判断游戏胜负，然后发给服务器，服务器再广播给这个房间的所有人。

Client Request

MsgName: ReqGameEnd

```json
{
    "userid" : 1,
    "token" : "abc123"
    "type" : 1,
    "winnerid" : 1
}
```

Server Response

MsgName: RetGameEnd

```json
    "roomid" : 1,
    "type" : 1,
    "winnerid" : 1,
    "winnername" : "zhangsan"
```

type
0: 正常胜利
1: 有人认输
2: 有人掉线

## 棋子落定

Client Request

MsgName: ReqOperatePiece

```json
{
    "userid" : 1,
    "token" : "abc123",
    "x" : 20,
    "y" : 18
}
```

Server Response

MsgName: RetOperatePiece

```json
{
    "x" : 20,
    "y" : 18
}
```

下子方客户端发消息给服务器，服务器仅仅通知另一方下子的位置。

## 请求结算

Client Request

MsgName: ReqCheckOut

```json
{
    "userid" : 1,
    "token" : "abc123"
}
```

Server Response

MsgName: RetCheckOut

```json
{
}
```

## 游戏开始

Client Request

MsgName: ReqGameStart

```json
{
    "userid" : 1,
    "token" : "abc123"
}
```

Server Response

MsgName: RetGameStart

```json
{
    "roomid" : 1,
}
```