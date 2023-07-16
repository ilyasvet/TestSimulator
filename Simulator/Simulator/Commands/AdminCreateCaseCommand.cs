﻿using Simulator.BotControl;
using Simulator.BotControl.State;
using Simulator.Properties;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Simulator.Commands
{
    public class AdminCreateCaseCommand : Command
    {
        public async override Task Execute(long userId, ITelegramBotClient botClient, string param = "")
        {
            await DataBaseControl.UserTableCommand.SetDialogState(userId, DialogState.CreatingCase);
            await botClient.SendTextMessageAsync(
                chatId: userId,
                text: Resources.CreateCase,
                replyMarkup: CommandKeyboard.ToMainMenuAdmin);

        }
    }
}
