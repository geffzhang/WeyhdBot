using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;

namespace WeyhdBot.Dialogs
{
    public class BasicLuisDialog : ComponentDialog
    {
        public BasicLuisDialog(string dialogId)
            : base(dialogId)
        {
        }
    }
}
