namespace GameController;

using System.Data.SqlTypes;
using System.Diagnostics;
using System.Text.RegularExpressions;
using NetworkUtil;
using Model;
using System.Runtime.InteropServices.JavaScript;
using Newtonsoft.Json.Linq;
using System.Text.Json;

public class GameController
{
    private string? name;
    private Model model;
    private int phase;
    private SocketState? server;

    public delegate void GameUpdateHandler();
    public event GameUpdateHandler GameUpdate;

    public delegate void ConnectionError();
    public event ConnectionError Error;

    public delegate void SuccessfulConnection();
    public event SuccessfulConnection Success;

    public delegate void ServerClose();
    public event ServerClose Closed;

    private string upCommand, downCommand, leftCommand, rightCommand, noneCommand;

    public GameController() 
    {
        model = new();
        phase = 0;

        upCommand = JsonSerializer.Serialize(new ControlCommand("up"));
        downCommand = JsonSerializer.Serialize(new ControlCommand("down"));
        leftCommand = JsonSerializer.Serialize(new ControlCommand("left"));
        rightCommand = JsonSerializer.Serialize(new ControlCommand("right"));
        noneCommand = JsonSerializer.Serialize(new ControlCommand("none"));
    }

    public Model GetModel()
    {
        return model;
    }

    public void Move(string direction)
    {
        if (server == null)
        {
            return;
        }

        string message = "";

        switch (direction)
        {
            case "up":
                message = upCommand + "\n";
                break;
            case "down":
                message = downCommand + "\n";
                break;
            case "left":
                message = leftCommand + "\n";
                break;
            case "right":
                message = rightCommand + "\n";
                break;
            case "none":
                message = noneCommand + "\n";
                break;
        }
        
        Networking.Send(server.TheSocket, message);
    }
    public void Connect(string server, string name)
    {
        this.name = name;
        Networking.ConnectToServer(OnConnect, server, 11000);
            
    }
    private void OnConnect(SocketState state)
    {
        if (state.ErrorOccurred)
        {
            // If an error occurs during the connection process, display an error
            Error.Invoke();
            return;
        }
        Success.Invoke();
        server = state;
        state.OnNetworkAction = ReceiveMessage;
        Networking.GetData(state);
        Networking.Send(server.TheSocket, name);
    }
        
    private void ReceiveMessage(SocketState state) 
    { 
        ProcessMessages(state);
        Networking.GetData(state);

    }

    private void ProcessMessages(SocketState state)
    {
        if (state.ErrorOccurred)
        {
            Closed.Invoke();
            return;
        }

        string totalData = state.GetData();
        string[] parts = Regex.Split(totalData, @"(?<=[\n])");
            
        foreach(string p in parts)
        {
            if (p.Length==0)
                continue;

            if (p[p.Length - 1] != '\n')
                break;


            switch (phase) 
            {
                case 0:
                    string id = p.TrimEnd('\n');

                    // Get the player ID
                    model.SetID(id);

                    // Change the phase so that the next string that is read will do phase 1
                    phase = 1;

                    break;

                case 1:
                    string worldSize = p.TrimEnd('\n');

                    model!.SetWorldSize(worldSize);

                    phase = 2;

                    break;

                case 2:
                    string json = p.TrimEnd('\n');


                    JObject obj = JObject.Parse(json);

                    JToken? power = obj["power"];
                    JToken? wall = obj["wall"];
                    JToken? snake = obj["snake"];

                    if (power != null)
                    {
                        model!.AddPowerup(json);
                    }
                    else if (wall != null)
                    {
                        model!.AddWall(json);
                    }
                    else if (snake != null)
                    {
                        model!.AddSnake(json);
                    }

                    break;
            }

            state.RemoveData(0, p.Length);
        }

        GameUpdate.Invoke();
    }
}