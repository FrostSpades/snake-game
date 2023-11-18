using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SnakeGame;

namespace Model
{
    public class Powerup
    {
        [JsonInclude]
        public int power;
        [JsonInclude]
        public Vector2D loc;
        [JsonInclude]
        public bool died;

        [JsonConstructor]
        public Powerup(int power, Vector2D loc, bool died) 
        {
            this.power = power;
            this.loc = loc;
            this.died = died;
        }

        public Vector2D GetLocation()
        {
            return loc;
        }
    }
}
