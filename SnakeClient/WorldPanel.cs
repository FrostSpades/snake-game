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

public class WorldPanel : IDrawable
{
    private IImage wall;
    private IImage background;
    private IImage skeleTail, skeleHead, skeleBody;
    private Model model;

    private bool initializedForDrawing = false;

    private IImage loadImage(string name)
    {
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        string path = "SnakeClient.Resources.Images";
        using (Stream stream = assembly.GetManifestResourceStream($"{path}.{name}"))
        {
#if MACCATALYST
            return PlatformImage.FromStream(stream);
#else
            return new W2DImageLoadingService().FromStream(stream);
#endif
        }
    }

    public WorldPanel()
    {
    }

    public void SetModel(Model model)
    {
        this.model = model;
    }

    private void InitializeDrawing()
    {
        wall = loadImage( "wallsprite.png" );
        background = loadImage( "background.png" );
        //skeleHead = loadImage();
        //skeleTail = loadImage();
        //skeleBody = loadImage();
        initializedForDrawing = true;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if ( !initializedForDrawing )
            InitializeDrawing();

        // undo previous transformations from last frame
        canvas.ResetState();

        int viewSize = 1000;
        Vector2D head;

        lock (model.GetSnakeLock())
        {
            head = model.GetHead();
        }
        

        if (head != null)
        {
            float playerX = (float)head.GetX();
            float playerY = (float)head.GetY();

            canvas.Translate(-playerX + (viewSize / 2), -playerY + (viewSize / 2));
        }

        if (model.GetWorldSize() != 0)
        {
            canvas.DrawImage(background, (-model.GetWorldSize() / 2), (-model.GetWorldSize() / 2), model.GetWorldSize(), model.GetWorldSize());
        }
        

        IEnumerable<Wall> walls = model.GetWalls();
        IEnumerable<Powerup> powerups = model.GetPowerups();
        IEnumerable<Snake> snakes = model.GetSnakes();

        lock (model.GetWallLock())
        {
            foreach (Wall w in walls)
            {
                foreach (Tuple<double, double> segment in w.GetSegments())
                {
                    canvas.DrawImage(wall, (float)segment.Item1, (float)segment.Item2, 50, 50);
                }
            }
        }

        lock (model.GetSnakeLock())
        {
            DrawSnakes(canvas, dirtyRect, snakes);
        }

        lock (model.GetPowerupLock())
        {
            foreach (Powerup p in powerups)
            {
                Vector2D loc = p.GetLocation();
                canvas.StrokeColor = Color.FromArgb("FFFF0000");
                canvas.StrokeSize = 10;
                canvas.StrokeLineCap = LineCap.Round;
                canvas.DrawCircle((int)loc.GetX(), (int)loc.GetY(), 4);
            }
        }
        
    }

    public void DrawSnakes(ICanvas canvas, RectF dirtyRect, IEnumerable<Snake> snakes)
    {
        foreach (Snake s in snakes)
        {
            int remainder = s.snake % 10;

            if (!s.died)
            {
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

                canvas.StrokeSize = 10;
                canvas.StrokeLineCap = LineCap.Round;
                foreach (Tuple<float, float, float, float> segment in s.GetSegments())
                {
                    canvas.DrawLine(segment.Item1, segment.Item2, segment.Item3, segment.Item4);
                }
            }

            else if (!s.alive)
            {
                foreach (Tuple<float, float, float, float> segment in s.GetSegments())
                {
                    
                }
            }


            canvas.StrokeColor = Color.FromArgb("#FFFFFFFF");

            Vector2D sHead = s.GetHead();
            canvas.DrawString(s.GetName() + ": " + s.GetScore().ToString(), (float)sHead.GetX(), (float)sHead.GetY() + 13, HorizontalAlignment.Center);
        }
    }

}
