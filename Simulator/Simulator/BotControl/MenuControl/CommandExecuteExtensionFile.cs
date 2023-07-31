using System.Threading.Tasks;
using Telegram.Bot;
using Simulator.BotControl.State;
using System;
using Simulator.Properties;
using Simulator.Services;
using Simulator.Case;
using Telegram.Bot.Types.InputFiles;
using System.IO.Compression;
using System.IO;
using Simulator.Models;

namespace Simulator.BotControl
{
    public static class CommandExecuteExtensionFile
    {
        public static async Task Execute(long userId, ITelegramBotClient botClient, string path, int messageId)
        {
            try
            {
                await Task.Run(async () =>
                {
                    DialogState state = await DataBaseControl.UserTableCommand.GetDialogState(userId);
                    bool resultOperation = false;
                    switch (state)
                    {
                        case DialogState.None:
                            await botClient.DeleteMessageAsync(userId, messageId);
                            break;
                        case DialogState.AddingUsersToGroup:
                            resultOperation = await AddNewUsersTable(userId, botClient, path);
                            break;
                        case DialogState.AddingCase:
                            resultOperation = await AddCase(userId, botClient, path);
                            break;
                        case DialogState.CreatingCase:
                            resultOperation = await CreateCase(userId, botClient, path);
                            break;
                        default:
                            await BotCallBack(userId, botClient, Resources.WrongArgumentMessage);
                            break;
                    }
                    if (resultOperation)
                    {
                        await DataBaseControl.UserTableCommand.SetDialogState(userId, DialogState.None);
                    }
                });
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        private async static Task<bool> CreateCase(long userId, ITelegramBotClient botClient, string path)
        {
            if (!Checker.IsCorrectFileExtension(path, FileType.ExcelTable))
            {
                await BotCallBack(userId, botClient, "File must be excel");
                return false;
            }

            try
            {
                string fileCasePath = await ExcelHandler.CreateCaseAsync(path);
                await BotCallBackWithFile(userId, botClient, fileCasePath);
                return true;
            }
            catch (ArgumentException ex)
            {
                await BotCallBack(userId, botClient, ex.Message);
                return false;
            }
        }

        private async static Task<bool> AddCase(long userId, ITelegramBotClient botClient, string path)
        {
            if (!Checker.IsCorrectFileExtension(path, FileType.Case))
            {
                await BotCallBack(userId, botClient, "Файл должен быть .zip");
                return false;
            }

            StagesControl.DeleteCaseFiles(); // Удаляем старые файлы перед добавлением новых
            ZipFile.ExtractToDirectory(path, ControlSystem.caseDirectory);

            StagesControl.Make();

            // Сообщение об успехе операции

            bool isNew = await DataBaseControl.CourseTableCommand.AddCourse(StagesControl.Stages.CourseName);
            if (isNew || StagesControl.Stages.ReCreateStats)
            {
                if (!isNew)
                {
                    await DataBaseControl.UserStatsControl.DeleteStatsTables(StagesControl.Stages.CourseName);
                }
                await DataBaseControl.UserStatsControl.MakeStatsTables(StagesControl.Stages);
            }
            await BotCallBack(userId, botClient, Resources.AddCaseSuccess);

            return true;
        }

        private async static Task<bool> AddNewUsersTable(long userId, ITelegramBotClient botClient, string path)
        {
            string callBackMessage = "";
            string groupNumber = null;

            if (!Checker.IsCorrectFileExtension(path, FileType.ExcelTable))
            {
                await BotCallBack(userId, botClient, "Файл должен быть exel");
                return false;
            }

            UserType senderType = await DataBaseControl.UserTableCommand.GetUserType(userId);
            try
            {
                if (senderType == UserType.ClassLeader)
                {
                    groupNumber = await DataBaseControl.UserTableCommand.GetGroupNumber(userId);
                    // Номер группы = номер группы старосты
                    // В этом случае группа уже существует, и она корректна
                }
                else if (senderType == UserType.Admin)
                {
                    groupNumber = Path.GetFileNameWithoutExtension(path);
                    // Получаем номер группы из названия файла

                    if (!GroupHandler.IsCorrectGroupNumber(groupNumber))
                    {
                        await BotCallBack(userId, botClient, "Неверный формат номера группы");
                        return false;
                    }

                    if (await GroupHandler.AddGroup(groupNumber)) // Вернёт true, если добавили группу
                    {
                        callBackMessage += $"\nГруппа \"{groupNumber}\" была добавлена";
                    }
                }

                int count = await ExcelHandler.AddUsersFromExcel(path, groupNumber);
                // Добавляем пользователей из файла в группу

                callBackMessage += $"\nДобавлено пользователей в группу \"{groupNumber}\": {count}\n";
                await BotCallBack(userId, botClient, callBackMessage.Insert(0, Resources.SuccessAddGroup));
                return true;
            }
            catch (ArgumentException ex)
            {
                await BotCallBack(userId, botClient, ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + callBackMessage);
            }
        }
        private async static Task BotCallBack(long userId, ITelegramBotClient botClient, string message)
        {
            await botClient.SendTextMessageAsync(
                       chatId: userId,
                       text: message,
                       replyMarkup: CommandKeyboard.ToMainMenu);
        }
        private async static Task BotCallBackWithFile(long userId, ITelegramBotClient botClient, string filePath)
        {
            using (Stream fs = new FileStream(filePath, FileMode.Open))
            {
                await botClient.SendDocumentAsync(
                    chatId: userId,
                    document: new InputOnlineFile(fs, filePath),
                    replyMarkup: CommandKeyboard.ToMainMenu
                    );
                File.Delete(filePath);
            }
        }
    }
}