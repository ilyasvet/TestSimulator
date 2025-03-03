using Telegram.Bot;
using Telegram.Bot.Types;
using EduBot.BotControl.State;
using EduBotCore.Properties;
using EduBot.Services;
using System.IO.Compression;
using EduBotCore.Models.DbModels;

using DbUser = EduBotCore.Models.DbModels.User;
using EduBotCore.Case;

namespace EduBot.BotControl
{
    public static class CommandExecuteExtensionFile
    {
        public static async Task Execute(long userId, ITelegramBotClient botClient, string path, int messageId)
        {
            try
            {
                await Task.Run(async () =>
                {
                    UserState? userState = await DataBaseControl.GetEntity<UserState>(userId);

					DialogState state = userState?.GetDialogState() ?? DialogState.None;
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
                        userState.SetDialogState(DialogState.None);
                        await DataBaseControl.UpdateEntity(userId, userState);
                    }
                });
            }
            finally
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
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

            string courseName = Path.GetFileNameWithoutExtension(path);
            string coursePath = ControlSystem.caseDirectory + "/" + courseName;
            bool isNew = !CoursesControl.Courses.Contains(courseName);
            ControlSystem.CreateDirectory(coursePath);

			// Удаляем старые файлы перед добавлением новых
			ControlSystem.DeleteFilesFromDirectory(coursePath);
            
            ZipFile.ExtractToDirectory(path, coursePath);

            await CoursesControl.ReMake(coursePath);



            // Сообщение об успехе операции
            try
            {
                if (isNew || CoursesControl.Courses[courseName].ReCreateStats)
                {
                    if (!isNew)
                    {
                        try
                        {
                            await DataBaseControl.UserStatsControl.DeleteStatsTables(courseName);
                        }
                        catch { }
                    }
                    else
                    {
                        await DataBaseControl.AddEntity(new Course { CourseName = courseName });
                    }
                    await DataBaseControl.UserStatsControl.MakeStatsTables(CoursesControl.Courses[courseName]);
                }
            } catch
            {
				ControlSystem.DeleteFilesFromDirectory(coursePath);
                throw;
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

            UserState sender = await DataBaseControl.GetEntity<UserState>(userId);
            try
            {
                if (sender.GetUserType() == UserType.ClassLeader)
                {
                    groupNumber = (await DataBaseControl.GetEntity<DbUser>(userId)).GroupNumber;
                    // Номер группы = номер группы старосты
                    // В этом случае группа уже существует, и она корректна
                }
                else if (sender.GetUserType() == UserType.Admin)
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
                    document: new InputFileStream(fs, filePath),
                    replyMarkup: CommandKeyboard.ToMainMenu
                    );
            }
            System.IO.File.Delete(filePath);
        }
    }
}