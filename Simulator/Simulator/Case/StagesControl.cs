﻿using Simulator.BotControl;
using Simulator.Models;
using Simulator.Services;
using Simulator.TelegramBotLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Simulator.Case
{
    internal static class StagesControl
    {
        private readonly static string pathToCase = 
                $"{AppDomain.CurrentDomain.BaseDirectory}" +
                $"{ConfigurationManager.AppSettings["PathCase"]}";
        public static StageList Stages { get; set; } = new StageList();
        public static bool Make()
        {
            string caseInfoFileName = ConfigurationManager.AppSettings["CaseInfoFileName"];
            string path = pathToCase + "\\" + caseInfoFileName;
            if (System.IO.File.Exists(path))
            {
                try
                {
                    CaseConverter.FromFile(path);
                }
                catch
                {
                    DeleteCaseFiles();
                    return false;
                }
                return true;
            }
            return false;
        }

        public static void DeleteCaseFiles()
        {
            var files = Directory.GetFiles(pathToCase);
            foreach (var file in files)
            {
                System.IO.File.Delete(file);
            }
        }

        public static double CalculateRatePoll(CaseStagePoll stage, int[] answers)
        {
            double rate = 0;
            
            int count = answers.Length;
            if(stage.Limit < count && stage.Limit != 0)
            {
                //Штраф за превышение кол-ва ответов
                rate -= stage.Fine * (count - stage.Limit);
                count = stage.Limit;
            }
            for (int i = 0; i < count;i++)
            {
                rate += stage.PossibleRate[answers[i]];
            }
            if(stage.WatchNonAnswer)
            {
                foreach (var opt in stage.NonAnswers)
                {
                    if(!answers.Contains(opt.Key))
                    {
                        rate -= opt.Value;
                    }
                }
            }
            return rate;
        }
        public static void SetStageForMove(CaseStagePoll stage, int[] answers)
        {
            if(stage.ConditionalMove)
            {
                stage.NextStage = stage.MovingNumbers[answers[0]];
            } 
            //Если переход безусловный, то NextStage уже установлено
            //Если нет, то на основе свойств и ответа выбираем NextStage
        }
        public static CaseStage GetNextStage(CaseStage current, CallbackQuery query)
        {
            if (query.Data == "MoveNext")
            {
                return Stages[current.NextStage];
            }
            else if(query.Data == "ToBegin")
            {
                UserCaseTableCommand.SetRate(query.From.Id, 0);
                return Stages.StagesNone.Min();
            }
            else if(query.Data == "ToOut")
            {
                return null;
            }
            throw new ArgumentException();
        }
        public static async Task Move(long userId, CaseStage nextStage, ITelegramBotClient botClient)
        {
            int hp = UserCaseTableCommand.GetHealthPoints(userId);

            switch (nextStage)
            {
                case CaseStagePoll poll:
                    UserCaseTableCommand.SetStartTime(userId, DateTime.Now);
                    await ShowAdditionalInfo(botClient, poll, userId);
                    await botClient.SendPollAsync(
                        chatId: userId,
                        question: poll.TextBefore,
                        options: poll.Options,
                        isAnonymous: false,
                        allowsMultipleAnswers: poll.ManyAnswers,
                        replyMarkup: new InlineKeyboardMarkup(CommandKeyboard.ToFinishButton)
                        );
                    break;
                case CaseStageMessage message:
                    UserCaseTableCommand.SetStartTime(userId, DateTime.Now);
                    await ShowAdditionalInfo(botClient, message, userId);
                    await botClient.SendTextMessageAsync(
                        chatId: userId,
                        text: message.TextBefore,
                        replyMarkup: new InlineKeyboardMarkup(CommandKeyboard.ToFinishButton)
                        );
                    break;
                case CaseStageNone none:
                    if(hp == 3) // начальное значение
                    {
                        UserCaseTableCommand.SetStartCaseTime(userId, DateTime.Now);
                        await CaseJsonCommand.CheckJsonFile($"{ConfigurationManager.AppSettings["PathStats"]}\\{userId}.json");
                    }
                    await botClient.SendTextMessageAsync(userId,
                        none.TextBefore,
                        replyMarkup: CommandKeyboard.StageMenu);
                    break;
                case CaseStageEndModule endStage:
                    var resultsCallback = EndStageCalc.GetResultOfModule(endStage, userId);
                    if(endStage.IsEndOfCase && hp <= 1) // На этом моменте hp в базе будет уже 0
                    {
                        UserCaseTableCommand.SetEndCaseTime(userId, DateTime.Now);
                    }
                    await botClient.SendTextMessageAsync(userId,
                        resultsCallback.Item1,
                        replyMarkup: resultsCallback.Item2);
                    break;
                default:
                    break;
            }
        }
        private async static Task ShowAdditionalInfo(ITelegramBotClient botClient, CaseStage nextStage, long userId)
        {
            if (nextStage.AdditionalInfo == null) return;
            foreach (string docName in nextStage.AdditionalInfo.GetNamesDocuments())
            {
                using (Stream fs = new FileStream(pathToCase + "\\" + docName.Trim(), FileMode.Open))
                {
                    var inputOnlineFile = new InputOnlineFile(fs, docName.Trim());
                    await botClient.SendDocumentAsync(userId, inputOnlineFile);
                }
            }
            foreach (string audioName in nextStage.AdditionalInfo.GetNamesDocuments())
            {
                using (Stream fs = new FileStream(pathToCase + "\\" + audioName.Trim(), FileMode.Open))
                {
                    var inputOnlineFile = new InputOnlineFile(fs, audioName.Trim());
                    await botClient.SendAudioAsync(userId, inputOnlineFile);
                }
            }
            if (nextStage.AdditionalInfo.GetNamesVideos().Count == 0 &&
                nextStage.AdditionalInfo.GetNamesPhotos().Count == 0) return;

            List<IAlbumInputMedia> mediaList = new List<IAlbumInputMedia>();
            foreach (string photoName in nextStage.AdditionalInfo.GetNamesPhotos())
            {
                using (Stream fs = new FileStream(pathToCase + "\\" + photoName.Trim(), FileMode.Open))
                {
                    var inputMediaFile = new InputMediaPhoto(new InputMedia(fs, photoName.Trim()));
                    mediaList.Add(inputMediaFile);
                }
            }
            foreach (string videoName in nextStage.AdditionalInfo.GetNamesVideos())
            {
                using (Stream fs = new FileStream(pathToCase + "\\" + videoName.Trim(), FileMode.Open))
                {
                    var inputMediaFile = new InputMediaVideo(new InputMedia(fs, videoName.Trim()));
                    mediaList.Add(inputMediaFile);
                }
            }

            await botClient.SendMediaGroupAsync(userId, mediaList);              
        }

        public static Dictionary<int, int> GetTaskCountDictionary()
        {
            return Stages.GetTaskCountDictionary();
        }

        public static List<int> GetStageNumbers(int moduleNumber)
        {
            return Stages.GetStageNumbers(moduleNumber);
        }
    }
}
