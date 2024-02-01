﻿using Simulator.Models;
using SimulatorCore.Properties;
using System.Text;

namespace SimulatorCore.DbLibrary.StatsTableCommand
{
    public enum StatsTableType
    {
        Rate,
        Answers,
        Time,
        Base,
        State,
    }

    //Тут создаются таблицы со статистикой и заполняются поумолчанию
    public class UserStatsControl : CommandTable
    {
        private const int COUNT_TABLES_WITH_ALL_STAGES = 3;
        private List<string> FillTableNames(string courseName)
        {
            List<string> tableNames = new List<string>();
            foreach (string typeName in Enum.GetNames(typeof(StatsTableType)))
            {
                string pattern = $"Stats{courseName}{typeName}";
                tableNames.Add(pattern);
            }
            return tableNames;
        }
        public async Task DeleteStatsTables(string courseName)
        {
            List<string> tableNames = FillTableNames(courseName);
            string commandText = string.Empty;
            foreach (string tableName in tableNames)
            {
                commandText += $"DROP TABLE {tableName};\n";
            }
            await ExecuteNonQueryCommand(commandText);
        }

        internal async Task MakeStatsTables(StageList stages)
        {
            List<string> tableNames = FillTableNames(stages.CourseName);
            for (int i = 0; i < COUNT_TABLES_WITH_ALL_STAGES; i++)
            {
                string dataType = i != 1 ? "DOUBLE" : "NVARCHAR(20)";
                await MakeStatsAllStages(stages, tableNames[i], dataType);
            }
            await MakeBaseStatsTable(stages.AttemptCount, tableNames[3]);
            await MakeStateStatsTable(tableNames[4]);
            await FillTablesDefaultInformation(tableNames);
            await FillStateStatsTable(stages.AttemptCount, stages.ExtraAttempt, tableNames[4]);
        }

        private async Task FillStateStatsTable(int attemptCount, bool extraAttempt, string tableName)
        {
            string commandText = $"UPDATE {DbConfigProperties.DatabaseName}.{tableName} SET ExtraAttempt = {extraAttempt}, Attempts = {attemptCount}";
            await ExecuteNonQueryCommand(commandText);
        }

        private async Task FillTablesDefaultInformation(List<string> tableNames)
        {
            string commandText;
            foreach (string tableName in tableNames)
            {
                commandText = $"INSERT INTO {DbConfigProperties.DatabaseName}.{tableName} (UserID) SELECT UserID from Users";
                await ExecuteNonQueryCommand(commandText);
            }
        }

        private async Task MakeStateStatsTable(string tableName)
        {
            StringBuilder commandText = new StringBuilder($"CREATE TABLE {tableName} (\n");

            commandText.AppendLine("UserID INT NOT NULL PRIMARY KEY REFERENCES Users(UserID),");
            commandText.AppendLine("Point INT NOT NULL DEFAULT 0,");
            commandText.AppendLine("ExtraAttempt BOOLEAN NULL,");
            commandText.AppendLine("Attempts INT NULL,");
            commandText.AppendLine("Rate DOUBLE NOT NULL DEFAULT 0,");
            commandText.AppendLine("StartTime DATETIME NULL");

            commandText.Append(')');

            await ExecuteNonQueryCommand(commandText.ToString());
        }

        private async Task MakeBaseStatsTable(int attemptCount, string tableName)
        {
            StringBuilder commandText = new StringBuilder($"CREATE TABLE {tableName} (\n");

            commandText.AppendLine("UserID INT NOT NULL PRIMARY KEY REFERENCES Users(UserID),");
            commandText.AppendLine($"StartCourseTime DATETIME NULL,");
            commandText.AppendLine($"EndCourseTime DATETIME NULL,");
            commandText.AppendLine($"AttemptsUsed INT NOT NULL DEFAULT 0,");

            for (int i = 1; i < attemptCount; i++)
            {
                commandText.AppendLine($"RateAttempt{i} DOUBLE NOT NULL DEFAULT 0,");
            }
			commandText.AppendLine($"RateAttempt{attemptCount} DOUBLE NOT NULL DEFAULT 0)");

			await ExecuteNonQueryCommand(commandText.ToString());
        }

        // Создаёт таблицу, столбцы которой - сначала все вопросы первой попытки,
        // Потом второй, и так далее, в зависимости от попыток
        private async Task MakeStatsAllStages(StageList stages, string tableName, string DataType)
        {
            StringBuilder commandText = new StringBuilder($"CREATE TABLE {tableName} (\n");

            commandText.AppendLine("UserID INT NOT NULL PRIMARY KEY REFERENCES Users(UserID),");

            List<StringBuilder> commandTextAnyAttempt = new();
            for (int i = 0; i < stages.AttemptCount; i++)
            {
                commandTextAnyAttempt.Add(new StringBuilder());
            }

            for (int j = 0; j < stages.GetLastStageIndex(); j++)
            {
                CaseStage currentStage = stages[j];
                if (currentStage is CaseStagePoll || currentStage is CaseStageMessage)
                {
                    for (int i = 0; i < stages.AttemptCount; i++)
                    {
                        commandTextAnyAttempt[i].AppendLine($"P{currentStage.Number}M{currentStage.ModuleNumber}A{i + 1}" +
                            $" {DataType} NOT NULL DEFAULT 0,");
                    }
                }
            }

            for (int i = 0; i < stages.AttemptCount - 1; i++)
            {
                commandText.Append(commandTextAnyAttempt[i]);
            }
            var last = commandTextAnyAttempt[stages.AttemptCount - 1];
            var lastStr = last.ToString();
            lastStr = lastStr.Remove(lastStr.Length - 3);
            commandText.Append(lastStr);
            commandText.Append(')');
			
			await ExecuteNonQueryCommand(commandText.ToString());
        }
    }
}
