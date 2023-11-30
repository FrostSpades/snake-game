using Model;
using NetworkUtil;
using System.Security.Cryptography;

namespace ServerController;
public class ServerController
{
    World world;
    Dictionary<long, SocketState> clients;

    public ServerController()
    {
        world = new World();
        clients = new();

        StartServer();
    }

    public World GetWorld()
    {
        return world;
    }

    public void StartServer()
    {
        Networking.StartServer(NewClientConnected, 11000);

        Console.WriteLine("Server is running. Accepting clients.");
    }

    public void NewClientConnected(SocketState state)
    {
        if (state.ErrorOccurred)
        {
            return;
        }

        // Finish networking stuff
        //
        //
        //
        //
        //
        //
    }
}
