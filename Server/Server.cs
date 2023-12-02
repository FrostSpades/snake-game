namespace Server;

using System.Diagnostics;
using Model;
using ServerController;
using NetworkUtil;

public class Server
{

    ServerController serverController;
    World world;
    public static void Main(string[] args)
    {
        Server server = new();
        
    }

    public Server()
    {
        serverController = new();
        world = serverController.GetWorld();
        serverController.StartMainLoop();
    }
}