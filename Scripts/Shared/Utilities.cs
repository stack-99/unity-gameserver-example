using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Assets.Scripts.Shared
{
    public static class Utilities
    {
        public static BufferInfo SerializeData(object obj, int buffer_size = ServerSettings.RELIABLE_BYTE_SIZE)
        {
            byte[] buffer = new byte[buffer_size];

            Stream message = new MemoryStream(buffer);
            message.Position = 0;
            BinaryFormatter formatter = new BinaryFormatter();
            //Serialize the message
            formatter.Serialize(message, obj);

            return new BufferInfo() { Buffer = buffer, UsedLength = (int)message.Position };
        }

        public static T DeserializeData<T>(byte[] buffer)
        {
            Stream message = new MemoryStream(buffer);
            message.Position = 0;
            BinaryFormatter formatter = new BinaryFormatter();
            //Serialize the message
            object text = formatter.Deserialize(message);

            return (T)text;
        }
    }
}
