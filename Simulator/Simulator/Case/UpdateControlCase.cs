﻿using Simulator.Commands;
using Simulator.Models;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Simulator.TelegramBotLibrary;
using System;
using System.IO;

namespace Simulator.Case
{
    internal static class UpdateControlCase
    {
        public static async Task CallbackQueryHandlingCase(CallbackQuery query, ITelegramBotClient botClient)
        {
            long userId = query.Message.Chat.Id;
            CaseStage currentStage = StagesControl.Stages[UserCaseTableCommand.GetPoint(userId)];
            //Получили этап, который пользователь только что прошёл, нажав на кнопку

            CaseStage nextStage = StagesControl.GetNextStage(currentStage, query);
            //Получаем следующий этап, исходя из кнопки, на которую нажал пользователь

            if(nextStage == null)
            {
                if (currentStage is CaseStageEndModule endModule)
                {
                    if(!endModule.IsEndOfCase)
                    {
                        return;
                    }
                    else
                    {
                        if (UserCaseTableCommand.GetHealthPoints(userId) != 0)
                        {
                            return;
                        }
                    }
                }
                await GoOut(userId, botClient);
                return;
            }

            await SetAndMovePoint(userId, nextStage, botClient);
        }
        public static async Task PollAnswerHandlingCase(PollAnswer answer, ITelegramBotClient botClient)
        {
            long userId = answer.User.Id;
            int attemptNo = UserCaseTableCommand.GetHealthPoints(userId) > 1 ? 1 : 2;
            CaseStagePoll currentStage = (CaseStagePoll)StagesControl.Stages[UserCaseTableCommand.GetPoint(userId)];
            //Так как мы получаем PollAnswer, то очевидно, что текущий этап - опросник

            StagesControl.SetStageForMove(currentStage, answer.OptionIds);
            //По свойствам опросника и ответу определояем свойство NextStage

            double rate = StagesControl.CalculateRatePoll(currentStage, answer.OptionIds);
            double currentUserRate = UserCaseTableCommand.GetRate(userId);
            currentUserRate += rate;
            UserCaseTableCommand.SetRate(userId, currentUserRate);
            //Считаем на основе ответа очки пользователя и добавляем их к общим

            await SetResultsJson(userId, currentStage, rate, attemptNo, answer.OptionIds);

            var nextStage = StagesControl.Stages[currentStage.NextStage]; //next уже установлено
            //await botClient.message
            await SetAndMovePoint(userId, nextStage, botClient);
        }

        public static async Task MessageHandlingCase(Message message, ITelegramBotClient botClient)
        {
            long userId = message.Chat.Id;
            int attemptNo = UserCaseTableCommand.GetHealthPoints(userId);
            CaseStageText currentStage = (CaseStageText)StagesControl.Stages[UserCaseTableCommand.GetPoint(userId)];
            CaseStagePoll moduleQuestionNumber = (CaseStagePoll)StagesControl.Stages[UserCaseTableCommand.GetPoint(userId)];
            switch (currentStage.MessageTypeAnswer)
            {
                case Telegram.Bot.Types.Enums.MessageType.Video:

                    // сохраняем видос
                    var video = message.Video;
                    string fileName = $"{userId}-{moduleQuestionNumber.Number}.mp4";
                    string filePath = $"temp/answers/videos/{fileName}";
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await botClient.DownloadFileAsync(video.FileId, stream);
                    }

                    // добавляем птс за видос
                    double currentUserRate = UserCaseTableCommand.GetRate(userId);
                    currentUserRate += 1.0;
                    UserCaseTableCommand.SetRate(userId, currentUserRate);
                    //Считаем на основе ответа очки пользователя и добавляем их к общим

                    // юзер прислал видео, вариантов ответа нету
                    int[] emptyArray = new int[0];

                    await UserCaseJsonCommand.AddValueToJsonFile(userId, (moduleQuestionNumber.ModuleNumber, moduleQuestionNumber.Number), 1.0, attemptNo);
                    break;
                default:
                    break;
            }
        }

        private static async Task SetResultsJson(long userId, CaseStage currentStage, double rate, int attemptNo, int[] optionsIds)
        {
            int moduleNumber = currentStage.ModuleNumber;
            int questionNumber = currentStage.Number;

            var time = DateTime.Now - UserCaseTableCommand.GetStartTime(userId);

            StageResults results = new StageResults();
            results.Time = time;
            results.Rate = rate;
           
            //варианты ответа
            if (currentStage is CaseStagePoll)
            {
                string jsonUserAnswers = "";
                foreach (int option in optionsIds)
                {
                    jsonUserAnswers += $"{option + 1};";
                    // счёт идёт от 0, а надо от 1, поэтому +1
                }
                results.Answers = jsonUserAnswers; 
            }
            await UserCaseJsonCommand.AddValueToJsonFile(userId, (moduleNumber, questionNumber), results, attemptNo);
        }

        private static async Task GoOut(long userId, ITelegramBotClient botClient)
        {
            UserCaseTableCommand.SetOnCourse(userId, false);
            var outCommand = new GoToMainMenuUserCommand();
            await outCommand.Execute(userId, botClient);
        }
        private async static Task SetAndMovePoint(long userId, CaseStage nextStage, ITelegramBotClient botClient)
        {
            UserCaseTableCommand.SetPoint(userId, nextStage.Number);
            //Ставим в базе для пользователя следующий его этап

            await StagesControl.Move(userId, nextStage, botClient);
            //Выдаём пользователю следующий этап
        }
    }
}
