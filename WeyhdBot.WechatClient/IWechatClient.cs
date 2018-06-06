using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WeyhdBot.Core.Wechat;

namespace WeyhdBot.WechatClient
{
    public interface IWechatClient
    {
        /// <summary>
        /// 通过公众号发送微信消息
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        Task PostMessage(WechatMessage msg);

        /// <summary>
        /// 上传媒体信息到微信公众号
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fileName"></param>
        /// <param name="mimeType"></param>
        /// <param name="mediaBytes"></param>
        /// <returns></returns>
        Task<string> UploadMedia(string type, string fileName, string mimeType, byte[] mediaBytes);

        /// <summary>
        /// 上传菜单到微信公众号
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        Task UploadMenu(Menu menu);

        /// <summary>
        /// 上传默认菜单到微信公众号
        /// </summary>
        /// <returns></returns>
        Task UpdateDefaultMenu();
    }
}
