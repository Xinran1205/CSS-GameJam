# CSS-GameJam

# UNIVERSITY OF BRISTOL COMPUTER SCIENCE SOCIETY GameJam

## Game Name:    ECO QUEST

### Description
This is a project from the GameJam competition organized by the UNIVERSITY OF BRISTOL COMPUTER SCIENCE SOCIETY. Our team uniquely presents a multiplayer online game focused around the theme of environmental pollution.

### Gameplay
- One player takes on the role of an infected horse.
- Other players act as normal horses, aiming to evade the infected horse's pursuit.

### How to Play:
1. **Download** the `GameClient.zip`.
2. **Extract** the `GameClient.zip` file.
3. Once extracted, **launch** the game by clicking on `My project.exe` to start the client.

## Server Code （Golang）

### Features:
- **Listens for incoming client connections** on port 8000.
- **Assigns a unique ID** to each client based on their remote address.
- **Broadcasts a player's position** to all other players.
- Handles a player's "**infection**" status and notifies all other players.
- **Maintains a list** of all connected clients and their last known positions.
- Sends a list of all other players' positions to a **newly connected client**.

### Main Structures:
- **ClientAction**: Represents the action taken by a client. It includes the action type (move, join, infected, leave), player ID, position (X, Y coordinates), direction, and order (sequence of joining).
- **clients**: A dictionary that maps a client's connection to their ID.
- **clientPositions**: A dictionary that maps a client's ID to their last known ClientAction.

### Important Functions:
- **main**: Starts the server and listens for incoming client connections.
- **handleClient**: Handles individual client connections, including sending and receiving messages.
- **broadcastMessage**: Sends a message to all connected clients except the sender.
- **sendMessage**: Sends a message to a specific client.

## Client Code (Unity)

### Features:
- **Connects to the server** and gets assigned a unique ID and order.
- **Sends its position** to the server whenever it moves.
- **Receives updates** about other players' positions and updates their positions locally.
- **Detects if it is near** the "infected" player (the boss) and sends an "infected" status to the server if true.
- **Displays an end-game screen** when infected.
- **Enters spectator mode** after end-game screen.
- **Disconnects from the server** gracefully when the application quits.

### Main Components:
- **PlayerController**: Handles player movements, checks for infections, and communicates with the server.
- **TCPConnection**: Manages the TCP connection to the server, sends and receives messages, and handles server messages.
- **OtherPlayer**: Represents other players in the game world and provides functions to spawn, move, and remove them.

### Important Functions:
- **SendInitialPosition**: Sends the player's initial position to the server upon connection.
- **Update**: Checks for player input and sends movement updates to the server.
- **CheckDistanceWithInfectedHorse**: Checks if the player is near the "infected" player and updates its status if necessary.
- **ReceiveMessages**: Listens for incoming server messages in a separate thread.
- **HandleMessage**: Processes server messages and updates the game state accordingly.
- **SendMessageToServer**: Sends a message to the server.
- **OnApplicationQuit**: Gracefully disconnects from the server when the application closes.

# Contributors:

- **Xinran Wang (id19037@bristol.ac.uk)**: Lead Developer and Planner. Responsible for both server and client development.
- **Xinyu Wang**: Co-developer and Planner. Primarily focused on certain parts of Unity and additional coding.
- **Lihan Shen (az19939@bristol.ac.uk)**: Managed server deployment, and also handled map and terrain design and build.
- **Boyang Lin**: Responsible for model creation and artistic contributions.
- **Daolin Zhang**: Contributed to copywriting, design, and project planning.
