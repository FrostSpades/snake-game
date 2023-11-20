// Authors: Ethan Andrews and Mary Garfield
// Model for storing objects.
// University of Utah

using SnakeGame;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Xml.Linq;

namespace Model
{
    /// <summary>
    /// Class for storing objects for the snake game.
    /// </summary>
    public class Model
    {
        // ID of the player snake
        private int snakeID;
        
        // Size of the world
        private int worldSize;

        // Hash sets for storing the game objects
        private Dictionary<int, Wall> walls;
        private Dictionary<int, Snake> snakes;
        private Dictionary<int, Powerup> powerups;

        // Snake object of the player
        private Snake? player;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Model()
        {
            walls = new();
            snakes = new();
            powerups = new();
            worldSize = 0;
        }

        /// <summary>
        /// Set the world size given an input string.
        /// </summary>
        /// <param name="size"></param>
        public void SetWorldSize(string size)
        {
            if (int.TryParse(size, out int n))
            {
                worldSize = n;
            }
        }

        /// <summary>
        /// Returns the world size
        /// </summary>
        /// <returns></returns>
        public int GetWorldSize()
        {
            return worldSize;
        }

        /// <summary>
        /// Adds a wall object to the game given a json string.
        /// </summary>
        /// <param name="wall"></param>
        public void AddWall(string wall)
        {
            Wall? rebuilt = JsonSerializer.Deserialize<Wall>(wall);

            
            if (rebuilt != null)
            {
                walls.Add(rebuilt.wall, rebuilt);
            }
        }

        /// <summary>
        /// Adds a snake object to the game given a json string.
        /// </summary>
        /// <param name="snake"></param>
        public void AddSnake(string snake)
        {
            Snake? rebuilt = JsonSerializer.Deserialize<Snake>(snake);

            if (rebuilt != null)
            {
                // If the snake id is already in the dict, replace existing snake
                if (snakes.ContainsKey(rebuilt.snake))
                {
                    if (rebuilt.dc)
                    {
                        snakes.Remove(rebuilt.snake);
                    }
                    else
                    {
                        snakes[rebuilt.snake] = rebuilt;
                    }
                        
                }

                // If snake id is not yet already in dict, add it
                else
                {
                    if (!rebuilt.dc)
                    {
                        snakes.Add(rebuilt.snake, rebuilt);
                    }
                }

                // Set player equal to the most current snake with the same snake id
                if (rebuilt.snake == snakeID)
                {
                    player = rebuilt;
                }
            }
            
        }

        /// <summary>
        /// Return the Vector2D of the head
        /// </summary>
        /// <returns></returns>
        public Vector2D? GetHead()
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

        /// <summary>
        /// Add a powerup to the game.
        /// </summary>
        /// <param name="powerup"></param>
        public void AddPowerup(string powerup)
        {
            Powerup? rebuilt = JsonSerializer.Deserialize<Powerup>(powerup);

            if (rebuilt != null)
            {
                // Replace object in dict if already contains
                if (powerups.ContainsKey(rebuilt.power))
                {
                    // If died, remove it from dictionary
                    if (rebuilt.died)
                    {
                        powerups.Remove(rebuilt.power);
                    }
                    else
                    {
                        powerups[rebuilt.power] = rebuilt;
                    }
                }
                // Add to dict if not yet contains and is not dead
                else
                {
                    if (!rebuilt.died)
                    {
                        powerups.Add(rebuilt.power, rebuilt);
                    }
                }
            }
            
        }

        /// <summary>
        /// Sets the id of the player
        /// </summary>
        /// <param name="id"></param>
        public void SetID(string id)
        {
            if (int.TryParse(id, out int n))
            {
                snakeID = n;
            }
        }

        /// <summary>
        /// Returns the snakes
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Snake> GetSnakes()
        {
            return snakes.Values;
            
        }

        /// <summary>
        /// Returns the walls
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Wall> GetWalls()
        {
            return walls.Values;
        }

        /// <summary>
        /// Returns the powerups
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Powerup> GetPowerups()
        {
            return powerups.Values;
        }
    }
}