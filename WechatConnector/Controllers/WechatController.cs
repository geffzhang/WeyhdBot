using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Senparc.CO2NET.HttpUtility;
using Senparc.Weixin;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP.MvcExtension;
using WechatConnector.CustomMessageHandlers;
using WeyhdBot.Core.Devices;
using WeyhdBot.Core.Devices.Model;
using WeyhdBot.Core.Wechat;
using WeyhdBot.Wechat.Client;
using WeyhdBot.Wechat.Connector;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WechatConnector.Controllers
{
    [Route("Wechat")]
    public class WechatController : ControllerBase
    {
        private static readonly string SUCCESS = "success";
        private static readonly string WECHAT_SUBCHANNEL = "wechat";
        private static readonly string DIRECTLINE_CHANNEL = "directline";

        public static readonly string Token = Config.SenparcWeixinSetting.Token ?? CheckSignature.Token;//与微信公众账号后台的Token设置保持一致，区分大小写。
        public static readonly string EncodingAESKey = Config.SenparcWeixinSetting.EncodingAESKey;//与微信公众账号后台的EncodingAESKey设置保持一致，区分大小写。
        public static readonly string AppId = Config.SenparcWeixinSetting.WeixinAppId;//与微信公众账号后台的AppId设置保持一致，区分大小写。


        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IWechatClient _connector;
        private readonly IDirectLineConnector _directLineConnector;
        private readonly IDeviceRegistrar _deviceRegistrar;
        private readonly ILogger _logger;

        public WechatController(IHostingEnvironment hostingEnvironment, IWechatClient connector, IDirectLineConnector directLineConnector, IDeviceRegistrar deviceRegistrar, ILoggerFactory loggerFactory)
        {
            _hostingEnvironment = hostingEnvironment;
            _connector = connector;
            _directLineConnector = directLineConnector;
            _deviceRegistrar = deviceRegistrar;
            _logger = loggerFactory.CreateLogger<WechatController>();
        }

        [HttpGet]
        [Route("")]
        public Task<ActionResult> Get(string signature, string timestamp, string nonce, string echostr)
        {
            return Task.Factory.StartNew(() =>
            {
                if (CheckSignature.Check(signature, timestamp, nonce, Token))
                {
                    return echostr; //返回随机字符串则表示验证通过
                }
                else
                {
                    return "failed:" + signature + "," + Senparc.Weixin.MP.CheckSignature.GetSignature(timestamp, nonce, Token) + "。" +
                        "如果你在浏览器中看到这句话，说明此地址可以被作为微信公众账号后台的Url，请注意保持Token一致。";
                }
            }).ContinueWith<ActionResult>(task => Content(task.Result));
        }

        /// <summary>
        /// 最简化的处理流程
        /// </summary>
        [HttpPost]
        [Route("")]
        public async Task<ActionResult> Post(PostModel postModel)
        {
            if (!CheckSignature.Check(postModel.Signature, postModel.Timestamp, postModel.Nonce, Token))
            {
                return new WeixinResult("参数错误！");
            }

            postModel.Token = Token;
            postModel.EncodingAESKey = EncodingAESKey; //根据自己后台的设置保持一致
            postModel.AppId = AppId; //根据自己后台的设置保持一致

            var messageHandler = new CustomMessageHandler(Request.GetRequestMemoryStream(), postModel, 10);

            messageHandler.DefaultMessageHandlerAsyncEvent = Senparc.NeuChar.MessageHandlers.DefaultMessageHandlerAsyncEvent.SelfSynicMethod;//没有重写的异步方法将默认尝试调用同步方法中的代码（为了偷懒）

            #region 设置消息去重

            /* 如果需要添加消息去重功能，只需打开OmitRepeatedMessage功能，SDK会自动处理。
             * 收到重复消息通常是因为微信服务器没有及时收到响应，会持续发送2-5条不等的相同内容的RequestMessage*/
            messageHandler.OmitRepeatedMessage = true;//默认已经开启，此处仅作为演示，也可以设置为false在本次请求中停用此功能

            #endregion

            //messageHandler.SaveRequestMessageLog();//记录 Request 日志（可选）
            messageHandler.DeviceRegistrar = _deviceRegistrar;
            messageHandler.DirectLineConnector = _directLineConnector;

            await messageHandler.ExecuteAsync(); //执行微信处理过程（关键）

            //messageHandler.SaveResponseMessageLog();//记录 Response 日志（可选）
            

            return new FixWeixinBugWeixinResult(messageHandler);
        }


        [Route("outgoingmessage")]
        public async Task<IActionResult> PostOutgoingMessage()
        {
            var json = await this.ReadRequestContentAsync();
            if (string.IsNullOrWhiteSpace(json))
            {
                _logger.LogWarning("/wechat/outgoingmessage invoked without a message in post body");
                return BadRequest("No message sent to post!");
            }
            var message = JsonConvert.DeserializeObject<WechatMessage>(json);
            try
            {
                await _connector.PostTextMessage(AppId, message.ToUserName, message.Content);
                return Ok(SUCCESS);
            }
            catch (Exception ex)
            {
                var errorMessage = "Could not post message to WeChat customer service";
                _logger.LogError(ex, errorMessage);
                return StatusCode(500, errorMessage);
            }
        }

    }
}
