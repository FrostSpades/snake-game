using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SnakeGame;

namespace Model
{
    internal class Snake
    {
        public int snake;
        public string name;
        public List<Vector2D> body;
        public Vector2D dir;
        public int score;
        public bool died;
        public bool alive;
        public bool dc;
        public bool join;

        public Snake() 
        {
        } 
    }
}
