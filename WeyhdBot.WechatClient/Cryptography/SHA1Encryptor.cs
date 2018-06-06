using System.Security.Cryptography;
using System.Text;

namespace WeyhdBot.WechatClient.Cryptography
{
    public class SHA1Encryptor: ISHA1Encryptor
    {
        public string Encrypt(string s)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(s));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}
