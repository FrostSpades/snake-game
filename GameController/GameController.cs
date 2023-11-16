using System.Data.SqlTypes;
using System.Diagnostics;
using System.Text.RegularExpressions;
using NetworkUtil;

namespace GameController
{
    public class GameController
    {
        private string? name;
        private SocketState? server;
        public GameController() 
        { 
            
        }
        public void Move(string direction)
        {
            string message = direction + "\n";
            Networking.Send(server.TheSocket, message);
        }
        public void Connect(string server, string name)
        {
            this.name = name;
            Networking.ConnectToServer(OnConnect, server, 11000);
            
        }
        private void OnConnect(SocketState state)
        {
            server = state;
            state.OnNetworkAction = ReceiveMessage;
            Networking.GetData(state);
            Networking.Send(server.TheSocket, name);
        }
        
        private void ReceiveMessage(SocketState state) 
        { 
            ProcessMessages(state);
            Networking.GetData(state);

        }

        private void ProcessMessages(SocketState state)
        {
            string totalData = state.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");
            foreach(string p in parts)
            {
                if (p.Length==0)
                    continue;
                if (p[p.Length - 1] != '\n')
                    break;

                Debug.WriteLine(p);
            }
        }
    }

}