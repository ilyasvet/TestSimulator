﻿using Simulator.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Simulator.BotControl
{
    internal class TelegramBotControl
    {
        private TelegramBotClient botClient;
        private static Dictionary<string, Command> commandsDictionary;
        private static Dictionary<string, string> accordanceDictionaryTextCommand;
        private static Dictionary<string, string> accordanceDictionaryButtonCommand;

        static TelegramBotControl()
        {
            FillCommandDictionary();
            FillAccordanceDictionaries();
        }
        public TelegramBotControl(string token)
        {
            botClient = new TelegramBotClient(token);
        }
        public void ManagementTelegramBot()
        {
            botClient.StartReceiving(
                Update,
                Error
            );
        }

        private async Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            if (update.Message?.Text != null)
            {
                string messageText = update.Message.Text;
                long userId = update.Message.Chat.Id;
                if(TextIsCommand(messageText))
                {
                    //Тут бот выполняет команду, введённую пользователем
                    await commandsDictionary[accordanceDictionaryTextCommand[messageText]].Execute(userId, botClient);
                }
                else
                {
                    //Тут бот принимает от пользователя какие-то данные
                    await CommandExecuteExtensionText.Execute(userId, botClient, messageText);
                }
            }
            else if (update.Message?.Document != null)
            {
                //Тут бот принимает от пользователя новую группу с эксель файла
                Document messageDocument = update.Message.Document;
                long userId = update.Message.Chat.Id;
                if (messageDocument.FileName != null && (messageDocument.FileName.EndsWith(".xlsx") || messageDocument.FileName.EndsWith(".xls")))
                {
                    await CommandExecuteExtensionFile.Execute(userId, botClient, messageDocument); //TODO схерали Cannot resolve symbol 'CommandExecuteExtensionFile'
                }
            }
            if(update.Type == UpdateType.CallbackQuery)
            {
                //Тут бот выполняет команду по нажатию на кнопку
                CallbackQuery callbackQuery = update.CallbackQuery;
                await commandsDictionary[accordanceDictionaryButtonCommand[callbackQuery.Data]].Execute(
                    callbackQuery.Message.Chat.Id,
                    botClient);
            }
        }
        private async Task Error(ITelegramBotClient botClient, Exception exception, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        private bool TextIsCommand(string text)
        {
            if (text.StartsWith("/"))
            {
                if (accordanceDictionaryTextCommand.ContainsKey(text))
                {
                    return true;
                }
            }
            return false;
        }

        private static void FillCommandDictionary()
        {
            commandsDictionary = new Dictionary<string, Command>();

            Type baseType = typeof(Command);
            IEnumerable<Type> listOfSubclasses = Assembly.GetAssembly(baseType)
                .GetTypes()
                .Where(type => type.IsSubclassOf(baseType));

            foreach (Type type in listOfSubclasses)
            {
                Command commandObject = Activator.CreateInstance(type) as Command;
                commandsDictionary.Add(type.Name, commandObject);
            }
        }
        private static void FillAccordanceDictionaries()
        {
            accordanceDictionaryTextCommand = new Dictionary<string, string>()
            {
                { "/start", "WelcomeCommand" },
            };
            accordanceDictionaryButtonCommand = new Dictionary<string, string>()
            {
                { "Login", "LogInCommand" },
                { "MainMenuUser", "GoToMainMenuUserCommand" },
                { "MainMenuAdmin", "AdminGoToMainMenuCommand" },
                { "UserCard", "UserCardCommand" },
                { "ListUsers", "AdminShowUsersInfoCommand" },
                { "AddGroupAdmin", "AdminAddNewUsers" },
            };
        }
    }
}
