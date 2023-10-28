package main

import (
	"crypto/rand"
	"encoding/hex"
	"github.com/gorilla/websocket"
	"log"
	"net/http"
)

// 定义客户端的映射
var clients = make(map[*websocket.Conn]string)

// 定义广播的通道，用于接收和发送客户端操作数据
var broadcast = make(chan ClientAction)

// 配置WebSocket升级器,是用于将 HTTP 连接升级为 WebSocket 连接的结构体。
var upgrader = websocket.Upgrader{}

// 定义客户端操作的数据结构
type ClientAction struct {
	Action   string  `json:"Action"` // "join", "move", "leave"
	PlayerID string  `json:"PlayerID"`
	X        float32 `json:"x,omitempty"`
	Y        float32 `json:"y,omitempty"`
}

// 处理WebSocket连接的函数,当有新的 WebSocket 连接时，它会为该连接生成一个唯一的 playerID，然后发送回客户端。
func handleConnections(w http.ResponseWriter, r *http.Request) {
	//使用 upgrader 将 HTTP 请求升级为 WebSocket 连接。
	ws, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		log.Fatalf("WebSocket upgrade failed: %v", err)
		return
	}
	defer ws.Close()

	// 生成唯一的ID
	playerID := generateUniqueID()

	// 将新的WebSocket连接添加到clients映射中
	clients[ws] = playerID

	// 为新客户端发送其playerID
	idMessage := ClientAction{
		Action:   "id", // 新增的动作类型，表示ID消息
		PlayerID: playerID,
	}

	err = ws.WriteJSON(idMessage) // 发送消息回客户端
	if err != nil {
		log.Printf("Error sending playerID to client: %v", err)
		return
	}

	// 把新用户join的消息发送到广播通道
	joinAction := ClientAction{
		Action:   "join",
		PlayerID: playerID,
	}
	broadcast <- joinAction

	// 循环读取当前连接的客户端发送的消息
	for {
		var action ClientAction
		err := ws.ReadJSON(&action)
		if err != nil {
			log.Printf("Error reading JSON: %v", err)

			// 当客户端断开时，发送leave消息
			leaveAction := ClientAction{
				Action:   "leave",
				PlayerID: action.PlayerID, // 使用之前的PlayerID
			}
			broadcast <- leaveAction

			delete(clients, ws)
			break
		}

		// 打印接收到的信息
		log.Printf("Received action from player %s: %s, x=%f, y=%f", action.PlayerID, action.Action, action.X, action.Y)

		// 将读取到的信息发送到广播通道
		broadcast <- action
	}
}

// 处理接收到的信息并将其广播到所有连接的WebSocket客户端
func handleMessages() {
	for {
		action := <-broadcast
		for client := range clients {
			err := client.WriteJSON(action)
			if err != nil {
				log.Printf("Error writing JSON: %v", err)
				client.Close()
				delete(clients, client)
			}
		}
	}
}

// generateUniqueID 生成一个唯一的ID
func generateUniqueID() string {
	buf := make([]byte, 16) // 创建一个16字节的缓冲区
	_, err := rand.Read(buf)
	if err != nil {
		log.Fatalf("Failed to generate unique ID: %v", err)
	}
	return hex.EncodeToString(buf) // 将字节数组转换为十六进制字符串
}
