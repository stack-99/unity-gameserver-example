using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shared
{
    public static class ServerSettings
    {
        /// <summary>
        /// Max size for TCP with Reliabe
        /// </summary>
        public const int RELIABLE_BYTE_SIZE = 1024;
        public const string SERVER_IP = "127.0.0.1";
        public const int MAX_USER = 100;
        public const int PORT = 39100;
        public const int WEB_PORT = 39101;
    }
}
