namespace Model
{
    public class Model
    {
        private int id;
        private int worldSize;
        private List<Wall> walls;
        private List<Snake> snakes;
        private List<Powerup> powerups;

        public Model(string id)
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

        }

        public void AddSnake(string snake)
        {

        }

        public void AddPowerup(string powerup)
        {

        }
    }
}