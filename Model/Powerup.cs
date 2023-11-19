// Authors: Ethan Andrews and Mary Garfield
// Class for powerups
// University of Utah

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SnakeGame;

namespace Model
{
    /// <summary>
    /// Class for powerups
    /// </summary>
    public class Powerup
    {
        [JsonInclude]
        public int power;
        [JsonInclude]
        public Vector2D loc;
        [JsonInclude]
        public bool died;

        [JsonConstructor]
        // Constructor for json
        public Powerup(int power, Vector2D loc, bool died) 
        {
            this.power = power;
            this.loc = loc;
            this.died = died;
        }

        /// <summary>
        /// Returns the location of the powerup.
        /// </summary>
        /// <returns></returns>
        public Vector2D GetLocation()
        {
            return loc;
        }
    }
}
