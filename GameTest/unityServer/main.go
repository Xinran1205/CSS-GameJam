package main

//
//import (
//	"log"
//	"net/http"
//)
//
//func main() {
//	//这一行将 HTTP 路径 /ws 注册到了 handleConnections 函数。
//	//这意味着当有客户端尝试访问服务器的 /ws 路径时，会执行 handleConnections 函数来处理该请求。在此程序中，这主要用于处理 WebSocket 的连接请求。
//	http.HandleFunc("/ws", handleConnections)
//	//go 关键字用于启动一个新的协程，该协程会并发地执行 handleMessages 函数。
//	//这意味着主程序可以继续执行后续的代码，而 handleMessages 函数会在后台运行，负责从 broadcast 通道读取消息并将其发送给所有在线的 WebSocket 客户端。
//	go handleMessages()
//
//	// 启动web服务器
//	log.Println("http server started on :8000")
//	//除非有外部因素导致它退出（如手动停止程序、系统错误等），否则 main 函数中的 http.ListenAndServe(":8000", nil) 调用会使程序持续运行，
//	//并保持监听 :8000 端口。这是一个阻塞调用，意味着它会阻止 main 函数继续执行下去，直到服务器停止或发生错误。
//	err := http.ListenAndServe(":8000", nil)
//	if err != nil {
//		log.Fatal("ListenAndServe: ", err)
//	}
//}
