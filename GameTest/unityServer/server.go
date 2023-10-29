package main

import (
	"bufio"
	"encoding/json"
	"fmt"
	"net"
	"sync"
)

type ClientAction struct {
	Action   string  `json:"Action"`
	PlayerID string  `json:"PlayerID"`
	X        float64 `json:"X"`
	Y        float64 `json:"Y"`
}

var clients = make(map[net.Conn]string)
var clientPositions = make(map[string]ClientAction) // 保存每个客户端的最后位置
var lock sync.Mutex

func main() {
	listener, err := net.Listen("tcp", ":8000")
	if err != nil {
		fmt.Println("Error starting server:", err)
		return
	}
	fmt.Println("服务器启动成功，监听端口 :8000")

	defer listener.Close()

	for {
		conn, err := listener.Accept()
		if err != nil {
			fmt.Println("Error accepting connection:", err)
			continue
		}
		fmt.Printf("新的客户端已连接: %s\n", conn.RemoteAddr().String())

		go handleClient(conn)
	}
}

func handleClient(conn net.Conn) {
	defer conn.Close()

	clientID := fmt.Sprintf("%s", conn.RemoteAddr())

	lock.Lock()
	clients[conn] = clientID
	// 向这个新客户端发送 所有其他客户端的 "join" 信息
	for _, action := range clients {
		if action != clientID {
			sendMessage(ClientAction{
				Action:   "join",
				PlayerID: action,
				X:        clientPositions[action].X,
				Y:        clientPositions[action].Y,
			}, conn)
		}
	}

	lock.Unlock()

	// Inform other clients that a new player has joined
	joinMsg := ClientAction{
		Action:   "join",
		PlayerID: clientID,
	}
	broadcastMessage(joinMsg)

	reader := bufio.NewReader(conn)

	for {
		message, err := reader.ReadString('\n')
		if err != nil {
			fmt.Printf("客户端 %s 断开连接\n", clientID)
			break
		}

		var action ClientAction
		err = json.Unmarshal([]byte(message), &action)
		if err != nil {
			fmt.Println("Error unmarshalling JSON:", err)
			continue
		}

		action.PlayerID = clientID
		fmt.Printf("从客户端 %s 接收到消息: %+v\n", clientID, action)

		if action.Action == "move" {
			lock.Lock()
			clientPositions[action.PlayerID] = action
			lock.Unlock()
		}

		broadcastMessage(action)
	}

	// Inform other clients that this player has left
	leaveMsg := ClientAction{
		Action:   "leave",
		PlayerID: clientID,
	}
	broadcastMessage(leaveMsg)

	lock.Lock()
	delete(clients, conn)
	delete(clientPositions, clientID) // 在玩家离开时删除其位置
	lock.Unlock()
}

func broadcastMessage(action ClientAction) {
	message, err := json.Marshal(action)
	if err != nil {
		fmt.Println("Error marshalling JSON:", err)
		return
	}

	for otherConn := range clients {
		_, err := otherConn.Write(append(message, '\n'))
		if err != nil {
			fmt.Println("Error sending message:", err)
		}
	}
}

func sendMessage(action ClientAction, conn net.Conn) {
	message, err := json.Marshal(action)
	if err != nil {
		fmt.Println("Error marshalling JSON:", err)
		return
	}
	_, err = conn.Write(append(message, '\n'))
	if err != nil {
		fmt.Println("Error sending message:", err)
	}
}
