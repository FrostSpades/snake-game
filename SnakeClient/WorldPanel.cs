// Authors: Ethan Andrews and Mary Garfield
// Drawing panel for snake game.
// University of Utah
namespace SnakeGame;

using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using IImage = Microsoft.Maui.Graphics.IImage;
#if MACCATALYST
using Microsoft.Maui.Graphics.Platform;
#else
using Microsoft.Maui.Graphics.Win2D;
#endif
using Color = Microsoft.Maui.Graphics.Color;
using System.Reflection;
using Microsoft.Maui;
using System.Net;
using Font = Microsoft.Maui.Graphics.Font;
using SizeF = Microsoft.Maui.Graphics.SizeF;
using Model;
using Newtonsoft.Json.Serialization;
using Microsoft.Maui.Graphics;

/// <summary>
/// Drawing panel for the maui application.
/// </summary>
public class WorldPanel : IDrawable
{
    private IImage wall;
    private IImage background;

    // Snake skeleton images
    private IImage skeleTail, skeleHeadUp, skeleHeadDown, skeleHeadRight, skeleHeadLeft, skeleBodyUp, skeleBodyDown;
    
    private Model model;
    private bool initializedForDrawing = false;

    /// <summary>
    /// Loads the given image from SnakeClient\Resources\Images given an image name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private IImage loadImage(string name)
    {
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        string path = "SnakeClient.Resources.Images." + name;
        using (Stream stream = assembly.GetManifestResourceStream(path))
        {
#if MACCATALYST
            return PlatformImage.FromStream(stream);
#else
            return new W2DImageLoadingService().FromStream(stream);
#endif
        }
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public WorldPanel()
    {
    }

    /// <summary>
    /// Sets the model
    /// </summary>
    /// <param name="model"></param>
    public void SetModel(Model model)
    {
        this.model = model;
    }

    /// <summary>
    /// Initializes the images.
    /// </summary>
    private void InitializeDrawing()
    {
        wall = loadImage( "wallsprite.png" );
        background = loadImage( "background.png" );
        skeleHeadUp = loadImage("skeleheadup.png");
        skeleHeadDown = loadImage("skeleheaddown.png");
        skeleHeadLeft = loadImage("skeleheadleft.png");
        skeleHeadRight = loadImage("skeleheadright.png");
        skeleTail = loadImage("skeletail.png");
        skeleBodyUp = loadImage("skelebodyup.png");
        skeleBodyDown = loadImage("skelebodydown.png");
        initializedForDrawing = true;
    }

    /// <summary>
    /// Redraws the panel.
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="dirtyRect"></param>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if ( !initializedForDrawing )
            InitializeDrawing();

        // undo previous transformations from last frame
        canvas.ResetState();

        // Set the view size
        int viewSize = model.GetWorldSize() / 2;
        
        lock (model)
        {
            // Get the snake head location
            Vector2D head;
            head = model.GetHead();

            // Set the canvas to move with the head
            if (head != null)
            {
                float playerX = (float)head.GetX();
                float playerY = (float)head.GetY();

                canvas.Translate(-playerX + (viewSize / 2), -playerY + (viewSize / 2));
            }

            // Draws the background
            if (model.GetWorldSize() != 0)
            {
                canvas.DrawImage(background, (-model.GetWorldSize() / 2), (-model.GetWorldSize() / 2), model.GetWorldSize(), model.GetWorldSize());
            }

            // Gets the game objects
            IEnumerable<Wall> walls = model.GetWalls();
            IEnumerable<Powerup> powerups = model.GetPowerups();
            IEnumerable<Snake> snakes = model.GetSnakes();

            // Draws the walls
            foreach (Wall w in walls)
            {
                foreach (Tuple<double, double> segment in w.GetSegments())
                {
                    canvas.DrawImage(wall, (float)segment.Item1, (float)segment.Item2, 50, 50);
                }
            }

            // Draws the snakes
            DrawSnakes(canvas, dirtyRect, snakes);

            // Draws the powerups
            foreach (Powerup p in powerups)
            {
                Vector2D loc = p.GetLocation();
                canvas.StrokeColor = Color.FromArgb("FFFF0000");
                canvas.StrokeSize = 6;
                canvas.StrokeLineCap = LineCap.Round;
                canvas.DrawCircle((int)loc.GetX(), (int)loc.GetY(), 4);
            }
        }
    }

