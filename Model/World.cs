using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Model;
public class World
{
    GameSettings settings;
    Dictionary<int, Snake> snakes;

    public World()
    {
        snakes = new Dictionary<int, Snake>();
        string filePath = "settings.xml";
        string jsonContent = File.ReadAllText(filePath);
        GameSettings? tempSettings = JsonSerializer.Deserialize<GameSettings>(jsonContent);

        if (tempSettings == null )
        {
            throw new FileNotFoundException("The settings file was not found at " + filePath);
        }

        settings = tempSettings;
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
    public int AddSnake()
    {
        return 0;
    }
    public int GetWorldSize()
    {
        return settings.WorldSize;
    }
}
