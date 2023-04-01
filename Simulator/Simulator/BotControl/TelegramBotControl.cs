﻿using Simulator.Commands;
using Simulator.Services;
using System;
using System.Collections.Generic;
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
        private static Dictionary<string, string> accordanceDictionaryTextCommand = new Dictionary<string, string>();
        private static Dictionary<string, string> accordanceDictionaryButtonCommand = new Dictionary<string, string>();

        static TelegramBotControl()
        {
            commandsDictionary = Filler.FillCommandDictionary();
            accordanceDictionaryButtonCommand = Filler.FillAccordanceDictionary("AccordanceButtonCommands.txt");
            accordanceDictionaryTextCommand = Filler.FillAccordanceDictionary("AccordanceTextCommands.txt");
        }
        public TelegramBotControl(string token)
        {
            botClient = new TelegramBotClient(token);
        }
        public void ManageTelegramBot()
        {
            botClient.StartReceiving(
                Update,
                Error
            );
        }

        private async Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            long userId = 0;
            try
            {
                if (update.Message != null)
                {
                    int messageId = update.Message.MessageId;
                    try
                    {
                        if (update.Message.Text != null)
                        {
                            string messageText = update.Message.Text;
                            userId = update.Message.Chat.Id;
                            if (Checker.TextIsCommand(accordanceDictionaryTextCommand, messageText))
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
                        else if (update.Message.Document != null)
                        {
                            //Тут бот принимает от пользователя файл
                            userId = update.Message.Chat.Id;
                            Document messageDocument = update.Message.Document;

                            string path = await BotHandler.FileHandle(botClient, messageDocument);

                            await CommandExecuteExtensionFile.Execute(userId, botClient, path);
                        }
                    }
                    finally
                    {
                        try
                        {
                            await botClient.DeleteMessageAsync(userId, messageId - 1);
                            await botClient.DeleteMessageAsync(userId, messageId);
                        }
                        catch { }
                    }
                }
                else if (update.Type == UpdateType.CallbackQuery)
                {
                    CallbackQuery callbackQuery = update.CallbackQuery;
                    try
                    {
                        //Тут бот выполняет команду по нажатию на кнопку
                        userId = callbackQuery.Message.Chat.Id;
                        string data = callbackQuery.Data;
                        string commandWord = callbackQuery.Data.Split('|')[0];
                        string param = Checker.GetCommandCallbackQueryParam(data);
                        await commandsDictionary[accordanceDictionaryButtonCommand[commandWord]].Execute(
                            userId,
                            botClient,
                            param);
                    }
                    finally
                    {
                        try
                        {
                            await botClient.DeleteMessageAsync(userId, callbackQuery.Message.MessageId);
                        }
                        catch { }
                    }
                }
            }
            catch(Exception ex)
            {
                //Все исключения обработаны так, что возвращают в сообщении также userId
                await commandsDictionary[accordanceDictionaryTextCommand["/skip"]].Execute(
                            userId,
                            botClient,
                            ex.Message + "\nПереход в меню...");
            }
        }
        private async Task Error(ITelegramBotClient botClient, Exception exception, CancellationToken token)
        {

        }
    }
}
