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

        // The speed of the snake
        //private int speed = 6;

        private bool headChanged;

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

        public Snake(int id, string name, IEnumerable<Snake> snakes, HashSet<Tuple<int, int>> wallLocations, IEnumerable<Powerup> powerups, string uLock, int worldSize)
        {
            snake = id;
            this.name = name;
            headChanged = false;

            // Generate Body
            // Generate Segments
            // Generate Direction
            body = new();
            segments = new();
            Random random = new Random();

            int newDirection = (int)Math.Floor(random.NextDouble() * 4);

            switch (newDirection)
            {
                case 0: // Up
                    this.dir = new Vector2D(0, -1);
                    break;

                case 1: // Down
                    this.dir = new Vector2D(0, 1);
                    break;

                case 2: // Left
                    this.dir = new Vector2D(-1, 0);
                    break;

                default: // Right
                    this.dir = new Vector2D(1, 0);
                    break;
            }

            GenerateBody(snakes, wallLocations, powerups, uLock, worldSize);

            score = 0;
            died = false;
            alive = true;
            dc = false;
            join = true;
        }

        private void GenerateBody(IEnumerable<Snake> snakes, HashSet<Tuple<int, int>> wallLocations, IEnumerable<Powerup> powerups, string uLock, int worldSize)
        {
            // Lock so that two snakes can't respawn at the same time
            lock (uLock)
            {
                int sideLength = (worldSize - 10) / 120;
                int numOfSquares = sideLength * sideLength;

                // Generate a random 120 by 120 square
                Random random = new Random();
                int randomSquare = (int)Math.Floor(random.NextDouble() * numOfSquares);

                HashSet<Tuple<int, int>> respawnable = new HashSet<Tuple<int, int>>();

                // Add snake points to the respawnable set
                foreach (Snake s in snakes)
                {
                    foreach (Tuple<float, float, float, float> segment in s.GetSegments())
                    {
                        if (segment.Item1 == segment.Item2)
                        {
                            if (segment.Item3 > segment.Item4)
                            {
                                for (int k = (int)segment.Item4; k <= (int)segment.Item3; k++)
                                {
                                    respawnable.Add(new Tuple<int, int>((int)segment.Item1, k));
                                }
                            }

                            else
                            {
                                for (int k = (int)segment.Item3; k <= (int)segment.Item4; k++)
                                {
                                    respawnable.Add(new Tuple<int, int>((int)segment.Item1, k));
                                }
                            }
                        }

                        else
                        {
                            if (segment.Item1 > segment.Item2)
                            {
                                for (int k = (int)segment.Item2; k <= (int)segment.Item1; k++)
                                {
                                    respawnable.Add(new Tuple<int, int>(k, (int)segment.Item3));
                                }
                            }

                            else
                            {
                                for (int k = (int)segment.Item1; k <= (int)segment.Item2; k++)
                                {
                                    respawnable.Add(new Tuple<int, int>(k, (int)segment.Item3));
                                }
                            }
                        }
                    }
                }

                foreach (Powerup p in powerups)
                {
                    respawnable.Add(new Tuple<int, int>((int)p.GetLocation().X, (int)p.GetLocation().Y));
                }

                // Try to create snake. If fails, increment to the next square and retry
                while (true)
                {
                    int rowOffset = 120 * (randomSquare / sideLength);
                    int colOffset = 120 * (randomSquare % sideLength);

                    // If snake direction is up
                    if (dir.Equals(new Vector2D(0, -1)))
                    {
                        for (int i = 0; i < 120; i++)
                        {
                            for (int j = 0; j < 120; j++)
                            {
                                bool found = true;

                                for (int k = 0; k < 125; k++)
                                {
                                    // If there are things in the way, keep searching
                                    if (respawnable.Contains(new Tuple<int, int>(colOffset + i, rowOffset + j - 5 + k)) || wallLocations.Contains(new Tuple<int, int>(colOffset + i, rowOffset + j - 5 + k)))
                                    {
                                        found = false;
                                        continue;
                                    }
                                }

                                if (found)
                                {
                                    body.Add(new Vector2D(i, j));

                                    // Add all of the segments to the segments list
                                    for (int h = 0; h < body.Count - 1; h++)
                                    {
                                        segments.Add(new Tuple<float, float, float, float>((float)body[h].GetX(), (float)body[h].GetY(), (float)body[h + 1].GetX(), (float)body[h + 1].GetY()));
                                    }
                                    return;
                                }
                            }
                        }
                    }

                    // If snake direction is down
                    else if (dir.Equals(new Vector2D(0, 1)))
                    {
                        for (int i = 0; i < 120; i++)
                        {
                            for (int j = 0; j < 120; j++)
                            {
                                bool found = true;

                                for (int k = 0; k < 125; k++)
                                {
                                    // If there are things in the way, keep searching
                                    if (respawnable.Contains(new Tuple<int, int>(colOffset + i, rowOffset + j + 5 - k)) || wallLocations.Contains(new Tuple<int, int>(colOffset + i, rowOffset + j + 5 - k)))
                                    {
                                        found = false;
                                        continue;
                                    }
                                }

                                if (found)
                                {
                                    body.Add(new Vector2D(i, j));

                                    // Add all of the segments to the segments list
                                    for (int h = 0; h < body.Count - 1; h++)
                                    {
                                        segments.Add(new Tuple<float, float, float, float>((float)body[h].GetX(), (float)body[h].GetY(), (float)body[h + 1].GetX(), (float)body[h + 1].GetY()));
                                    }
                                    return;
                                }
                            }
                        }
                    }

                    // If snake direction is left
                    else if (dir.Equals(new Vector2D(-1, 0)))
                    {
                        for (int i = 0; i < 120; i++)
                        {
                            for (int j = 0; j < 120; j++)
                            {
                                bool found = true;

                                for (int k = 0; k < 125; k++)
                                {
                                    // If there are things in the way, keep searching
                                    if (respawnable.Contains(new Tuple<int, int>(colOffset + i + 5 - k, rowOffset + j)) || wallLocations.Contains(new Tuple<int, int>(colOffset + i + 5 - k, rowOffset + j)))
                                    {
                                        found = false;
                                        continue;
                                    }
                                }

                                if (found)
                                {
                                    body.Add(new Vector2D(i, j));

                                    // Add all of the segments to the segments list
                                    for (int h = 0; h < body.Count - 1; h++)
                                    {
                                        segments.Add(new Tuple<float, float, float, float>((float)body[h].GetX(), (float)body[h].GetY(), (float)body[h + 1].GetX(), (float)body[h + 1].GetY()));
                                    }
                                    return;
                                }
                            }
                        }
                    }

                    // If snake direction is right
                    else
                    {
                        for (int i = 0; i < 120; i++)
                        {
                            for (int j = 0; j < 120; j++)
                            {
                                bool found = true;

                                for (int k = 0; k < 125; k++)
                                {
                                    // If there are things in the way, keep searching
                                    if (respawnable.Contains(new Tuple<int, int>(colOffset + i - 5 + k, rowOffset + j)) || wallLocations.Contains(new Tuple<int, int>(colOffset + i - 5 + k, rowOffset + j)))
                                    {
                                        found = false;
                                        continue;
                                    }
                                }

                                if (found)
                                {
                                    body.Add(new Vector2D(i, j));

                                    // Add all of the segments to the segments list
                                    for (int h = 0; h < body.Count - 1; h++)
                                    {
                                        segments.Add(new Tuple<float, float, float, float>((float)body[h].GetX(), (float)body[h].GetY(), (float)body[h + 1].GetX(), (float)body[h + 1].GetY()));
                                    }
                                    return;
                                }
                            }
                        }
                    }

                    randomSquare = (randomSquare + 1) % numOfSquares;
                }
            
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

        public void SetDirection(string dir)
        {
            if (dir == "none")
            {
                return;
            }

            string currentDirection = GetDir();

            switch (dir)
            {
                case "left":
                    if (currentDirection != "right")
                    {
                        this.dir = new Vector2D(-1, 0);
                        headChanged = dir == currentDirection;
                    }
                    break;
                case "right":
                    if (currentDirection != "left")
                    {
                        this.dir = new Vector2D(1, 0);
                        headChanged = dir == currentDirection;
                    }
                    break;
                case "up":
                    if (currentDirection != "down")
                    {
                        this.dir = new Vector2D(0, -1);
                        headChanged = dir == currentDirection;
                    }
                    break;
                case "down":
                    if (currentDirection != "up")
                    {
                        this.dir = new Vector2D(0, 1);
                        headChanged = dir == currentDirection;
                    }
                    break;
            }
        }

        public void Move()
        {
            Vector2D tail = body[0];
            Vector2D afterTail = body[1];
            
            // If the head changed directions, add a new head
            if (headChanged)
            {
                Vector2D newHead = body[body.Count - 1] + (dir * 6);
                body.Add(newHead);

                // Set head changed back to false
                headChanged = false;
            }
            // If the head did not change directions, change the head
            else
            {
                body[body.Count - 1] += (dir * 6);
            }

        }
    }
}
