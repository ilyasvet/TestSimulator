﻿using DbLibrary.Attributes;
using EduBotCore.Properties;
using EduBot.Services;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduBotCore.Models.DbModels
{
    [Table("users")]
    [PrimaryKey("UserID", typeof(long))]
    public class User
    {
        private long _userID;
        public long UserID
        {
            get => _userID;
            set
            {
                if (value < 0)
                    throw new ArgumentException(Resources.WrongFormatID);
                _userID = value;
            }
        }
        private string? _name;
        public string? Name
        {
            get => _name;
            set
            {
                if (!UserHandler.IsCorrectName(value))
                    throw new ArgumentException(Resources.WrongFormatName);
                _name = value;
            }
        }
        private string? _surname;
        public string? Surname
        {
            get => _surname;
            set
            {
                if (!UserHandler.IsCorrectName(value))
                    throw new ArgumentException(Resources.WrongFormatSurname);
                _surname = value;
            }
        }
        private string? _groupNumber;
        public string? GroupNumber
        {
            get => _groupNumber;
            set
            {
                _groupNumber = value;
            }
        }
        public override string ToString()
        {
            string result = Name + " " + Surname;
            return result;
        }
    }
}
