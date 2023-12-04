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

        private World? world;

        // The segments list where (item1, item2) are the coordinates of the beginning of the segment
        // and (item3, item4) are the coordinates of the end of the segment
        private List<Tuple<float, float, float, float>> segments;

        // The speed of the snake
        private int speed = 6;
        private int powerupFrames = 24;

        private int currentRespawnFrames;

        private bool eatenPowerup;
        private int currentPowerupFrames;

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
            world = null;

            segments = new();
            currentRespawnFrames = 0;

            eatenPowerup = false;
            powerupFrames = 0;

            // Add all of the segments to the segments list
            for (int i = 0; i < body.Count - 1; i++)
            {
                segments.Add(new Tuple<float, float, float, float>((float)body[i].GetX(), (float)body[i].GetY(), (float)body[i + 1].GetX(), (float)body[i + 1].GetY()));
            }
        }

        public Snake(int id, string name, IEnumerable<Snake> snakes, IEnumerable<Wall> walls, IEnumerable<Powerup> powerups, string uLock, int worldSize, World world)
        {
            snake = id;
            this.name = name;
            this.world = world;

            // Default values to be changed later by GenerateBody method
            body = new();
            dir = new();
            segments = new();

            GenerateBody(snakes, walls, powerups, uLock, worldSize);

            currentRespawnFrames = 0;
            score = 0;
            died = false;
            alive = true;
            dc = false;
            join = true;
        }

        /// <summary>
        /// Helper method for generating a body of the snake for spawning and respawning.
        /// </summary>
        /// <param name="snakes"></param>
        /// <param name="walls"></param>
        /// <param name="powerups"></param>
        /// <param name="uLock"></param>
        /// <param name="worldSize"></param>
        private void GenerateBody(IEnumerable<Snake> snakes, IEnumerable<Wall> walls, IEnumerable<Powerup> powerups, string uLock, int worldSize)
        {
            // Initialize body and segments
            body = new();
            segments = new();

            // Choose a random direction for the snake to look in
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

            // Lock so that two snakes can't respawn at the same time
            lock (uLock)
            {
                int sideLength = (worldSize - 240) / 120;
                int numOfSquares = sideLength * sideLength;

                // Generate a random 120 by 120 square
                random = new Random();
                int randomSquare = (int)Math.Floor(random.NextDouble() * numOfSquares);


                // Try to create snake. If fails, increment to the next square and retry
                while (true)
                {
                    int rowOffset = (120 * (randomSquare / sideLength) + 120) - (worldSize / 2);
                    int colOffset = (120 * (randomSquare % sideLength) + 120) - (worldSize / 2);

                    for (int i = 0; i < 120; i++)
                    {
                        for (int j = 0; j < 120; j++)
                        {

                            Vector2D head = new Vector2D(i + rowOffset, j +colOffset);

                            // If there are collisions, continue loop
                            if (CheckForCollisionsBody(snakes, walls, powerups, head))
                            {
                                continue;
                            }

                            // If successful, add the new body
                            body.Add((new Vector2D(i + rowOffset, j + colOffset)) - (dir*120));
                            body.Add(new Vector2D(i + rowOffset, j + colOffset));

                            // Add all of the segments to the segments list
                            for (int k = 0; k < body.Count - 1; k++)
                            {
                                segments.Add(new Tuple<float, float, float, float>((float)body[k].GetX(), (float)body[k].GetY(), (float)body[k + 1].GetX(), (float)body[k + 1].GetY()));
                            }

                            return;
                        }
                    }

                    randomSquare = (randomSquare + 1) % numOfSquares;
                }
            
            }
        }

        /// <summary>
        /// Helper method for determining whether a tested segment will spawn with collisions.
        /// </summary>
        /// <param name="snakes"></param>
        /// <param name="walls"></param>
        /// <param name="powerups"></param>
        /// <param name="collisionVectors"></param>
        /// <returns></returns>
        private bool CheckForCollisions(IEnumerable<Snake> snakes, IEnumerable<Wall> walls, IEnumerable<Powerup> powerups, Vector2D head)
        {
            lock (world!.GetSnakeLock())
            {
                // Check for wall collisions
                foreach (Wall w in walls)
                {
                    if (w.Collision(head, dir))
                    {
                        return true;
                    }
                }

                // Check for snake collisions
                foreach (Snake s in snakes)
                {
                    if (s.Collision(head, dir))
                    {
                        return true;
                    }
                }
                return false;
            }
            
        }

        private bool CollisionPowerup(Vector2D head)
        {
            IEnumerable<Powerup> powerups = world!.GetPowerups();
            // Check for powerup collisions
            foreach (Powerup p in powerups)
            {
                if (p.CollisionRectangle(head, head))
                {
                    p.died = true;
                    return true;
                }
            }

            return false;
        }
        private bool CheckForCollisionsBody(IEnumerable<Snake> snakes, IEnumerable<Wall> walls, IEnumerable<Powerup> powerups, Vector2D head)
        {
            lock (world!.GetSnakeLock())
            {
                // Check for wall collisions
                foreach (Wall w in walls)
                {
                    if (w.CollisionRectangle(head, (dir * -120) + head))
                    {
                        return true;
                    }
                }

                // Check for snake collisions
                foreach (Snake s in snakes)
                {
                    if (s.CollisionRectangle(head, (dir * -120) + head))
                    {
                        return true;
                    }
                }

                // Check for powerup collisions
                foreach (Powerup p in powerups)
                {
                    if (p.CollisionRectangle(head, (dir * -120) + head))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool Collision(Vector2D head, Vector2D dir)
        {
            if (!alive)
            {
                return false;
            }

            Vector2D topOfHead = head + (dir * 10);

            for (int i = 0; i < body.Count - 1; i++)
            {
                if (body[i].X == body[i + 1].X)
                {
                    if (body[i].Y > body[i + 1].Y)
                    {
                        if (body[i + 1].Y - 5 < topOfHead.Y && topOfHead.Y < body[i].Y + 5)
                        {
                            if (body[i].X - 5 < topOfHead.X && topOfHead.X < body[i].X + 5)
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (body[i].Y - 5 < topOfHead.Y && topOfHead.Y < body[i + 1].Y + 5)
                        {
                            if (body[i].X - 5 < topOfHead.X && topOfHead.X < body[i].X + 5)
                            {
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    if (body[i].X > body[i + 1].X)
                    {
                        if (body[i + 1].X - 5 < topOfHead.X && topOfHead.X < body[i].X + 5)
                        {
                            if (body[i].Y - 5 < topOfHead.Y && topOfHead.Y < body[i].Y + 5)
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (body[i].X - 5 < topOfHead.X && topOfHead.X < body[i + 1].X + 5)
                        {
                            if (body[i].Y - 5 < topOfHead.Y && topOfHead.Y < body[i].Y + 5)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public bool CollisionRectangle(Vector2D head, Vector2D tail)
        {
            if (!alive)
            {
                return false;
            }

            for (int i = 0; i < body.Count - 1; i++)
            {
                List<Vector2D> snakePoints = World.CalculatePoint(head, tail, 5);
                List<Vector2D> rectanglePoints = World.CalculatePoint(body[i], body[i+1], 5);
                foreach (Vector2D point in snakePoints)
                {
                    if (rectanglePoints[0].X < point.X && point.X < rectanglePoints[1].X)
                    {
                        if (rectanglePoints[0].Y < point.Y && point.Y < rectanglePoints[2].Y)
                        {
                            return true;
                        }
                    }
                }
                foreach (Vector2D point in rectanglePoints)
                {
                    if (snakePoints[0].X < point.X && point.X < snakePoints[1].X)
                    {
                        if (snakePoints[0].Y < point.Y && point.Y < snakePoints[2].Y)
                        {
                            return true;
                        }
                    }
                }
                if (snakePoints[0].X > rectanglePoints[0].X && snakePoints[0].X < rectanglePoints[1].X && snakePoints[0].Y < rectanglePoints[0].Y)
                {
                    if (snakePoints[3].Y > rectanglePoints[0].Y)
                    {
                        return true;
                    }
                }
                if (rectanglePoints[0].X > snakePoints[0].X && rectanglePoints[0].X < snakePoints[1].X && rectanglePoints[0].Y < snakePoints[0].Y)
                {
                    if (rectanglePoints[3].Y > snakePoints[0].Y)
                    {
                        return true;
                    }
                }
            }

            return false;
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
            if (!alive)
            {
                return;
            }
            if (dir == "none")
            {
                return;
            }

            Vector2D currentDirection = body[body.Count - 1] - body[body.Count - 2];
            currentDirection.Clamp();

            switch (dir)
            {
                case "left":
                    if (!currentDirection.Equals(new Vector2D(1, 0)))
                    {
                        this.dir = new Vector2D(-1, 0);
                    }
                    break;
                case "right":
                    if (!currentDirection.Equals(new Vector2D(-1, 0)))
                    {
                        this.dir = new Vector2D(1, 0);
                    }
                    break;
                case "up":
                    if (!currentDirection.Equals(new Vector2D(0, 1)))
                    {
                        this.dir = new Vector2D(0, -1);
                    }
                    break;
                case "down":
                    if (!currentDirection.Equals(new Vector2D(0, -1)))
                    {
                        this.dir = new Vector2D(0, 1);
                    }
                    break;
            }
        }

        public void AddDeadFrame()
        {
            currentRespawnFrames += 1;

            if (currentRespawnFrames == world!.GetRespawnFrames())
            {
                currentRespawnFrames = 0;
                alive = true;

                GenerateBody(world.GetSnakes(), world.GetWalls(), world.GetPowerups(), world.GetSnakeLock(), world.GetWorldSize());
                score = 0;
            }
        }

        /// <summary>
        /// Updates the Snake's body position
        /// </summary>
        public void Update()
        {
            Vector2D currentDirection = body[body.Count - 1] - body[body.Count - 2];
            currentDirection.Clamp();

            bool headChanged = !currentDirection.Equals(dir);

            // If the head changed directions, add a new head
            if (headChanged)
            {
                Vector2D newHead = body[body.Count - 1] + (dir * speed);
                body.Add(newHead);
            }
            // If the head did not change directions, change the head value
            else
            {
                body[body.Count - 1] += (dir * speed);
            }

            if (!eatenPowerup)
            {
                Vector2D tail = body[0];
                Vector2D afterTail = body[1];

                Vector2D normalized = (afterTail - tail);
                normalized.Clamp();

                Vector2D newTail = tail + (normalized * speed);

                if (newTail.Equals(afterTail))
                {
                    body.RemoveAt(0);
                }
                else
                {
                    body.RemoveAt(0);
                    body.Insert(0, newTail);
                }
            }

            // If it has eaten a powerup, don't move the tail
            else
            {
                currentPowerupFrames += 1;

                if (currentPowerupFrames == powerupFrames)
                {
                    currentPowerupFrames = 0;
                    eatenPowerup = false;
                }
            }
            

            // Check for collisions
            if (CheckForCollisions(world!.GetSnakes(), world.GetWalls(), world.GetPowerups(), body[body.Count - 1]))
            {
                died = true;
                alive = false;
                eatenPowerup = false;
                score = 0;
                currentPowerupFrames = 0;
            }

            // If there is a powerup collision, set the eaten powerup field to true
            if (CollisionPowerup(body[body.Count - 1]))
            {
                eatenPowerup = true;
                currentPowerupFrames = 0;
                score += 1;
            }
        }
    }
}
