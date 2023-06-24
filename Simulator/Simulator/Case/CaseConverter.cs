﻿using Simulator.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Simulator.Case
{
    internal static class CaseConverter
    {
        public static void FromFile(string path)
        {
            using (Stream stream = new FileStream(path, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string[] stageStrings = reader.ReadToEnd().Split('$');
                    //Вопросы разделяются знаком $
                    StagesControl.Stages = new StageList() { Stages = new List<CaseStage>() };
                    foreach (string stageString in stageStrings)
                    {
                        StagesControl.Stages.Stages.Add(StringToStage(stageString));
                    }
                }
            }
        }
        public static CaseStage StringToStage(string stringStage)
        {
            CaseStage stage = null;
            string[] stageParameters = stringStage.Split('*');
            string type = stageParameters[0].Trim();
            int number = int.Parse(stageParameters[1].Trim());
            string textBefore = stageParameters[2].Trim();
            switch (type)
            {
                case "poll":
                    stage = new CaseStagePoll();
                    MakeStagePoll(stage as CaseStagePoll, stageParameters);
                    break;
                case "none":
                    stage = new CaseStageNone();
                    MakeStageNone(stage as CaseStageNone, stageParameters);
                    break;
                case "end":
                    stage = new CaseStageEndModule();
                    MakeStageEnd(stage as CaseStageEndModule, stageParameters);
                    break;
                case "message":
                    stage = new CaseStageMessage();
                    MakeStageMessage(stage as CaseStageMessage, stageParameters);
                    break;
                default:
                    break;
            }
            stage.ModuleNumber = int.Parse(stageParameters[3].Trim());
            return stage;
        }

        private static void MakeStageMessage(CaseStageMessage stage, string[] stageParameters)
        {
            stage.NextStage = int.Parse(stageParameters[4].Trim());
            stage.Rate = double.Parse(stageParameters[5].Trim());
            switch (stageParameters[6].Trim())
            {
                case "video":
                    stage.MessageTypeAnswer = Telegram.Bot.Types.Enums.MessageType.Video;
                    break;
                default:
                    break;
            }
        }

        private static void MakeStageEnd(CaseStageEndModule stage, string[] stageParameters)
        {
            stage.IsEndOfCase = bool.Parse(stageParameters[4].Trim());

            foreach (var rate in stageParameters[5].Split('^'))
            {
                stage.Rates.Add(double.Parse(rate.Trim()));
            }
            foreach (var text in stageParameters[6].Split('^'))
            {
                stage.Texts.Add(text.Trim());
            }

            if (stage.IsEndOfCase)
            {
                stage.NextStage = stage.Number;
            }
            else
            {
                stage.NextStage = int.Parse(stageParameters[7].Trim());
            }
        }

        private static void MakeStageNone(CaseStageNone stage, string[] stageParameters)
        {
            stage.NextStage = int.Parse(stageParameters[4].Trim());
        }

        private static void MakeStagePoll(CaseStagePoll stage, string[] stageParameters)
        {
            stage.ManyAnswers = bool.Parse(stageParameters[4].Trim());

            foreach (var option in stageParameters[5].Split('^'))
            {
                stage.Options.Add(option.Trim());
            }

            stage.ConditionalMove = bool.Parse(stageParameters[6]);

            foreach (var option in stageParameters[7].Split('^'))
            {
                string[] rates = option.Trim().Split('-');
                stage.PossibleRate.Add(int.Parse(rates[0]), double.Parse(rates[1]));
            }

            if (stage.ConditionalMove)
            {
                stage.MovingNumbers = new Dictionary<int, int>();
                foreach (var option in stageParameters[8].Split('^'))
                {
                    string[] numbers = option.Trim().Split('-');
                    stage.MovingNumbers.Add(int.Parse(numbers[0]), int.Parse(numbers[1]));
                }
            }
            else
            {
                stage.NextStage = int.Parse(stageParameters[8].Trim());
            }
            stage.AdditionalInfoType = (AdditionalInfo)Enum.Parse(typeof(AdditionalInfo), stageParameters[9]);
            stage.NamesAdditionalFiles = new List<string>();
            foreach (string fileName in stageParameters[10].Split('^'))
            {
                stage.NamesAdditionalFiles.Add(fileName);
            }
            if (stage.ManyAnswers)
            {
                stage.Limit = int.Parse(stageParameters[11].Trim());
                stage.Fine = double.Parse(stageParameters[12].Trim());
                stage.WatchNonAnswer = bool.Parse(stageParameters[13]);
                if(stage.WatchNonAnswer)
                {
                    stage.NonAnswers = new Dictionary<int, double>();
                    foreach (var option in stageParameters[14].Split('^'))
                    {
                        string[] rates = option.Trim().Split('-');
                        stage.NonAnswers.Add(int.Parse(rates[0]), double.Parse(rates[1]));
                    }
                }
            }        
        }
    }
}
