using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media.Effects;
using Newtonsoft.Json;
using SmartSchedule.ViewModel;

namespace SmartSchedule.Services
{
    // 1.1 Implement backup system to prevent data loss.
    // 1.1 Try to get rid of repeating code
    class FileIOService
    {
        private readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SmartSchedule", "tCDataList.json");    

        public ObservableCollection<TCViewModel> LoadData()
        {
            // Verify save folder existence.
            string? directoryPath = Path.GetDirectoryName(_path);

            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath); 
            }

            // Verify json file existence.
            var fileExists = File.Exists(_path);

            if (!fileExists)
            {
                File.CreateText(_path).Dispose();
                return new ObservableCollection<TCViewModel>() { new TCViewModel() };
            }
            using (var reader = File.OpenText(_path))
            {
                var fileText = reader.ReadToEnd();

                return JsonConvert.DeserializeObject<ObservableCollection<TCViewModel>>(fileText) ?? new ObservableCollection<TCViewModel>();
            }
        }

        public void SaveData(object tCDataList)
        {
            string? directoryPath = Path.GetDirectoryName(_path);

            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath); 
            }

            using (var writer = File.CreateText(_path))
            {
                string output = JsonConvert.SerializeObject(tCDataList, Formatting.Indented);
                writer.Write(output);
            }
        }
    }
}
