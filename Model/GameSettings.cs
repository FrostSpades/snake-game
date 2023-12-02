using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Model;

[DataContract(Namespace="")]
public class GameSettings
{
    [DataMember]
    public int MSPerFrame;
    [DataMember]
    public int RespawnRate;
    [DataMember]
    public int UniverseSize;
    [DataMember]
    public List<Wall> Walls;

    [IgnoreDataMember]
    public Dictionary<int, Wall> walls;

    public GameSettings()
    {
        MSPerFrame = 0;
        RespawnRate = 0;
        UniverseSize = 0;
        Walls = new();
        walls = new();
    }

    //public GameSettings(int MSPerFrame, int RespawnRate, int WorldSize, List<Wall> Walls)
    //{
    //    this.MSPerFrame = MSPerFrame;
    //    this.RespawnRate = RespawnRate;
    //    this.WorldSize = WorldSize;
    //    this.Walls = Walls;
    //}
}
