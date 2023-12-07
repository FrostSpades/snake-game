// Authors: Ethan Andrews and Mary Garfield
// Class for the walls of the game.
// University of Utah
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using SnakeGame;

namespace Model
{
    /// <summary>
    /// Class for the walls of the game.
    /// </summary>

    [DataContract(Name ="Wall", Namespace ="")]
    public class Wall
    {
        [JsonInclude]
        [DataMember(Name="ID")]
        public int wall;

        [JsonInclude]
        [DataMember(Name="p1")]
        public Vector2D p1 { get; set; }

        [JsonInclude]
        [DataMember(Name="p2")]
        public Vector2D p2 { get; set; }

        [IgnoreDataMember]
        // List of the segments of the wall for the client
        private List<Tuple<double, double>> segments;

        /// <summary>
        /// Default constructor for xml
        /// </summary>
        public Wall()
        {
            wall = 0;
            p1 = new Vector2D();
            p2 = new Vector2D();

            // It is fine for this to be empty for the server because segments is not used by server.
            // It is only used by the client to simplify drawing.
            segments = new();
        }

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

        /// <summary>
        /// Simulate if there was a collision with this wall.
        /// </summary>
        /// <param name="head"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public bool Collision(Vector2D head, Vector2D dir)
        {
            // Generate snake head
            Vector2D topOfHead = head + (dir * 5);

            if (p1.X == p2.X)
            {
                if (p1.Y > p2.Y)
                {
                    if (p2.Y - 25 < topOfHead.Y && topOfHead.Y < p1.Y + 25)
                    {
                        if (p1.X - 25 < topOfHead.X && topOfHead.X < p1.X + 25)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (p1.Y - 25 < topOfHead.Y && topOfHead.Y < p2.Y + 25)
                    {
                        if (p1.X - 25 < topOfHead.X && topOfHead.X < p1.X + 25)
                        {
                            return true;
                        }
                    }
                }
            }
            else
            {
                if (p1.X > p2.X)
                {
                    if (p2.X - 25 < topOfHead.X && topOfHead.X < p1.X + 25)
                    {
                        if (p1.Y - 25 < topOfHead.Y && topOfHead.Y < p1.Y + 25)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (p1.X - 25 < topOfHead.X && topOfHead.X < p2.X + 25)
                    {
                        if (p1.Y - 25 < topOfHead.Y && topOfHead.Y < p1.Y + 25)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Simulates if a rectangle is colliding with this wall.
        /// </summary>
        /// <param name="head"></param>
        /// <param name="tail"></param>
        /// <returns></returns>
        public bool CollisionRectangle(Vector2D head, Vector2D tail)
        {
            // Simulate a collision between two rectangles
            return World.CollisionRectangle(head, tail, 5, p1, p2, 25);
        }
        
    }
}

    

