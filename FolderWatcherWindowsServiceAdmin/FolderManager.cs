using FolderWatcherWindowsServiceAdmin.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace FolderWatcherWindowsServiceAdmin
{
    internal class FolderManager
    {
        private string _configFilePath;
        private Configuration _config;
        private List<string> _folders;

        public FolderManager()
        {
            FindConfigFile();

            var configMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = _configFilePath
            };

            _config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            _folders = new List<string>();
        }

        private void FindConfigFile()
        {
            try
            {
                // Try multiple possible locations for the config file
                var possiblePaths = new[]
                {
                    Path.Combine(Application.StartupPath, "FolderWatcherWindowsService.exe.config"),
                    Path.Combine(Application.StartupPath, "..", "FolderWatcherWindowsService", "FolderWatcherWindowsService.exe.config"),
                    Path.Combine(Application.StartupPath, "FolderWatcherWindowsService.exe"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "FolderWatcher", "FolderWatcherWindowsService.exe.config")
                };

                foreach (var path in possiblePaths)
                {
                    var configPath = path.EndsWith(".exe")
                        ? path + ".config"
                        : path;

                    if (File.Exists(configPath))
                    {
                        _configFilePath = configPath;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(_configFilePath))
                {
                    _configFilePath = Path.Combine(Application.StartupPath, "FolderWatcherWindowsService.exe.config");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error finding config file: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public List<string> GetWatchedFolders()
        {
            try
            {
                List<string> folders = new List<string>();
                
                // Access userSettings section where FolderPaths is actually stored
                var userSettingsGroup = _config.GetSectionGroup("userSettings") as UserSettingsGroup;
                var settingsSection = userSettingsGroup?.Sections["FolderWatcherWindowsService.Properties.Settings"] as ClientSettingsSection;
                var folderPathsSetting = settingsSection?.Settings.Get("FolderPaths");
                
                if (folderPathsSetting?.Value?.ValueXml?.InnerXml != null)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(string[]));
                    using (StringReader reader = new StringReader(folderPathsSetting.Value.ValueXml.InnerXml))
                    {
                        string[] folderArray = (string[])serializer.Deserialize(reader);
                        folders.AddRange(folderArray);
                    }
                }

                _folders = folders;
                return _folders;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading watched folders: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return new List<string>();
            }
        }

        public void SaveWatchedFolders(List<string> folders)
        {
            try
            {
                // Access userSettings section where FolderPaths is actually stored
                var userSettingsGroup = _config.GetSectionGroup("userSettings") as UserSettingsGroup;
                var settingsSection = userSettingsGroup?.Sections["FolderWatcherWindowsService.Properties.Settings"] as ClientSettingsSection;
                
                XmlSerializer serializer = new XmlSerializer(typeof(string[]));
                using (StringWriter writer = new StringWriter())
                {
                    serializer.Serialize(writer, folders.ToArray());
                    string xmlContent = writer.ToString();
                    
                    // Remove XML declaration
                    if (xmlContent.StartsWith("<?xml"))
                    {
                        int firstNewLine = xmlContent.IndexOf('\n');
                        if (firstNewLine > 0)
                            xmlContent = xmlContent.Substring(firstNewLine + 1).Trim();
                    }
                    
                    var folderPathsSetting = settingsSection?.Settings.Get("FolderPaths");
                    if (folderPathsSetting != null)
                    {
                        folderPathsSetting.Value.ValueXml.InnerXml = xmlContent;
                    }
                    else if (settingsSection != null)
                    {
                        var newSetting = new SettingElement("FolderPaths", System.Configuration.SettingsSerializeAs.Xml);
                        newSetting.Value.ValueXml.InnerXml = xmlContent;
                        settingsSection.Settings.Add(newSetting);
                    }
                }
                
                _config.Save(ConfigurationSaveMode.Modified);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving config file: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public static bool IsSubfolder(string parentPath, string childPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(parentPath) || string.IsNullOrWhiteSpace(childPath))
                    return false;

                var parentUri = new Uri(Path.GetFullPath(parentPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);
                var childUri = new Uri(Path.GetFullPath(childPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);

                bool result = parentUri.IsBaseOf(childUri) && !parentUri.Equals(childUri);
                
                // Debug logging to verify the logic
                System.Diagnostics.Debug.WriteLine($"IsSubfolder('{parentPath}', '{childPath}') = {result}");
                System.Diagnostics.Debug.WriteLine($"  Parent URI: {parentUri}");
                System.Diagnostics.Debug.WriteLine($"  Child URI: {childUri}");
                System.Diagnostics.Debug.WriteLine($"  IsBaseOf: {parentUri.IsBaseOf(childUri)}, NotEqual: {!parentUri.Equals(childUri)}");
                
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IsSubfolder error: {ex.Message}");
                return false;
            }
        }

        public bool ValidateFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                return false;

            try
            {
                return Directory.Exists(folderPath);
            }
            catch
            {
                return false;
            }
        }

        public string ConfigFilePath => _configFilePath;
    }
}