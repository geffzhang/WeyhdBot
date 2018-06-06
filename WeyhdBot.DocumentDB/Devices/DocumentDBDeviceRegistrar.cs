using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
using WeyhdBot.Core.Devices;
using WeyhdBot.Core.Devices.Model;
using WeyhdBot.Core.Devices.Options;
using WeyhdBot.DocumentDB.Extensions;

namespace WeyhdBot.DocumentDB.Devices
{
    public class DocumentDBDeviceRegistrar : IDeviceRegistrar
    {
        private readonly IDocumentDBConnector _docDbConnector;
        private readonly DeviceRegistrationOptions _options;

        public DocumentDBDeviceRegistrar(IOptions<DeviceRegistrationOptions> options, IDocumentDBConnector docDbConnector)
        {
            _docDbConnector = docDbConnector;
            _options = options.Value;
        }

        public async Task<DeviceRegistration> GetDeviceRegistrationAsync(string userId, string channelId, string subchannelId = null)
        {
            var q = _docDbConnector.CreateDocumentQuery<DeviceRegistration>(_options.DbName, _options.CollectionName)
                .Where(r => r.Data.UserId == userId && r.Data.ChannelId == channelId && (subchannelId == null || r.Data.Subchannel == subchannelId));

            var docs = await q.AsDocumentQueryAsync();
            return docs.FirstOrDefault()?.Data;
        }

        public async Task RegisterDeviceAsync(DeviceRegistration registration)
            => await _docDbConnector.CreateDocumentAsync(_options.DbName, _options.CollectionName, null, registration);
    }
}
