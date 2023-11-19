// Authors: Ethan Andrews and Mary Garfield
// Class for the snakes.
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
    /// Class for storing snake data.
    /// </summary>
    public class Snake
    {
        [JsonInclude]
        public int snake;
        [JsonInclude]
        public string name;
        [JsonInclude]
        public List<Vector2D> body;
        [JsonInclude]
        public Vector2D dir;
        [JsonInclude]
        public int score;
        [JsonInclude]
        public bool died;
        [JsonInclude]
        public bool alive;
        [JsonInclude]
        public bool dc;
        [JsonInclude]
        public bool join;

        // The segments list where (item1, item2) are the coordinates of the beginning of the segment
        // and (item3, item4) are the coordinates of the end of the segment
        private List<Tuple<float, float, float, float>> segments;

        [JsonConstructor]
        // Constructor for json objects
        public Snake(int snake, string name, List<Vector2D> body, Vector2D dir, int score, bool died, bool alive, bool dc, bool join)
        {
            this.snake = snake;
            this.name = name;
            this.body = body;
            this.dir = dir;
            this.score = score;
            this.died = died;
            this.alive = alive;
            this.dc = dc;
            this.join = join;

            segments = new();

            // Add all of the segments to the segments list
            for (int i = 0; i < body.Count - 1; i++)
            {
                segments.Add(new Tuple<float, float, float, float>((float)body[i].GetX(), (float)body[i].GetY(), (float)body[i + 1].GetX(), (float)body[i + 1].GetY()));
            }
        } 

        /// <summary>
        /// Returns the segments list
        /// </summary>
        /// <returns></returns>
        public List<Tuple<float, float, float, float>> GetSegments()
        {
            return segments;
        }

        /// <summary>
        /// Returns the name of the snake.
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return name;
        }

        /// <summary>
        /// Returns the score of the snake.
        /// </summary>
        /// <returns></returns>
        public int GetScore()
        {
            return score;
        }

        /// <summary>
        /// Returns the head location of the snake.
        /// </summary>
        /// <returns></returns>
        public Vector2D GetHead()
        {
            return body[body.Count - 1];
        }

        /// <summary>
        /// Returns the string representation of the direction of the snake.
        /// </summary>
        /// <returns></returns>
        public string GetDir()
        {
            if (dir.X == -1)
            {
                return "left";
            }
            else if (dir.X == 1)
            {
                return "right";
            }
            else if (dir.Y == -1)
            {
                return "up";
            }
            else
            {
                return "down";
            }
        }
    }
}
