using System;
using System.IO;
using System.Xml;

namespace BallBotGui
{
    public static class AppConfigHelper
    {
        private static string ConfigPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BallBotGui.dll.config");

        public static void SaveSetting(string name, string value)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(ConfigPath);

                var node = doc.SelectSingleNode($"//setting[@name='{name}']/value");
                if (node != null)
                {
                    node.InnerText = value;
                }
                else
                {
                    // Если узла нет, попробуем найти секцию и добавить
                    var section = doc.SelectSingleNode("//BallBotGui.Properties.Settings");
                    if (section != null)
                    {
                        var newSetting = doc.CreateElement("setting");
                        newSetting.SetAttribute("name", name);
                        newSetting.SetAttribute("serializeAs", "String");
                        var newValue = doc.CreateElement("value");
                        newValue.InnerText = value;
                        newSetting.AppendChild(newValue);
                        section.AppendChild(newSetting);
                    }
                }

                doc.Save(ConfigPath);
            }
            catch (Exception ex)
            {
                // Если не получилось сохранить в файл (например, нет прав доступа), 
                // можно вывести ошибку или просто проигнорировать
                System.Windows.Forms.MessageBox.Show($"Не удалось сохранить в {ConfigPath}: {ex.Message}");
            }
        }

        public static string LoadSetting(string name, string defaultValue = "")
        {
            try
            {
                if (!File.Exists(ConfigPath)) return defaultValue;

                var doc = new XmlDocument();
                doc.Load(ConfigPath);

                var node = doc.SelectSingleNode($"//setting[@name='{name}']/value");
                return node?.InnerText ?? defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
