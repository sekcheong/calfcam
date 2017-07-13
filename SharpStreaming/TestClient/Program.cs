using Simon.SharpStreaming.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            RtspClient rtspClient = new RtspClient();
            bool result = rtspClient.ConnectServer("144.92.136.102", 554);
            rtspClient.OpenStream("rtsp://admin:1675WisM%40d@144.92.136.102", "c:\\temp\\");
        }
    }
}
