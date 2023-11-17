using System.Collections.Immutable;
using System.Text.Json;

namespace Model
{
    public class Model
    {
        private int snakeID;
        private int worldSize;
        private List<Wall> walls;
        private List<Snake> snakes;
        private List<Powerup> powerups;

        public Model()
        {
            walls = new();
            snakes = new();
            powerups = new();
            worldSize = 0;
        }

        public void SetWorldSize(string size)
        {
            
        }

        public void AddWall(string wall)
        {
            Wall? rebuilt = JsonSerializer.Deserialize<Wall>(wall);

            if (rebuilt != null)
            {
                walls.Add(rebuilt);
            }
        }

        public void AddSnake(string snake)
        {

        }

        public void AddPowerup(string powerup)
        {

        }

        public void SetID(string id)
        {
            //snakeID = id;
        }

        public List<Snake> GetSnakes()
        {
            return snakes;
        }

        public List<Wall> GetWalls()
        {
            return walls;
        }

        public List<Powerup> GetPowerups()
        {
            return powerups;
        }
    }
}