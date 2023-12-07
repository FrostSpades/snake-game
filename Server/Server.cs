// Entrypoint to server application.
// Authors: Ethan Andrews and Mary Garfield
// Date: 12/6/2023
namespace Server;

using System.Diagnostics;
using Model;
using ServerController;
using NetworkUtil;

/// <summary>
/// Class for starting the server application
/// </summary>
public class Server
{

    ServerController serverController;
    
    /// <summary>
    /// Entrypoint to application.
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        Server server = new();
        
    }

    public Server()
    {
        serverController = new();
        // Start the main loop
        serverController.StartMainLoop();
    }
}