﻿using Simulator.BotControl;
using System.Text.RegularExpressions;

using DbGroup = SimulatorCore.Models.DbModels.Group;

namespace Simulator.Services
{
    public static class GroupHandler
    {
        public static async Task<bool> AddGroup(string groupNumber)
        {
            bool hasGroup = true;
            try
            {
                await DataBaseControl.GetEntity<DbGroup>(groupNumber);
            }
            catch (KeyNotFoundException)
            {
                hasGroup = false;
            }
            if (!hasGroup)
            {
                DbGroup group = new DbGroup() { GroupNumber = groupNumber };
                group.SetPassword();
                await DataBaseControl.AddEntity<DbGroup>(group);
                return true;
            }
            return false;
        }
        public static bool IsCorrectGroupNumber(string groupNumber)
        {
            Regex regex = new Regex("^[0-9]{7}-[0-9]{5}$");
            return regex.IsMatch(groupNumber);
        }
    }
}
