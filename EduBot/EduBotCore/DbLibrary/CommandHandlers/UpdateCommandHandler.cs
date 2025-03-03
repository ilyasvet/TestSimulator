﻿using DbLibrary.Reflection;
using EduBotCore.Properties;
using System.Text;

namespace DbLibrary.CommandHandlers
{
    public class UpdateCommandHandler<T> : CommandHandlerBase<T> where T : new()
    {
        public UpdateCommandHandler() : base() { }

        public async Task<int> UpdateEntity(object primaryKey, T newEntity)
        {
            if (GetPrimaryKeyValue(primaryKey, out string value))
            {
                var commandText = GetCommandTextBase();
                commandText.Append($"{Properties.GetPropertiesDataStringUpdate(newEntity)} ");
                commandText.Append($"WHERE {_primaryKeyName} = {value}");

                return await ExecuteNonQuery(commandText.ToString());
            }
            throw new NotSupportedException("There is not primary key in table");
        }

        protected override StringBuilder GetCommandTextBase()
        {
            StringBuilder result = new StringBuilder();
            result.Append($"UPDATE {DbConfigProperties.DatabaseName}.{_tableName} SET ");    

            return result;
        }
    }
}
