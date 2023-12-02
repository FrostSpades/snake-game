using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NetworkUtil;

namespace Model;
public class World
{
    private GameSettings settings;
    private Dictionary<SocketState, Snake> snakes;
    private int uniqueID;
    private string universalLock;
    private HashSet<Tuple<int, int>> wallLocations;

    public World()
    {
        snakes = new Dictionary<SocketState, Snake>();
        universalLock = "universalLock";

        string filePath = "settings.xml";
        string jsonContent = File.ReadAllText(filePath);
        GameSettings? tempSettings = JsonSerializer.Deserialize<GameSettings>(jsonContent);

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
    public void AddSnake(string name, int id, SocketState state)
    {
        
    }
    public int GetWorldSize()
    {
        return settings.WorldSize;
    }

    public int GetUniqueID()
    {
        int id = uniqueID;
        uniqueID++;

        return id;
    }
}
