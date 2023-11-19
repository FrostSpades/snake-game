// Authors: Ethan Andrews and Mary Garfield
// Class for the walls of the game.
// University of Utah
using System.Text.Json.Serialization;
using SnakeGame;

namespace Model
{
    /// <summary>
    /// Class for the walls of the game.
    /// </summary>
    public class Wall
    {
        [JsonInclude]
        public int wall;

        [JsonInclude]
        public Vector2D p1, p2;
        
        // List of the segments of the wall
        private List<Tuple<double, double>> segments;

        [JsonConstructor]
        // Constructor for json objects
        public Wall(int wall, Vector2D p1, Vector2D p2)
        {
            this.wall = wall;
            this.p1 = p1;
            this.p2 = p2;
            segments = new();

            // Adds the segments based on orientation
            if (p1.X == p2.X)
            {
                // Wall is facing up
                if (p2.Y > p1.Y)
                {
                    for (int i = 0; i <= (p2.Y - p1.Y) / 50; i++)
                    {
                        segments.Add(Tuple.Create(p1.X - 25, p1.Y + i * 50 - 25));
                    }

                }
                // Wall is facing down
                else if (p1.Y > p2.Y)
                {
                    for (int i = 0; i >= (p2.Y - p1.Y) / 50; i--)
                    {
                        segments.Add(Tuple.Create(p1.X - 25, p1.Y + i * 50 - 25));

                    }
                }
            }
            else
            {
                // Wall is facing right
                if (p2.X > p1.X)
                {
                    for (int i = 0; i <= (p2.X - p1.X) / 50; i++)
                    {
                        segments.Add(Tuple.Create(p1.X + 50 * i - 25, p1.Y - 25));
                    }

                }
                // Wall is facing left
                else if (p1.X > p2.X)
                {
                    for (int i = 0; i >= (p2.X - p1.X) / 50; i--)
                    {
                        segments.Add(Tuple.Create(p1.X + 50 * i - 25, p1.Y - 25));

                    }


                }
            }

        }

    /// <summary>
    /// Return the segments of the wall
    /// </summary>
    /// <returns></returns>
    public List<Tuple<double, double>> GetSegments()
        {
            return segments;
        }
    }
}
