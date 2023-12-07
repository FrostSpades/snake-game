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
    [DataMember]
    public bool SpeedMode;

    public GameSettings()
    {
        MSPerFrame = 0;
        RespawnRate = 0;
        UniverseSize = 0;
        SpeedMode = false;
        Walls = new();
    }
}
