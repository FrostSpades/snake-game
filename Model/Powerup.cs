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

        private World? world;

        [JsonConstructor]
        // Constructor for json
        public Powerup(int power, Vector2D loc, bool died) 
        {
            this.power = power;
            this.loc = loc;
            this.died = died;
            world = null;
        }

        /// <summary>
        /// Constructor for powerups.
        /// </summary>
        /// <param name="power"></param>
        /// <param name="world"></param>
        public Powerup(int power, World world)
        {
            this.power = power;
            this.died = false;
            this.world = world;
            loc = new Vector2D();

            RandomLocation();
        }

        /// <summary>
        /// Assigns a new random location.
        /// </summary>
        public void RandomLocation()
        {
            Random random = new Random();

            // Try and find a random location, and don't stop until found.
            while (true)
            {
                int x = (int)(random.NextDouble() * world!.GetWorldSize()) - (world.GetWorldSize() / 2);
                int y = (int)(random.NextDouble() * world!.GetWorldSize()) - (world.GetWorldSize() / 2);

                Vector2D newLoc = new Vector2D(x, y);

                // If there's a collision, search for a new location and try again
                if (CheckForCollisionsBody(newLoc))
                {
                    continue;
                }

                loc = newLoc;
                return;
            }
        }

        /// <summary>
        /// Method for checking if there was a collision.
        /// </summary>
        /// <param name="head"></param>
        /// <returns></returns>
        private bool CheckForCollisionsBody(Vector2D head)
        {
            IEnumerable<Snake> snakes = world!.GetSnakes();
            IEnumerable<Wall> walls = world.GetWalls();
            IEnumerable<Powerup> powerups = world.GetPowerups();

            // Check for wall collisions
            foreach (Wall w in walls)
            {
                if (w.CollisionRectangle(head, head))
                {
                    return true;
                }
            }

            // Check for snake collisions
            foreach (Snake s in snakes)
            {
                if (s.CollisionRectangle(head, head))
                {
                    return true;
                }
            }

            // Check for powerup collisions
            foreach (Powerup p in powerups)
            {
                if (p.CollisionRectangle(head, head))
                {
                    return true;
                }
            }

            return false;
            
        }


        /// <summary>
        /// Returns the location of the powerup.
        /// </summary>
        /// <returns></returns>
        public Vector2D GetLocation()
        {
            return loc;
        }

        /// <summary>
        /// Simulates if a rectangle collides with the powerup object.
        /// </summary>
        /// <param name="head"></param>
        /// <param name="tail"></param>
        /// <returns></returns>
        public bool CollisionRectangle(Vector2D head, Vector2D tail)
        {
            List<Vector2D> pPoints = World.CalculatePoint(head, tail, 5);

            // Create rectangle for the powerup
            Vector2D point1 = new Vector2D(loc);
            point1.X -= 5;
            point1.Y -= 5;
            Vector2D point2 = new Vector2D(loc);
            point2.X += 5;
            point2.Y -= 5;
            Vector2D point3 = new Vector2D(loc);
            point3.X += 5;
            point3.Y += 5;
            Vector2D point4 = new Vector2D(loc);
            point4.X -= 5;
            point4.Y += 5;

            // Create the point list for the object
            List<Vector2D> rectanglePoints = new List<Vector2D>() {point1, point2, point3, point4};

            foreach (Vector2D point in pPoints)
            {
                if (rectanglePoints[0].X <= point.X && point.X <= rectanglePoints[1].X)
                {
                    if (rectanglePoints[0].Y <= point.Y && point.Y <= rectanglePoints[2].Y)
                    {
                        return true;
                    }
                }
            }

            foreach (Vector2D point in rectanglePoints)
            {
                if (pPoints[0].X <= point.X && point.X <= pPoints[1].X)
                {
                    if (pPoints[0].Y <= point.Y && point.Y <= pPoints[2].Y)
                    {
                        return true;
                    }
                }
            }

            if (pPoints[0].X >= rectanglePoints[0].X && pPoints[0].X <= rectanglePoints[1].X && pPoints[0].Y <= rectanglePoints[0].Y)
            {
                if (pPoints[3].Y >= rectanglePoints[0].Y)
                {
                    return true;
                }
            }

            if (pPoints[1].X >= rectanglePoints[0].X && pPoints[1].X <= rectanglePoints[1].X && pPoints[1].Y <= rectanglePoints[0].Y)
            {
                if (pPoints[2].Y >= rectanglePoints[1].Y)
                {
                    return true;
                }
            }

            if (rectanglePoints[0].X >= pPoints[0].X && rectanglePoints[0].X <= pPoints[1].X && rectanglePoints[0].Y <= pPoints[0].Y)
            {
                if (rectanglePoints[3].Y >= pPoints[0].Y)
                {
                    return true;
                }
            }

            if (rectanglePoints[1].X >= pPoints[0].X && rectanglePoints[1].X <= pPoints[1].X && rectanglePoints[1].Y <= pPoints[0].Y)
            {
                if (rectanglePoints[2].Y >= pPoints[1].Y)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
