using WeyhdBot.Core.Devices.Model;
using System.Threading.Tasks;

namespace WeyhdBot.Core.Devices
{
    public interface IDeviceRegistrar
    {
        Task<DeviceRegistration> GetDeviceRegistrationAsync(string userId, string channelId, string subchannelId = null);

        Task RegisterDeviceAsync(DeviceRegistration registration);
    }
}
