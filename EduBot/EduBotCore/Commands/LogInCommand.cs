using EduBot.BotControl.State;
using EduBotCore.Properties;
using Telegram.Bot;
using EduBot.BotControl;
using EduBotCore.Models.DbModels;

namespace EduBot.Commands
{
    public class LogInCommand : Command
    {
        public override async Task Execute(long userId, ITelegramBotClient botClient, string param = "")
        {
			UserState userState = await DataBaseControl.GetEntity<UserState>(userId);
			userState.SetDialogState(DialogState.EnterPassword);
			await DataBaseControl.UpdateEntity(userId, userState);

			int messageId = (await botClient.SendTextMessageAsync(userId, Resources.EnterPassword)).MessageId;

			UserFlags userFlags = await DataBaseControl.GetEntity<UserFlags>(userId);
			userFlags.StartDialogId = messageId;
			await DataBaseControl.UpdateEntity(userId, userFlags);
		}
    }
}
