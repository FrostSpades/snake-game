// Authors: Ethan Andrews and Mary Garfield
// Controller for snake game.
// University of Utah
namespace GameController;

using System.Data.SqlTypes;
using System.Diagnostics;
using System.Text.RegularExpressions;
using NetworkUtil;
using Model;
using System.Runtime.InteropServices.JavaScript;
using Newtonsoft.Json.Linq;
using System.Text.Json;

/// <summary>
/// Controller for snake game.
/// </summary>
public class GameController
{
    private string? name;
    private Model model;
    private int phase;
    private SocketState? server;

    // Event handler for when the view needs to update
    public delegate void GameUpdateHandler();
    public event GameUpdateHandler? GameUpdate;

    // Event for when there is an error in the connection process
    public delegate void ConnectionError();
    public event ConnectionError? Error;

    // Event for when the connection is successful
    public delegate void SuccessfulConnection();
    public event SuccessfulConnection? Success;

    // Event for when the server is closed
    public delegate void ServerClose();
    public event ServerClose? Closed;

    // Commands list for snake
    private string upCommand, downCommand, leftCommand, rightCommand, noneCommand;

    /// <summary>
    /// Constructor
    /// </summary>
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

    /// <summary>
    /// Returns the model
    /// </summary>
    /// <returns></returns>
    public Model GetModel()
    {
        return model;
    }

    /// <summary>
    /// Sends move instructions to the server based on input direction.
    /// </summary>
    /// <param name="direction"></param>
    public void Move(string direction)
    {
        // If server is null, don't do anything.
        if (server == null)
        {
            return;
        }

        string message = "";

        // Sends command to server based on direction
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

    /// <summary>
    /// Connects to server and sends name as the player name.
    /// </summary>
    /// <param name="server"></param>
    /// <param name="name"></param>
    public void Connect(string server, string name)
    {
        this.name = name;
        Networking.ConnectToServer(OnConnect, server, 11000);
            
    }

    /// <summary>
    /// Delegate to be called when server is connected.
    /// </summary>
    /// <param name="state"></param>
    private void OnConnect(SocketState state)
    {
        if (state.ErrorOccurred)
        {
            // If an error occurs during the connection process, display an error
            Error!.Invoke();
            return;
        }
        // If connection is successful, notify the view
        Success!.Invoke();

        server = state;
        state.OnNetworkAction = ReceiveMessage;
        Networking.GetData(state);
        Networking.Send(server.TheSocket, name);
    }
    
    /// <summary>
    /// Starts receive loop.
    /// </summary>
    /// <param name="state"></param>
    private void ReceiveMessage(SocketState state) 
    { 
        ProcessMessages(state);
        Networking.GetData(state);

    }

    /// <summary>
    /// Turns received data into model objects.
    /// </summary>
    /// <param name="state"></param>
    private void ProcessMessages(SocketState state)
    {
        // If error occurs, close the program
        if (state.ErrorOccurred)
        {
            Closed!.Invoke();
            return;
        }

        string totalData = state.GetData();
        string[] parts = Regex.Split(totalData, @"(?<=[\n])");
        
        // Loop through the message
        foreach(string p in parts)
        {
            if (p.Length==0)
                continue;

            // If message wasn't finished, break and wait for more message
            if (p[p.Length - 1] != '\n')
                break;


            switch (phase) 
            {
                // If message is player id, commit that to model.
                case 0:
                    string id = p.TrimEnd('\n');

                    // Get the player ID
                    model.SetID(id);

                    // Change the phase so that the next string that is read will do phase 1
                    phase = 1;

                    break;

                // If the incoming message is the world size, commit that to model
                case 1:
                    string worldSize = p.TrimEnd('\n');

                    model!.SetWorldSize(worldSize);

                    phase = 2;

                    break;
                
                // Receive message and determine which object it is
                case 2:
                    string json = p.TrimEnd('\n');


                    JObject obj = JObject.Parse(json);

                    // The type of the object is the only non null object
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

            // Remove the data from the buffer
            state.RemoveData(0, p.Length);
        }

        // Tell view to redraw
        GameUpdate!.Invoke();
    }
}