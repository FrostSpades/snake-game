using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Model;
internal class GameSettings
{
    [JsonInclude]
    public int MSPerFrame;
    [JsonInclude]
    public int RespawnRate;
    [JsonInclude]
    public int WorldSize;
    [JsonInclude]
    public Dictionary<int, Wall> walls;

    [JsonConstructor]
    public GameSettings(int MSPerFrame, int RespawnRate, int WorldSize, Dictionary<int, Wall> walls)
    {
        this.MSPerFrame = MSPerFrame;
        this.RespawnRate = RespawnRate;
        this.WorldSize = WorldSize;
        this.walls = walls;
    }
}
