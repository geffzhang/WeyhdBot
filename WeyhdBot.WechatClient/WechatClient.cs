using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WeyhdBot.Core.Wechat;

namespace WeyhdBot.WechatClient
{
    public class WechatClient : IWechatClient
    {
        private readonly WechatOptions _options;
        private WechatAccessToken _accessToken;

        private ILogger<WechatClient> _logger;

        public WechatClient(IOptions<WechatOptions> options, ILogger<WechatClient> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task PostMessage(WechatMessage msg)
        {
            await ValidateMessage(msg);

            var token = await GetOrRefreshToken();
            using (var client = new HttpClient())
            {
                var uriBuilder = new UriBuilder(_options.CustomerEndpoint);
                uriBuilder.Query = $"access_token={token.AccessToken}";

                var json = JsonConvert.SerializeObject(msg);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(uriBuilder.Uri, content);
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task<string> UploadMedia(string type, string fileName, string mimeType, byte[] mediaBytes)
        {
            try
            {
                var boundaryStr = Guid.NewGuid().ToString();
                var token = await GetOrRefreshToken();

                //Getting this to work before using a singleton httpclient or HttpClientFactory in 2.1
                var handler = new HttpClientHandler();
                handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

                using (var client = new HttpClient(handler))
                using (var ms = new MemoryStream(mediaBytes))
                using (var request = new HttpRequestMessage())
                using (var form = new MultipartFormDataContent(boundaryStr))
                {
                    var uriBuilder = new UriBuilder(_options.MediaUploadEndpoint);
                    uriBuilder.Query = $"access_token={token.AccessToken}&type={type}";


                    //Quotes were added to better match how Tencent will parse the request
                    var mediaContent = new StreamContent(ms);
                    mediaContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
                    mediaContent.Headers.ContentDisposition.Name = "\"media\"";
                    mediaContent.Headers.ContentDisposition.FileName = $"\"{fileName}\"";
                    mediaContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

                    //Replace Content-Type header with one that doesn't wrap the boundary in quotes - because Tencent won't parse it properly
                    form.Headers.Remove("Content-Type");
                    form.Headers.TryAddWithoutValidation("Content-Type", $"multipart/form-data; boundary={boundaryStr}");
                    form.Add(mediaContent);

                    request.Method = HttpMethod.Post;
                    request.RequestUri = uriBuilder.Uri;
                    request.Content = form;

                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();
                    var mediaResponse = JsonConvert.DeserializeObject<WechatMediaUploadResponse>(json);

                    if (!string.IsNullOrWhiteSpace(mediaResponse.ErrorMessage))
                    {
                        _logger.LogError("Could not upload media {0}: | Error Code: {1} | Message: {2}", fileName, mediaResponse.ErrorCode, mediaResponse.ErrorMessage);
                        return string.Empty;
                    }

                    return mediaResponse.MediaId ?? string.Empty;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not upload media {0}", fileName);
                return string.Empty;
            }
        }

        public async Task UpdateDefaultMenu()
        {
            if (_options.UpdateMenuOnRun && !string.IsNullOrWhiteSpace(_options.DefaultMenu))
            {
                if (!File.Exists(_options.DefaultMenu))
                {
                    _logger.LogError("Tried to update menu but {0} does not exist", _options.DefaultMenu);
                    return;
                }

                var json = File.ReadAllText(_options.DefaultMenu);
                var menu = JsonConvert.DeserializeObject<Menu>(json);
                await UploadMenu(menu);
            }
        }

        public async Task UploadMenu(Menu menu)
        {
            try
            {
                var token = await GetOrRefreshToken();
                var handler = new HttpClientHandler();
                handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

                using (var client = new HttpClient(handler))
                {
                    var uriBuilder = new UriBuilder(_options.MenuUploadEndpoint);
                    uriBuilder.Query = $"access_token={token.AccessToken}";

                    var content = new StringContent(JsonConvert.SerializeObject(menu), Encoding.UTF8);
                    var response = await client.PostAsync(uriBuilder.Uri, content);
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();
                    var responseMessage = JsonConvert.DeserializeObject<WechatGenericResponse>(json);

                    if (responseMessage.ErrorCode != 0)
                    {
                        throw new ArgumentException($"Received Failure Code from WeChat: {responseMessage.ErrorCode} - {responseMessage.ErrorMessage}");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not upload menu");
            }
        }

        private async Task<WechatAccessToken> GetOrRefreshToken()
        {
            if (_accessToken != null)
            {
                var age = DateTime.UtcNow - _accessToken.IssueTime;
                if (age.TotalSeconds < _accessToken.ExpiresIn)
                    return _accessToken;
            }

            using (var client = new HttpClient())
            {
                var uriBuilder = new UriBuilder(_options.TokenURI);
                uriBuilder.Query = $"grant_type=client_credential&appid={_options.AppId}&secret={_options.AppSecret}";

                var response = await client.GetAsync(uriBuilder.Uri);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                _accessToken = JsonConvert.DeserializeObject<WechatAccessToken>(json);
                _accessToken.IssueTime = DateTime.UtcNow;

                return _accessToken;
            }
        }

        private async Task ValidateMessage(WechatMessage message)
        {
            if (message.MessageType == WechatMessageTypes.IMAGE)
            {
                //If we're holding a URL instead of a WeChat media ID
                //then we need to download the image and transfer to wechat
                if (message.MediaId.Contains("http"))
                {
                    message.MediaId = await GetWebImageIdAsync(message.MediaId);
                }
            }
        }

        private async Task<string> GetWebImageIdAsync(string url)
        {
            var fileName = Path.GetFileName(url);
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);

                var mimeType = response.Content.Headers.ContentType.MediaType;
                var imageBytes = await response.Content.ReadAsByteArrayAsync();

                return await UploadMedia(WechatMessageTypes.IMAGE, fileName, mimeType, imageBytes);
            }
        }
    }
}