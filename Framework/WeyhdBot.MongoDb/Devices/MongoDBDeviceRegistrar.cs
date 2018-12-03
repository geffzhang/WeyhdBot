using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeyhdBot.Core.Devices;
using WeyhdBot.Core.Devices.Model;
using WeyhdBot.Core.Devices.Options;

namespace WeyhdBot.MongoDb.Devices
{
    public class MongoDBDeviceRegistrar : IDeviceRegistrar
    {
        private readonly IMongoDBConnector _mongoDBConnector;
        private readonly DeviceRegistrationOptions _options;

        public MongoDBDeviceRegistrar(IMongoDBConnector mongoDBConnector, IOptions<DeviceRegistrationOptions> options)
        {
            _mongoDBConnector = mongoDBConnector;
            _options = options.Value;
        }

        public async Task<DeviceRegistration> GetDeviceRegistrationAsync(string userId, string channelId, string subchannelId = null)
        {
            return await Task.Factory.StartNew(() =>
                _mongoDBConnector.CreateDocumentQuery<DeviceRegistration>(_options.DbName)
                .Where(r => r.Data.UserId == userId && r.Data.ChannelId == channelId && (subchannelId == null || r.Data.Subchannel == subchannelId))
                .FirstOrDefault()?.Data
            );
        }

        public async Task RegisterDeviceAsync(DeviceRegistration registration) 
            => await _mongoDBConnector.CreateDocumentAsync(_options.DbName, null, registration);
    }
}
