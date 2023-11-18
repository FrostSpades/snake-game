using SnakeGame;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Xml.Linq;

namespace Model
{
    public class Model
    {
        private int snakeID;
        private int worldSize;
        private Dictionary<int, Wall> walls;
        private Dictionary<int, Snake> snakes;
        private Dictionary<int, Powerup> powerups;

        private Snake? player;
        private string snakeLock;

        public Model()
        {
            walls = new();
            snakes = new();
            powerups = new();
            worldSize = 0;
            snakeLock = "snakeLock";
        }

        public void SetWorldSize(string size)
        {
            if (int.TryParse(size, out int n))
            {
                worldSize = n;
            }
        }

        public int GetWorldSize()
        {
            return worldSize;
        }

        public void AddWall(string wall)
        {
            Wall? rebuilt = JsonSerializer.Deserialize<Wall>(wall);

            if (rebuilt != null)
            {
                walls.Add(rebuilt.wall, rebuilt);
            }
        }

        public void AddSnake(string snake)
        {
            Snake? rebuilt = JsonSerializer.Deserialize<Snake>(snake);

            lock (snakeLock)
            {
                if (rebuilt != null)
                {
                    if (snakes.ContainsKey(rebuilt.snake))
                    {
                        if (!rebuilt.alive)
                        {
                            snakes.Remove(rebuilt.snake);
                        }
                        else
                        {
                            snakes[rebuilt.snake] = rebuilt;
                        }
                    }
                    else
                    {
                        if (rebuilt.alive)
                        {
                            snakes.Add(rebuilt.snake, rebuilt);
                        }

                    }

                    if (rebuilt.snake == snakeID)
                    {
                        player = rebuilt;
                    }
                }
            }
            
        }

        public Vector2D? GetHead()
        {
            lock (snakeLock)
            {
                if (player == null)
                {
                    return null;
                }

                else
                {
                    return player.body[player.body.Count - 1];
                }
            }
        }

        public void AddPowerup(string powerup)
        {
            Powerup? rebuilt = JsonSerializer.Deserialize<Powerup>(powerup);

            lock (powerups)
            {
                if (rebuilt != null)
                {
                    if (powerups.ContainsKey(rebuilt.power))
                    {
                        if (rebuilt.died)
                        {
                            powerups.Remove(rebuilt.power);
                        }
                        else
                        {
                            powerups[rebuilt.power] = rebuilt;
                        }
                    }
                    else
                    {
                        if (!rebuilt.died)
                        {
                            powerups.Add(rebuilt.power, rebuilt);
                        }
                    }
                }
            }
            
        }

        public void SetID(string id)
        {
            if (int.TryParse(id, out int n))
            {
                snakeID = n;
            }
        }

        public IEnumerable<Snake> GetSnakes()
        {
            lock (snakeLock)
            {
                return snakes.Values;
            }
            
        }

        public IEnumerable<Wall> GetWalls()
        {
            return walls.Values;
        }

        public IEnumerable<Powerup> GetPowerups()
        {
            return powerups.Values;
        }

        public string GetSnakeLock()
        {
            return snakeLock;
        }
    }
}