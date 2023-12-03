using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using NetworkUtil;

namespace Model;
public class World
{
    private GameSettings settings;
    private Dictionary<SocketState, Snake> snakes;
    private Dictionary<int, Powerup> powerups;
    private int uniqueID;
    private string snakeLock, snakeCreateLock;
    public World()
    {
        snakes = new Dictionary<SocketState, Snake>();
        powerups = new();

        snakeLock = "snakeLock";
        snakeCreateLock = "snakeCreateLock";

        // Import GameSettings from settings.xml file
        string filePath = "settings.xml";
        GameSettings? tempSettings;

        DataContractSerializer ser = new(typeof(GameSettings));

        XmlReader reader = XmlReader.Create("settings.xml");
        tempSettings = (GameSettings?)ser.ReadObject(reader);

        if (tempSettings == null )
        {
            throw new FileNotFoundException("The settings file was not found at " + filePath);
        }

        settings = tempSettings;
    }

    public void Move(string dir, SocketState state)
    {
        snakes[state].SetDirection(dir);
    }

    public int GetMSPerFrame()
    {
        return settings.MSPerFrame;
    }

    public IEnumerable<Wall> GetWalls()
    {
        return settings.Walls;
    }

    public IEnumerable<Snake> GetSnakes()
    {
        return snakes.Values;
    }

    public IEnumerable<Powerup> GetPowerups()
    {
        return powerups.Values;
    }

    public void AddSnake(string name, int id, SocketState state)
    {
        Snake newSnake = new Snake(id, name, GetSnakes(), GetWalls(), GetPowerups(), snakeCreateLock, settings.UniverseSize, this);
        
        lock (snakeLock)
        {
            snakes.Add(state, newSnake);
        }
    }

    public void Update()
    {
        lock (snakeLock)
        {
            foreach (Snake s in snakes.Values)
            {
                s.Update();
            }
        }
    }

    public string GetSnakeLock()
    {
        return snakeLock;
    }

    public int GetWorldSize()
    {
        return settings.UniverseSize;
    }

    public int GetUniqueID()
    {
        int id = uniqueID;
        uniqueID++;

        return id;
    }
}
