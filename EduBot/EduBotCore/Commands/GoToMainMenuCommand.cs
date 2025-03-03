﻿using EduBot.BotControl;
using EduBotCore.Properties;
using EduBotCore.Models.DbModels;
using Telegram.Bot;

namespace EduBot.Commands
{
    public class GoToMainMenuCommand : Command
    {
        public override async Task Execute(long userId, ITelegramBotClient botClient, string param = "")
        {
			UserState userState = await DataBaseControl.GetEntity<UserState>(userId);
			int startDialogMessageId = (await DataBaseControl.GetEntity<UserFlags>(userId)).StartDialogId;
			int endDialogMessageId = 0;
            switch (userState.GetUserType())
            {
                case UserType.Admin:
                    endDialogMessageId = (await botClient.SendTextMessageAsync(
                            chatId: userId,
                            text: Resources.AdminMenu,
                            replyMarkup: CommandKeyboard.AdminMenu)).MessageId;
                    break;
                case UserType.ClassLeader:
                    endDialogMessageId = (await botClient.SendTextMessageAsync(
                            chatId: userId,
                            text: Resources.ClassLeaderMenu,
                            replyMarkup: CommandKeyboard.GroupLeaderMenu)).MessageId;
                    break;
				default:
                    endDialogMessageId = (await botClient.SendTextMessageAsync(
                        chatId: userId,
                        text: Resources.UserMenu,
                        replyMarkup: CommandKeyboard.UserMenu)).MessageId;
                    break;
            }
            endDialogMessageId--;
            if (startDialogMessageId != 0)
            {
                while (startDialogMessageId < endDialogMessageId)
                {
                    try
                    {
                        await botClient.DeleteMessageAsync(userId, startDialogMessageId);
                    }
                    catch { }
                    
                    startDialogMessageId++;
                }
            }
			UserFlags userFlags = await DataBaseControl.GetEntity<UserFlags>(userId);
			userFlags.StartDialogId = 0;
			await DataBaseControl.UpdateEntity(userId, userFlags);
		}
    }
}
