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
    private HashSet<Tuple<int, int>> wallLocations;
    private string snakeLock, snakeCreateLock;
    public World()
    {
        snakes = new Dictionary<SocketState, Snake>();
        powerups = new();

        snakeLock = "snakeLock";
        snakeCreateLock = "snakeCreateLock";

        string filePath = "settings.xml";
        GameSettings? tempSettings;

        DataContractSerializer ser = new(typeof(GameSettings));

        XmlReader reader = XmlReader.Create("settings.xml");
        tempSettings = (GameSettings?)ser.ReadObject(reader);

        //GameSettings? tempSettings = JsonSerializer.Deserialize<GameSettings>(jsonContent);

        if (tempSettings == null )
        {
            throw new FileNotFoundException("The settings file was not found at " + filePath);
        }

        settings = tempSettings;

        // Generate a set of all the point locations of the walls.
        // Useful for calculation of respawn location of other objects.
        wallLocations = new();

        foreach (Wall w in settings.walls.Values)
        {
            if (w.p1.X == w.p2.X)
            {
                // If wall is facing down
                if (w.p1.Y > w.p2.Y)
                {
                    for (int i = (int)(w.p1.X - 25); i < 50; i++)
                    {
                        for (int j = (int)(w.p2.Y - 25); j < w.p1.Y + 25; j++)
                        {
                            wallLocations.Add(new Tuple<int, int>(i, j));
                        }
                    }
                }
                // If wall is facing up
                else
                {
                    for (int i = (int)(w.p1.X - 25); i < 50; i++)
                    {
                        for (int j = (int)(w.p1.Y - 25); j < w.p2.Y + 25; j++)
                        {
                            wallLocations.Add(new Tuple<int, int>(i, j));
                        }
                    }
                }
            }
            else
            {
                // If wall is facing left
                if (w.p1.X > w.p2.X)
                {
                    for (int i = (int)(w.p1.Y - 25); i < 50; i++)
                    {
                        for (int j = (int)(w.p2.X - 25); j < w.p1.X + 25; j++)
                        {
                            wallLocations.Add(new Tuple<int, int>(j, i));
                        }
                    }
                }
                // If wall is facing right
                else
                {
                    for (int i = (int)(w.p1.Y - 25); i < 50; i++)
                    {
                        for (int j = (int)(w.p1.X - 25); j < w.p2.X + 25; j++)
                        {
                            wallLocations.Add(new Tuple<int, int>(j, i));
                        }
                    }
                }
            }
        }
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
        return settings.walls.Values;
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
        Snake newSnake = new Snake(id, name, GetSnakes(), wallLocations, GetPowerups(), snakeCreateLock, settings.UniverseSize);
        
        lock (snakeLock)
        {
            snakes.Add(state, newSnake);
        }
    }

    public void Update()
    {
        lock (snakeLock)
        {

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