    /// <summary>
    /// Helper method for drawing snakes
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="dirtyRect"></param>
    /// <param name="snakes"></param>
    private void DrawSnakes(ICanvas canvas, RectF dirtyRect, IEnumerable<Snake> snakes)
    {
        foreach (Snake s in snakes)
        {
            // Determines which color the snake is drawn as
            int remainder = s.snake % 10;

            if (s.alive)
            {
                // Given the remainder, choose a color
                switch (remainder)
                {
                    case 0:
                        canvas.StrokeColor = Color.FromArgb("#FF00FF00");
                        break;
                    case 1:
                        canvas.StrokeColor = Color.FromArgb("#FFFF0000");
                        break;
                    case 2:
                        canvas.StrokeColor = Color.FromArgb("#FFFFA500");
                        break;
                    case 3:
                        canvas.StrokeColor = Color.FromArgb("#FF0000FF");
                        break;
                    case 4:
                        canvas.StrokeColor = Color.FromArgb("#FF4B0082");
                        break;
                    case 5:
                        canvas.StrokeColor = Color.FromArgb("#FFEE82EE");
                        break;
                    case 6:
                        canvas.StrokeColor = Color.FromArgb("#FF00FFFF");
                        break;
                    case 7:
                        canvas.StrokeColor = Color.FromArgb("#FFFF00FF");
                        break;
                    case 8:
                        canvas.StrokeColor = Color.FromArgb("#FFFFFF00");
                        break;
                    case 9:
                        canvas.StrokeColor = Color.FromArgb("#FFFFC0CB");
                        break;
                }

                // Draw the snake
                canvas.StrokeSize = 10;
                canvas.StrokeLineCap = LineCap.Round;
                foreach (Tuple<float, float, float, float> segment in s.GetSegments())
                {
                    canvas.DrawLine(segment.Item1, segment.Item2, segment.Item3, segment.Item4);
                }

                // Draw the player name and score
                canvas.StrokeColor = Color.FromArgb("#FFFFFFFF");

                Vector2D sHead = s.GetHead();
                canvas.DrawString(s.GetName() + ": " + s.GetScore().ToString(), (float)sHead.GetX(), (float)sHead.GetY() + 13, HorizontalAlignment.Center);
            }

            // If snake is dead, draw a skeleton
            else
            {
                // Draw spine for each segment
                foreach (Tuple<float, float, float, float> segment in s.GetSegments())
                {
                    if (segment.Item1 == segment.Item3)
                    {
                        // If segment is facing to the up
                        if (segment.Item4 > segment.Item2)
                        {
                            canvas.DrawImage(skeleBodyUp, segment.Item1, segment.Item2, 11, segment.Item4 - segment.Item2 + 7);
                        }
                        // If segment is facing down
                        else
                        {
                            canvas.DrawImage(skeleBodyUp, segment.Item3, segment.Item4, 11, segment.Item2 - segment.Item4 + 3);
                        }
                    }
                    else
                    {
                        // If segment is facing right
                        if (segment.Item3 > segment.Item1)
                        {
                            canvas.DrawImage(skeleBodyDown, segment.Item1, segment.Item2, segment.Item3 - segment.Item1 + 7, 11);
                        }
                        // If segment is facing left
                        else
                        {
                            canvas.DrawImage(skeleBodyDown, segment.Item3, segment.Item4, segment.Item1 - segment.Item3 + 3, 11);
                        }
                    }
                }

                // Draws the skeleton head of the snake
                Vector2D head = s.GetHead();
                string dir = s.GetDir();

                // Change the rotation of the image based on the direction
                if (dir == "up")
                {
                    canvas.DrawImage(skeleHeadUp, (float)head.GetX() - 2, (float)head.GetY(), 15, 15);
                }
                else if (dir == "down")
                {
                    canvas.DrawImage(skeleHeadDown, (float)head.GetX() - 2, (float)head.GetY(), 15, 15);
                }
                else if (dir == "left")
                {
                    canvas.DrawImage(skeleHeadLeft, (float)head.GetX(), (float)head.GetY() - 2, 15, 15);
                }
                else if (dir == "right")
                {
                    canvas.DrawImage(skeleHeadRight, (float)head.GetX(), (float)head.GetY() - 2, 15, 15);
                }
                
            }
        }
    }

}
