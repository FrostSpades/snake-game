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

    // Message Queue for sending messages
    private Queue<string> messageQueue;

    public ServerController()
    {
        world = new World();
        clients = new();
        messageQueue = new();

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

        lock(world.GetSnakeLock())
        {
            clients[state.ID] = state;
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
            RemoveClient(state.ID);
            return;
        }
        

        state.OnNetworkAction = ReceiveFirstMessage;

        Networking.GetData(state);
    }
    private void ReceiveFirstMessage(SocketState state)
    {
        if(state.ErrorOccurred)
        {
            RemoveClient(state.ID);
            return;
        }

        ProcessName(state);

        Networking.GetData(state);
    }
    private void ReceiveMessage(SocketState state)
    {
        if (state.ErrorOccurred)
        {
            RemoveClient(state.ID);
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
                    world.Move("up", state);
                    break;

                case "down":
                    world.Move("down", state);
                    break;

                case "left":
                    world.Move("left", state);
                    break;

                case "right":
                    world.Move("right", state);
                    break;

                default:
                    world.Move("none", state);
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
            stopwatch.Restart();
            Update();
        }
    }

    private void RemoveClient(long id) 
    {
        Console.WriteLine("Client " + id + " disconnected");
        lock(world.GetSnakeLock())
        {
            clients.Remove(id);
        }
    }

    private void Update()
    {
        world.Update();

        lock (world.GetSnakeLock())
        {
            foreach (Snake s in world.GetSnakes())
            {
                string snakeString = JsonSerializer.Serialize(s);

                // Sends the json snake objects to all of the clients
                foreach (SocketState client in clients.Values)
                {
                    if (world.SnakeExists(client))
                    {
                        Networking.Send(client.TheSocket, snakeString + "\n");
                    }
                }

                if (s.died)
                {
                    s.died = false;
                }
            }

            // List of powerups to remove
            List<Powerup> removeList = new();

            foreach (Powerup p in world.GetPowerups())
            {
                string powerupString = JsonSerializer.Serialize(p);

                // Sends the json powerup object to all of the clients
                foreach (SocketState client in clients.Values)
                {
                    if (world.SnakeExists(client))
                    {
                        Networking.Send(client.TheSocket, powerupString + "\n");
                    }
                }

                if (p.died)
                {
                    removeList.Add(p);
                }
            }

            foreach (Powerup p in removeList)
            {
                world.RemovePowerup(p);
            }

            while (messageQueue.Count > 0)
            {
                string message = messageQueue.Dequeue();

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
