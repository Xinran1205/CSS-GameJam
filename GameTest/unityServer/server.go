package main

import (
	"bufio"
	"encoding/json"
	"fmt"
	"net"
	"sync"
)

type ClientAction struct {
	Action    string  `json:"Action"`
	PlayerID  string  `json:"PlayerID"`
	X         float64 `json:"X"`
	Y         float64 `json:"Y"`
	Direction float64 `json:"Direction"`
	Order     int     `json:"Order"` // 新增字段
}

var clientCounter int = 0

// keep track of all clients, key: connection, value: clientID
var clients = make(map[net.Conn]string)

// keep track of the last position of each client, key: clientID, value: ClientAction
var clientPositions = make(map[string]ClientAction)

// lock for updating clients and clientPositions
var lock sync.Mutex

func main() {
	listener, err := net.Listen("tcp", ":8000")
	if err != nil {
		fmt.Println("Error starting server:", err)
		return
	}
	fmt.Println("server started on :8000")

	defer listener.Close()

	for {
		conn, err := listener.Accept()
		if err != nil {
			fmt.Println("Error accepting connection:", err)
			continue
		}
		fmt.Println("new client connected:", conn.RemoteAddr().String())

		// handle each client in a separate goroutine
		go handleClient(conn)
	}
}

func handleClient(conn net.Conn) {
	defer conn.Close()

	clientID := fmt.Sprintf("%s", conn.RemoteAddr())

	lock.Lock()
	clientCounter++

	// 把这个clientID发回给客户端，作为客户端的PlayerID
	sendMessage(ClientAction{
		Action:   "myID",
		PlayerID: clientID,
		Order:    clientCounter,
	}, conn)

	clients[conn] = clientID

	// send all existing clients' last positions to the new client
	for _, clientIDTmp := range clients {
		//如果不是自己，就把这个人的位置信息发给自己
		if clientIDTmp != clientID {
			sendMessage(ClientAction{
				Action:    "join",
				PlayerID:  clientIDTmp,
				X:         clientPositions[clientIDTmp].X,
				Y:         clientPositions[clientIDTmp].Y,
				Direction: clientPositions[clientIDTmp].Direction,
				Order:     clientPositions[clientIDTmp].Order,
			}, conn)
		}
	}
	lock.Unlock()

	// Inform other clients (broadcast) that a new player has joined
	// 把他自己发给其他人，这个地方有bug
	//joinMsg := ClientAction{
	//	Action:   "join",
	//	PlayerID: clientID,
	//	Order:    clientCounter,
	//}
	//broadcastMessage(joinMsg)

	hasBroadcastedJoin := false
	reader := bufio.NewReader(conn)
	// only exit this for loop when the client disconnects
	for {
		message, err := reader.ReadString('\n')
		if err != nil {
			fmt.Printf("client %s disconnected\n", clientID)
			break
		}

		// unmarshal the JSON message into a ClientAction object
		var action ClientAction
		err = json.Unmarshal([]byte(message), &action)
		if err != nil {
			fmt.Println("Error unmarshalling JSON:", err)
			continue
		}

		action.PlayerID = clientID
		fmt.Printf("从客户端 %s 接收到消息: %+v\n", clientID, action)
		// 如果收到感染消息，就广播给其他人，然后我要让这个人下线
		if action.Action == "infected" {
			//给其他人广播他被感染的消息，然后break，再给其他人广播他离开的消息
			broadcastMessage(action)
			break
		}
		if action.Action == "move" {
			lock.Lock()
			clientPositions[action.PlayerID] = action
			lock.Unlock()

			if !hasBroadcastedJoin {
				joinMsg := ClientAction{
					Action:    "join",
					PlayerID:  clientID,
					X:         action.X,
					Y:         action.Y,
					Direction: action.Direction,
					Order:     clientCounter,
				}
				broadcastMessage(joinMsg)
				hasBroadcastedJoin = true
			}
			broadcastMessage(action)
		}
	}

	// Inform other clients that this player has left
	leaveMsg := ClientAction{
		Action:   "leave",
		PlayerID: clientID,
	}
	broadcastMessage(leaveMsg)

	lock.Lock()
	clientCounter--
	delete(clients, conn)
	delete(clientPositions, clientID)
	lock.Unlock()
}

// broadcastMessage sends a message to all connected clients
func broadcastMessage(action ClientAction) {
	message, err := json.Marshal(action)
	if err != nil {
		fmt.Println("Error marshalling JSON:", err)
		return
	}
	//不广播给自己
	for otherConn := range clients {
		if clients[otherConn] == action.PlayerID {
			continue
		}
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
