using Model;
using NetworkUtil;
using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ServerController;
public class ServerController
{
    World world;
    Dictionary<long, SocketState> clients;

    public ServerController()
    {
        world = new World();
        clients = new();

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

        foreach (string part in parts)
        {
            if (part.Length == 0)
                continue;

            if (part[part.Length - 1] != '\n')
                break;

            // Get a unique world id
            int id = world.GetUniqueID();

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

            // Change the OnNetworkAction to process commands
            state.OnNetworkAction = ReceiveMessage;
            state.RemoveData(0, part.Length);
        }
    }
    private void ProcessCommand(SocketState state)
    {
        string totalData = state.GetData();
        string[] parts = Regex.Split(totalData, @"(?<=[\n])");

        foreach (string part in parts)
        {
            if(part.Length == 0)
                continue;

            if (part[part.Length - 1] != '\n')
                break;

            ControlCommand? command = JsonSerializer.Deserialize<ControlCommand>(part);
            if (command != null)
            {
                switch (command.moving)
                {
                    case "up":

                        break;

                    case "down":

                        break;

                    case "left":

                        break;

                    case "right":

                        break;
                }
            }

            state.RemoveData(0, part.Length);
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
        Console.WriteLine("Client" + id + "disconnected");
        lock(clients)
        {
            clients.Remove(id);
        }
    }

    private void Update()
    {
        //change world and send to clients
    }


}
