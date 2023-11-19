// Authors: Ethan Andrews and Mary Garfield
// Class for sending commands to the server.
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
    /// Class for sending commands to the server in json format.
    /// </summary>
    public class ControlCommand
    {
        [JsonInclude]
        public string moving;

        /// <summary>
        /// Constructor that assigns the direction to the field.
        /// </summary>
        /// <param name="direction"></param>
        public ControlCommand(string direction) 
        {
            moving = direction;
        }
    }
}
