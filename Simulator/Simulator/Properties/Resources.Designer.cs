﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Simulator.Properties {
    using System;
    
    
    /// <summary>
    ///   Класс ресурса со строгой типизацией для поиска локализованных строк и т.д.
    /// </summary>
    // Этот класс создан автоматически классом StronglyTypedResourceBuilder
    // с помощью такого средства, как ResGen или Visual Studio.
    // Чтобы добавить или удалить член, измените файл .ResX и снова запустите ResGen
    // с параметром /str или перестройте свой проект VS.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Возвращает кэшированный экземпляр ResourceManager, использованный этим классом.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Simulator.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Перезаписывает свойство CurrentUICulture текущего потока для всех
        ///   обращений к ресурсу с помощью этого класса ресурса со строгой типизацией.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Добавление новой группы пользователей.
        /// </summary>
        internal static string AddNewGroupOfUsers {
            get {
                return ResourceManager.GetString("AddNewGroupOfUsers", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Это меню администратора.
        /// </summary>
        internal static string AdminMenu {
            get {
                return ResourceManager.GetString("AdminMenu", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Введите пароль.
        /// </summary>
        internal static string EnterPassword {
            get {
                return ResourceManager.GetString("EnterPassword", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Вход совершён успешно, переходим в главное меню.
        /// </summary>
        internal static string RightPassword {
            get {
                return ResourceManager.GetString("RightPassword", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Список групп, добавленных на курс.
        /// </summary>
        internal static string ShowGroups {
            get {
                return ResourceManager.GetString("ShowGroups", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Number === Password.
        /// </summary>
        internal static string ShowGroupsFormat {
            get {
                return ResourceManager.GetString("ShowGroupsFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Список пользователей, добавленных на курс.
        /// </summary>
        internal static string ShowUsers {
            get {
                return ResourceManager.GetString("ShowUsers", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Пользователи успешно добавлены.
        /// </summary>
        internal static string SuccessAddGroup {
            get {
                return ResourceManager.GetString("SuccessAddGroup", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Это ваша персональная карточка.
        /// </summary>
        internal static string UserCard {
            get {
                return ResourceManager.GetString("UserCard", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Это меню пользователя.
        /// </summary>
        internal static string UserMenu {
            get {
                return ResourceManager.GetString("UserMenu", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Здравствуйте, вы уже записаны на этот курс.
        /// </summary>
        internal static string WelcomeKnown {
            get {
                return ResourceManager.GetString("WelcomeKnown", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Приветствуем, вы администратор, как неожиданно и приятно!).
        /// </summary>
        internal static string WelcomeKnownAdmin {
            get {
                return ResourceManager.GetString("WelcomeKnownAdmin", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Приветствуем! К сожалению, вам недоступен этот курс.
        /// </summary>
        internal static string WelcomeUnknown {
            get {
                return ResourceManager.GetString("WelcomeUnknown", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Пароль неверный, попробуйте ещё раз.
        /// </summary>
        internal static string WrongPassword {
            get {
                return ResourceManager.GetString("WrongPassword", resourceCulture);
            }
        }
    }
}
