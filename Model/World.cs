using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    private string snakeLock, powerupLock;

    private bool speedMode;

    private int maxPowerups = 20;
    private int maxPowerupFrames = 75;
    private int currentPowerupFrames, currentNeededPowerupFrames;
    public World()
    {
        snakes = new Dictionary<SocketState, Snake>();
        powerups = new();

        snakeLock = "snakeLock";
        powerupLock = "powerupLock";

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

        speedMode = settings.SpeedMode;
        uniquePowerupID = 0;
    }

    public void Move(string dir, SocketState state)
    {
        lock (snakeLock)
        {
            snakes[state].SetDirection(dir);
        }
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
        lock (powerupLock)
        {
            return powerups.Values;
        }
    }

    public bool GetSpeedMode()
    {
        return speedMode;
    }

    public void AddSnake(string name, int id, SocketState state)
    {
        lock (snakeLock)
        {
            Snake newSnake = new Snake(id, name, settings.UniverseSize, this);
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
        }
        lock (powerupLock)
        {
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

    public List<string> GetSerializedObjects()
    {
        List<string> objects = new();
        lock (snakeLock)
        {
            foreach (Snake s in snakes.Values)
            {
                string snakeString = JsonSerializer.Serialize(s);

                if (s.died)
                {
                    s.died = false;
                }
                objects.Add(snakeString);
            }
        }

        lock (powerupLock)
        {
            // List of powerups to remove
            List<Powerup> removeList = new();

            foreach (Powerup p in powerups.Values)
            {
                string powerupString = JsonSerializer.Serialize(p);

                if (p.died)
                {
                    removeList.Add(p);
                }

                objects.Add(powerupString);
            }

            foreach (Powerup p in removeList)
            {
                powerups.Remove(p.power);
            }
        }

        return objects;
    }

    public string Disconnect(SocketState state)
    {
        string s;
        lock (snakeLock)
        {
            snakes[state].dc = true;

            s = JsonSerializer.Serialize(snakes[state]);

            snakes.Remove(state);
        }

        return s;
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

    /// <summary>
    /// Helper method for calculating the four corner points given a rectange and a size.
    /// </summary>
    /// <param name="head"></param>
    /// <param name="tail"></param>
    /// <param name="size"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Helper method for determining if two rectangles overlap. Head is the beginning point, Tail is the end point, and
    /// size is the radius.
    /// </summary>
    /// <param name="head1"></param>
    /// <param name="tail1"></param>
    /// <param name="size1"></param>
    /// <param name="head2"></param>
    /// <param name="tail2"></param>
    /// <param name="size2"></param>
    /// <returns></returns>
    public static bool CollisionRectangle(Vector2D head1, Vector2D tail1, int size1, Vector2D head2, Vector2D tail2, int size2)
    {
        List<Vector2D> snakePoints = World.CalculatePoint(head1, tail1, size1);
        List<Vector2D> rectanglePoints = World.CalculatePoint(head2, tail2, size2);

        foreach (Vector2D point in snakePoints)
        {
            if (rectanglePoints[0].X <= point.X && point.X <= rectanglePoints[1].X)
            {
                if (rectanglePoints[0].Y <= point.Y && point.Y <= rectanglePoints[2].Y)
                {
                    return true;
                }
            }
        }
        foreach (Vector2D point in rectanglePoints)
        {
            if (snakePoints[0].X <= point.X && point.X <= snakePoints[1].X)
            {
                if (snakePoints[0].Y <= point.Y && point.Y <= snakePoints[2].Y)
                {
                    return true;
                }
            }
        }
        if (snakePoints[0].X >= rectanglePoints[0].X && snakePoints[0].X <= rectanglePoints[1].X && snakePoints[0].Y <= rectanglePoints[0].Y)
        {
            if (snakePoints[3].Y >= rectanglePoints[0].Y)
            {
                return true;
            }
        }
        if (snakePoints[1].X >= rectanglePoints[0].X && snakePoints[1].X <= rectanglePoints[1].X && snakePoints[1].Y <= rectanglePoints[0].Y)
        {
            if (snakePoints[2].Y >= rectanglePoints[1].Y)
            {
                return true;
            }
        }
        if (rectanglePoints[0].X >= snakePoints[0].X && rectanglePoints[0].X <= snakePoints[1].X && rectanglePoints[0].Y <= snakePoints[0].Y)
        {
            if (rectanglePoints[3].Y >= snakePoints[0].Y)
            {
                return true;
            }
        }
        if (rectanglePoints[1].X >= snakePoints[0].X && rectanglePoints[1].X <= snakePoints[1].X && rectanglePoints[1].Y <= snakePoints[0].Y)
        {
            if (rectanglePoints[2].Y >= snakePoints[1].Y)
            {
                return true;
            }
        }
        return false;
    }
}
