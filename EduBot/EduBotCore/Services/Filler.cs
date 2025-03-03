﻿using EduBot.Commands;
using System.Reflection;

namespace EduBot.Services
{
    public static class Filler
    {
        public static Dictionary<string, Command> FillCommandDictionary()
        {
            Dictionary<string, Command> result = new Dictionary<string, Command>();

            Type baseType = typeof(Command);
            IEnumerable<Type> listOfSubclasses = Assembly.GetAssembly(baseType)
                .GetTypes()
                .Where(type => type.IsSubclassOf(baseType));

            foreach (Type type in listOfSubclasses)
            {
                Command commandObject = Activator.CreateInstance(type) as Command;
                result.Add(type.Name, commandObject);
            }
            return result;
        }

        public static Dictionary<string, string> FillAccordanceDictionaryButton()
        {
            Dictionary<string, string> result = new()
            {
                { "Login", "LogInCommand" },
                { "MainMenu", "GoToMainMenuCommand" },
                { "UserCard", "UserCardCommand" },
                { "ShowUsersInfo", "AdminShowUsersInfoCommand" },
                { "ListGroups", "AdminShowGroupsInfoCommand" },
                { "AddUsersAdmin", "AdminAddNewUsersCommand" },
                { "AddCase", "AdminAddCaseCommand" },
                { "ToCase", "UserGoToCaseCommand" },
				{ "ToListCourses", "UserGetListCoursesCommand" },
				{ "GetStatistics", "AdminGetStatisticsCommand" },
                { "GetListCourses", "AdminShowListCoursesForStatistics" },
                { "CreateCase", "AdminCreateCaseCommand" },
                { "CheckTelegramId", "TelegramIDCommand" },
                { "AddGroupLider", "AdminAddGroupLiderCommand" },
                { "AnswersFirst", "AdminAnswersCommand" },
                { "AnswersTypes", "AdminAnswersTypesCommand" },
                { "ShowAnswers", "AdminShowAnswersCommand" },
                { "EditUserInfo", "EditUserInfoCommand" },
                { "AddToCourse", "AdminShowCoursesToAdd" },
                { "AddGroupToCourse", "AdminAddGroupToCourseCommand" }
            };
            return result;
        }

        public static Dictionary<string, string> FillAccordanceDictionaryText()
        {
            Dictionary<string, string> result = new()
            {
                { "/start", "WelcomeCommand" },
                { "/skip", "SkipCommand" }
            };

            return result;
        }
    }
}
