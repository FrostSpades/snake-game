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

    public World()
    {
        string filePath = "settings.xml";
        string jsonContent = File.ReadAllText(filePath);
        GameSettings? tempSettings = JsonSerializer.Deserialize<GameSettings>(jsonContent);

        if (tempSettings == null )
        {
            throw new FileNotFoundException("The settings file was not found at " + filePath);
        }

        settings = tempSettings;
    }
}
