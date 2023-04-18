﻿using Simulator.BotControl;
using Simulator.Models;
using System;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace Simulator.Services
{
    enum ResultType
    {
        SafeFirst,
        FirstFail,
        SecondFail,
        Success,
    }

    internal static class EndStageCalc
    {
        public static ValueTuple<string, InlineKeyboardMarkup> GetResultOfModule(CaseStageEndModule descriptor, long userId)
        {
            double currentRate = 0;
            List<InlineKeyboardButton[]> buttons = new List<InlineKeyboardButton[]>()
            {
                new[]{ CommandKeyboard.ToFinishButton },
            };
            var result = new ValueTuple<string, InlineKeyboardMarkup>();
            
            currentRate = UserCaseTableCommand.GetRate(userId);
            int attemptNumber = UserCaseTableCommand.HPoints(userId) > 1 ? 1 : 2;
            //Если жизней больше, чем 1, то это первая попытка, иначе 2.
            
            if(descriptor.IsEndOfCase)
            {
                if(currentRate < descriptor.Rates[0])
                {
                    int ratePlace = 0;
                    result = GetResultEnd(attemptNumber, descriptor, buttons, ratePlace); 
                }
                else if(currentRate < descriptor.Rates[1])
                {
                    int ratePlace = 1;
                    result = GetResultEnd(attemptNumber, descriptor, buttons, ratePlace);
                }
                else
                {
                    int ratePlace = 2;
                    result = GetResultEnd(attemptNumber, descriptor, buttons, ratePlace);
                }
                UserCaseTableCommand.SetHPoints(userId, 2-attemptNumber);
                //Если была первая попытка, то поставится 1, если вторая, то 0
            }
            else if(currentRate < descriptor.Rates[0])
            {
                if(descriptor.ModuleNumber == 1 &&
                    UserCaseTableCommand.HPoints(userId)==3)
                {
                    result = GetResult(buttons, descriptor, ResultType.SafeFirst);
                    UserCaseTableCommand.SetHPoints(userId, 2);
                    //Снимается дополнительная попытка на 1 модуль
                }
                else if(attemptNumber == 1)
                {
                    result = GetResult(buttons, descriptor, ResultType.FirstFail);
                    UserCaseTableCommand.SetHPoints(userId, 1);
                }
                else if(attemptNumber == 2)
                {
                    result = GetResult(buttons, descriptor, ResultType.SecondFail);
                }
            }
            else
            {
                result = GetResult(buttons, descriptor, ResultType.Success);
            }
            return result;
        }
        private static ValueTuple<string, InlineKeyboardMarkup> GetResultEnd(
            int attemptNumber,
            CaseStageEndModule descriptor,
            List<InlineKeyboardButton[]> buttons,
            int ratePlace)
        {
            var result = new ValueTuple<string, InlineKeyboardMarkup>();
            if (attemptNumber == 1)
            {
                result.Item1 = descriptor.Texts[ratePlace]; //Информация о результате
                result.Item1 += descriptor.TextBefore; //Сказать, что есть ещё попытка
                buttons.Add(new[] { CommandKeyboard.ToBeginButton });
                result.Item2 = new InlineKeyboardMarkup(buttons);
            }
            else
            {
                result.Item1 = descriptor.Texts[ratePlace];
                result.Item1 += descriptor.TextAfter; //Сказать, что попыток больше нет
            }
            return result;
        }
        private static ValueTuple<string, InlineKeyboardMarkup> GetResult(
            List<InlineKeyboardButton[]> buttons,
            CaseStageEndModule descriptor,
            ResultType resultType
            )
        {
            var result = new ValueTuple<string, InlineKeyboardMarkup>();
            switch (resultType)
            {
                case ResultType.SafeFirst:
                    result.Item1 = descriptor.Texts[0];
                    buttons.Add(new[] { CommandKeyboard.ToBeginButton });
                    break;
                case ResultType.FirstFail:
                    result.Item1 = descriptor.Texts[1];
                    buttons.Add(new[] { CommandKeyboard.ToBeginButton });
                    break;
                case ResultType.SecondFail:
                    result.Item1 = descriptor.Texts[2];
                    buttons.Add(new[] { CommandKeyboard.NextButton });
                    break;
                case ResultType.Success:
                    result.Item1 = descriptor.Texts[3];
                    buttons.Add(new[] { CommandKeyboard.NextButton });
                    break;
                default:
                    break;
            }
            result.Item2 = new InlineKeyboardMarkup(buttons);
            return result;

        }
    }
}
