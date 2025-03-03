﻿using EduBot.BotControl;
using EduBot.BotControl.State;
using EduBotCore.Properties;
using EduBotCore.Models.DbModels;
using Telegram.Bot;

namespace EduBot.Commands
{
    internal class EditUserInfoCommand : Command
    {
        public override async Task Execute(long userId, ITelegramBotClient botClient, string param = "")
        {
			UserState userState = await DataBaseControl.GetEntity<UserState>(userId);
			userState.SetDialogState(DialogState.EditingUserInfo);
			await DataBaseControl.UpdateEntity(userId, userState);

			int messageId = (await botClient.SendTextMessageAsync(
                chatId: userId,
                text: Resources.EnterUserInfo,
                replyMarkup: CommandKeyboard.BackToUserCard)).MessageId;

			UserFlags userFlags = await DataBaseControl.GetEntity<UserFlags>(userId);
			userFlags.StartDialogId = messageId;
			await DataBaseControl.UpdateEntity(userId, userFlags);
		}
    }
}
