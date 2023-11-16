namespace GameController;

using System.Data.SqlTypes;
using System.Diagnostics;
using System.Text.RegularExpressions;
using NetworkUtil;
using Model;
using System.Runtime.InteropServices.JavaScript;
using Newtonsoft.Json.Linq;

public class GameController
{
    private string? name;
    private Model? model;
    private int phase;
    private SocketState? server;
    public GameController() 
    {
        phase = 0;
    }
    public void Move(string direction)
    {
        // ADD LOGIC SO THAT YOU CAN'T MOVE IF NOT CONNECTED YET

        string message = direction + "\n";
        Networking.Send(server.TheSocket, message);
    }
    public void Connect(string server, string name)
    {
        this.name = name;
        Networking.ConnectToServer(OnConnect, server, 11000);
            
    }
    private void OnConnect(SocketState state)
    {
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
        string totalData = state.GetData();
        string[] parts = Regex.Split(totalData, @"(?<=[\n])");
            
        foreach(string p in parts)
        {
            if (p.Length==0)
                continue;

            if (p[p.Length - 1] != '\n')
                break;

            //Debug.WriteLine(p);

            switch (phase) 
            {
                case 0:
                    string id = p.TrimEnd('\n');

                    // Get the player ID
                    model = new Model(id);

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

                    // IF TOO SLOW, CHANGE THIS

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
    }
}