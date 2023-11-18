using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SnakeGame;

namespace Model
{
    public class ControlCommand
    {
        [JsonInclude]
        public string moving;

        public ControlCommand(string direction) 
        {
            moving = direction;
        }
    }
}
