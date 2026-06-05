using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Effects;
using Newtonsoft.Json;
using SmartSchedule.ViewModel;

namespace SmartSchedule.Services
{
    // 1.1: make all IO operations async
    // 1.0.1: Add periodic backups
    class FileIOService
    {
        private readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SmartSchedule", "tCDataList.json");
        private readonly string _backupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SmartSchedule", "backup.json");

        public ObservableCollection<TCViewModel> LoadData()
        {
            // Verify save folder existence.
            CheckDirectoryExists(_path);

            // Verify json file existence.
            var fileExists = File.Exists(_path);
            var backupFileExists = File.Exists(_backupPath);
            if (!fileExists)
            {
                File.CreateText(_path).Dispose();
                return new ObservableCollection<TCViewModel>() { new TCViewModel() };
            }
            // Verify json backup file existence.
            if (!backupFileExists)
            {
                File.CreateText(_backupPath).Dispose();
            }
            try
            {
                using (var reader = File.OpenText(_path))
                {
                    var fileText = reader.ReadToEnd();
                    var data = JsonConvert.DeserializeObject<ObservableCollection<TCViewModel>>(fileText) ?? new ObservableCollection<TCViewModel>() { new TCViewModel() };
                    File.Copy(_path, _backupPath, overwrite: true);
                    return data;
                }
            }
            // in case, if sometning goes wrong and app can not load the task list
            catch (Exception ex) when (ex is JsonException || ex is IOException)
            {
                try
                {
                    using (var reader = File.OpenText(_backupPath))
                    {
                        var backupFileText = reader.ReadToEnd();

                        var backupData = JsonConvert.DeserializeObject<ObservableCollection<TCViewModel>>(backupFileText) ?? new ObservableCollection<TCViewModel>() { new TCViewModel() };

                        File.Copy(_backupPath, _path, overwrite: true);

                        // move this message to codebehind somehow
                        //Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        //{
                        //    MessageBox.Show(
                        //        "Failed to load the task list, but don't worry, all your tasks from last session are in safety.",
                        //        "Restore data",
                        //        MessageBoxButton.OK,
                        //        MessageBoxImage.Information
                        //    );
                        //}));

                        return backupData;
                    }
                }
                // in case both json files were corrupted
                catch
                {
                    if (File.Exists(_path))
                    {
                        File.Delete(_path);
                    }
                    if (File.Exists(_backupPath))
                    {
                        File.Delete(_backupPath);
                    }
                    throw;
                }
            }
        }

        public void SaveData(object tCDataList)
        {
            CheckDirectoryExists(_path);

            using (var writer = File.CreateText(_path))
            {
                string output = JsonConvert.SerializeObject(tCDataList, Formatting.Indented);
                writer.Write(output);
            }
        }

        private void CheckDirectoryExists(string filePath)
        {
            string? directoryPath = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath); 
            }
        }
    }
}
