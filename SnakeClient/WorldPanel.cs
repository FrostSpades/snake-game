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

public class WorldPanel : IDrawable
{
    private IImage wall;
    private IImage background;
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

        foreach(Wall w in walls) 
        { 
            foreach(Tuple<double, double> segment in w.GetSegments())
            {
                canvas.DrawImage(wall, (float)segment.Item1, (float)segment.Item2, 50, 50);
            }
        }

        lock (model.GetSnakeLock())
        {
            foreach (Snake s in snakes)
            {
                foreach (Tuple<float, float, float, float> segment in s.GetSegments())
                {

                    canvas.StrokeColor = Color.FromArgb("#FF00FF00");
                    canvas.StrokeSize = 10;
                    canvas.StrokeLineCap = LineCap.Round;
                    canvas.DrawLine(segment.Item1, segment.Item2, segment.Item3, segment.Item4);
                }
            }
        }

        lock (powerups)
        {
            foreach (Powerup p in powerups)
            {
                Vector2D loc = p.GetLocation();
                canvas.StrokeColor = Color.FromArgb("FFFF0000");
                canvas.DrawCircle((int)loc.GetX(), (int)loc.GetY(), 4);
            }
        }
        
    }

}
