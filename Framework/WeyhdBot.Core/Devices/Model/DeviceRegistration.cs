using System;

namespace WeyhdBot.Core.Devices.Model
{
    [Serializable]
    public class DeviceRegistration
    {
        public string ChannelId { get; set; }
        public string Subchannel { get; set; }
        public string UserId { get; set; }
        public string ConversationId { get; set; }
    }
}
