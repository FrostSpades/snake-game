using System.Text.Json.Serialization;
using SnakeGame;

namespace Model
{

    public class Wall
    {
        [JsonInclude]
        public int wall;

        [JsonInclude]
        public Vector2D p1, p2;
        
        private List<Tuple<double, double>> segments;

        [JsonConstructor]
        public Wall(int wall, Vector2D p1, Vector2D p2)
        {
            this.wall = wall;
            this.p1 = p1;
            this.p2 = p2;
            segments = new();

            if (p1.X == p2.X)
            {
                if (p2.Y > p1.Y)
                {
                    for (int i = 0; i <= (p2.Y - p1.Y) / 50; i++)
                    {
                        segments.Add(Tuple.Create(p1.X - 25, p1.Y + i * 50 - 25));
                    }

                }
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

                if (p2.X > p1.X)
                {
                    for (int i = 0; i <= (p2.X - p1.X) / 50; i++)
                    {
                        segments.Add(Tuple.Create(p1.X + 50 * i - 25, p1.Y - 25));
                    }

                }
                else if (p1.X > p2.X)
                {
                    for (int i = 0; i >= (p2.X - p1.X) / 50; i--)
                    {
                        segments.Add(Tuple.Create(p1.X + 50 * i - 25, p1.Y - 25));

                    }


                }
            }

        }
    public List<Tuple<double, double>> GetSegments()
        {
            return segments;
        }
    }
}
