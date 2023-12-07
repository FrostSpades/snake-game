using Model;
using NetworkUtil;
using System.Data;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Diagnostics.Tracing;

namespace ServerController;
public class ServerController
{
    private World world;
    private Dictionary<long, SocketState> clients;
    private HashSet<string> validCommands;

    // Message Queues for sending items on main thread
    private Queue<Tuple<string, SocketState>> messageQueue;
    private Queue<SocketState> disconnectQueue;

    // Fields for printing out frame count each second
    private long totalMillisecondsElapsed;
    private int frameCount;

    public ServerController()
    {
        world = new World();
        clients = new();
        messageQueue = new();
        disconnectQueue = new();

        totalMillisecondsElapsed = 0;
        frameCount = 0;

        validCommands = new HashSet<string>
            {
                "{\"moving\":\"left\"}",
                "{\"moving\":\"right\"}",
                "{\"moving\":\"up\"}",
                "{\"moving\":\"down\"}",
                "{\"moving\":\"none\"}"
            };

        StartServer();
    }

    public World GetWorld()
    {
        return world;
    }

    public void StartServer()
    {
        Networking.StartServer(NewClientConnected, 11000);

        Console.WriteLine("Server is running. Accepting clients.");
    }

    public void NewClientConnected(SocketState state)
    {
        if (state.ErrorOccurred)
        {
            return;
        }

        lock(clients)
        {
            clients[state.ID] = state;

            Debug.WriteLine("");
        }

        // Print out the IP address and port
        try
        {
            // Get the remote IP address
            IPAddress ipAddress = ((IPEndPoint)state.TheSocket.RemoteEndPoint!).Address;
            int port = ((IPEndPoint)state.TheSocket.RemoteEndPoint!).Port;
            Console.WriteLine("Accepted new connection from " + ipAddress + ":" + port);
        }
        catch
        {
            RemoveClient(state);
            return;
        }
        

        state.OnNetworkAction = ReceiveFirstMessage;

        Networking.GetData(state);
    }
    private void ReceiveFirstMessage(SocketState state)
    {
        if(state.ErrorOccurred)
        {
            RemoveClient(state);
            return;
        }

        ProcessName(state);

        Networking.GetData(state);
    }
    private void ReceiveMessage(SocketState state)
    {
        if (state.ErrorOccurred)
        {
            RemoveClient(state);
            return;
        }
        ProcessCommand(state);
        Networking.GetData(state);
    }

    private void ProcessName(SocketState state) 
    {
        string totalData = state.GetData();
        string[] parts = Regex.Split(totalData, @"(?<=[\n])");

        foreach (string p in parts)
        {
            if (p.Length == 0)
                continue;

            if (p[p.Length - 1] != '\n')
                break;

            string part = p.Substring(0, p.Length - 1);

            // Get a unique world id
            int id = world.GetUniqueID();

            // Send the id and the world size
            Networking.Send(state.TheSocket, id + "\n");
            Networking.Send(state.TheSocket, world.GetWorldSize() + "\n");
            
            // Sends all the walls
            foreach (Wall w in world.GetWalls())
            {
                string jsonWall = JsonSerializer.Serialize<Wall>(w);

                Networking.Send(state.TheSocket, jsonWall + "\n");
            }

            // Finally adds the snake to the snake list so that it can
            // receive all the other messages. Necessary for making sure
            // it doesn't receive snakes before it receives walls
            world.AddSnake(part, id, state);

            // Prints out that snake has connected
            Console.WriteLine("Player(" + id + ")" + " \"" + part + "\" has joined");

            // Change the OnNetworkAction to process commands
            state.OnNetworkAction = ReceiveMessage;
            state.RemoveData(0, part.Length);
        }
    }
    private void ProcessCommand(SocketState state)
    {
        string totalData = state.GetData();
        string[] parts = Regex.Split(totalData, @"(?<=[\n])");

        foreach (string p in parts)
        {
            if (p.Length == 0)
                continue;

            if (p[p.Length - 1] != '\n')
                break;

            string part = p.Substring(0, p.Length - 1);

            if (!validCommands.Contains(part))
            {
                continue;
            }

            ControlCommand command = JsonSerializer.Deserialize<ControlCommand>(part)!;
            
            switch (command.moving)
            {
                case "up":
                    //world.Move("up", state);
                    lock (messageQueue)
                    {
                        messageQueue.Enqueue(new Tuple<string, SocketState>("up", state));
                    }
                    break;

                case "down":
                    //world.Move("down", state);
                    lock (messageQueue)
                    {
                        messageQueue.Enqueue(new Tuple<string, SocketState>("down", state));
                    }
                    break;

                case "left":
                    //world.Move("left", state);
                    lock (messageQueue)
                    {
                        messageQueue.Enqueue(new Tuple<string, SocketState>("left", state));
                    }
                    break;

                case "right":
                    //world.Move("right", state);
                    lock (messageQueue)
                    {
                        messageQueue.Enqueue(new Tuple<string, SocketState>("right", state));
                    }
                    break;

                default:
                    //world.Move("none", state);
                    lock (messageQueue)
                    {
                        messageQueue.Enqueue(new Tuple<string, SocketState>("state", state));
                    }
                    break;
            }
            

            state.RemoveData(0, p.Length);
        }
    }
    public void StartMainLoop()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        while (true)
        {
            while (stopwatch.ElapsedMilliseconds < world.GetMSPerFrame())
            {
                continue;
            }

            totalMillisecondsElapsed += stopwatch.ElapsedMilliseconds;
            frameCount += 1;

            // If one total second has elapsed, print out the frame count
            if (totalMillisecondsElapsed >= 1000)
            {
                totalMillisecondsElapsed = 0;

                Console.WriteLine("FPS: " + frameCount);

                frameCount = 0;
            }

            stopwatch.Restart();
            Update();
        }
    }

    private void RemoveClient(SocketState state) 
    {
        Console.WriteLine("Client " + state.ID + " disconnected");
        lock(clients)
        {
            clients.Remove(state.ID);
        }
        lock(disconnectQueue)
        {
            disconnectQueue.Enqueue(state);
        }
    }

    private void Update()
    {

        // Update the snakes' movements
        lock (messageQueue)
        {
            while (messageQueue.Count > 0)
            {
                Tuple<string, SocketState> command = messageQueue.Dequeue();
                world.Move(command.Item1, command.Item2);
            }
        }

        // Update the world
        world.Update();

        // Send the objects
        List<string> messages = world.GetSerializedObjects();

        lock (clients)
        {
            foreach (string message in messages)
            {
                // Sends the json powerup object to all of the clients
                foreach (SocketState client in clients.Values)
                {
                    if (world.SnakeExists(client))
                    {
                        Networking.Send(client.TheSocket, message + "\n");
                    }
                }
            }
        }

        // Send the disconnectedSnakes
        List<string> disconnectedSnakes = new();
        lock (disconnectQueue)
        {
            while (disconnectQueue.Count > 0)
            {
                SocketState client = disconnectQueue.Dequeue();

                string json = world.Disconnect(client);
                
                disconnectedSnakes.Add(json);
            }
        }

        // Send the disconnectedSnakes data
        lock (clients)
        {
            foreach (string message in disconnectedSnakes)
            {
                // Sends the json powerup object to all of the clients
                foreach (SocketState client in clients.Values)
                {
                    if (world.SnakeExists(client))
                    {
                        Networking.Send(client.TheSocket, message + "\n");
                    }
                }
            }
        }
    }


}
