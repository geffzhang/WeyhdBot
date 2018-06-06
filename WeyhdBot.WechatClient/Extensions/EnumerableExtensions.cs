using System;
using System.Collections.Generic;
using System.Linq;

namespace WeyhdBot.WechatClient.Extensions
{
    public static class EnumerableExtensions
    {
        public static T RandomElementOrDefault<T>(this IEnumerable<T> items)
        {
            if (items == null || items.Count() == 0)
                return default(T);

            var r = new Random();
            return items.ElementAt(r.Next(items.Count()));
        }
    }
}
