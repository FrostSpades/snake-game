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
using SnakeGame;

namespace Model;
public class World
{
    private GameSettings settings;
    private Dictionary<SocketState, Snake> snakes;
    private Dictionary<int, Powerup> powerups;
    private int uniqueSnakeID, uniquePowerupID;
    private string snakeLock;

    private int maxPowerups = 20;
    private int maxPowerupFrames = 75;
    private int currentPowerupFrames, currentNeededPowerupFrames;
    public World()
    {
        snakes = new Dictionary<SocketState, Snake>();
        powerups = new();

        snakeLock = "snakeLock";

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
        uniquePowerupID = 0;
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
        lock (snakeLock)
        {
            return snakes.Values;
        }
    }

    public IEnumerable<Powerup> GetPowerups()
    {
        return powerups.Values;
    }

    public void AddSnake(string name, int id, SocketState state)
    {
        Snake newSnake = new Snake(id, name, GetSnakes(), GetWalls(), GetPowerups(), snakeLock, settings.UniverseSize, this);
        
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
                if (s.alive)
                {
                    s.Update();
                }
                else
                {
                    s.AddDeadFrame();
                }
            }

            if (powerups.Count < maxPowerups)
            {
                if (currentPowerupFrames == 0)
                {
                    Random random = new Random();
                    currentNeededPowerupFrames = (int)(random.NextDouble() * maxPowerupFrames) + 1;
                    currentPowerupFrames += 1;
                }
                else
                {
                    if (currentPowerupFrames == currentNeededPowerupFrames)
                    {
                        currentPowerupFrames = 0;
                        powerups.Add(uniquePowerupID, new Powerup(uniquePowerupID, this));
                        uniquePowerupID += 1;
                    }
                    else
                    {
                        currentPowerupFrames += 1;
                    }
                }
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
        int id = uniqueSnakeID;
        uniqueSnakeID++;

        return id;
    }

    public int GetRespawnFrames()
    {
        return settings.RespawnRate;
    }
    public bool SnakeExists(SocketState name)
    {
        return snakes.ContainsKey(name);
    }

    public void RemovePowerup(Powerup p)
    {
        powerups.Remove(p.power);
    }

    public static List<Vector2D> CalculatePoint(Vector2D head, Vector2D tail, int size)
    {
        if (head.X == tail.X)
        {
            if (head.Y > tail.Y)
            {
                Vector2D point1 = new Vector2D(tail);
                point1.X -= size;
                point1.Y -= size;
                Vector2D point2 = new Vector2D(tail);
                point2.X += size;
                point2.Y -= size;
                Vector2D point3 = new Vector2D(head);
                point3.X += size;
                point3.Y += size;
                Vector2D point4 = new Vector2D(head);
                point4.X -= size;
                point4.Y += size;
                List<Vector2D> points = new() { point1, point2, point3, point4 };
                return points;


            }
            else
            {
                Vector2D point1 = new Vector2D(head);
                point1.X -= size;
                point1.Y -= size;
                Vector2D point2 = new Vector2D(head);
                point2.X += size;
                point2.Y -= size;
                Vector2D point3 = new Vector2D(tail);
                point3.X += size;
                point3.Y += size;
                Vector2D point4 = new Vector2D(tail);
                point4.X -= size;
                point4.Y += size;
                List<Vector2D> points = new() { point1, point2, point3, point4 };
                return points;
            }
        }
        else
        {
            if (head.X > tail.X)
            {
                Vector2D point1 = new Vector2D(tail);
                point1.X -= size;
                point1.Y -= size;
                Vector2D point2 = new Vector2D(head);
                point2.X += size;
                point2.Y -= size;
                Vector2D point3 = new Vector2D(head);
                point3.X += size;
                point3.Y += size;
                Vector2D point4 = new Vector2D(tail);
                point4.X -= size;
                point4.Y += size;
                List<Vector2D> points = new() { point1, point2, point3, point4 };
                return points;
            }
            else
            {
                Vector2D point1 = new Vector2D(head);
                point1.X -= size;
                point1.Y -= size;
                Vector2D point2 = new Vector2D(tail);
                point2.X += size;
                point2.Y -= size;
                Vector2D point3 = new Vector2D(tail);
                point3.X += size;
                point3.Y += size;
                Vector2D point4 = new Vector2D(head);
                point4.X -= size;
                point4.Y += size;
                List<Vector2D> points = new() { point1, point2, point3, point4 };
                return points;
            }
        }
    }
}
