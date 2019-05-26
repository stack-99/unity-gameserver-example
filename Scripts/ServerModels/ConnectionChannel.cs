using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Assets.Scripts.Models
{
    public class ConnectionChannel
    {
        public byte ChannelId { get; set; }
        public QosType ChannelType { get; set; }

        public string Description { get; set; }

        public ConnectionChannel(byte channId, QosType chanType, string description = "")
        {
            ChannelId = channId;
            ChannelType = chanType;
            Description = description;
        }
    }
}
