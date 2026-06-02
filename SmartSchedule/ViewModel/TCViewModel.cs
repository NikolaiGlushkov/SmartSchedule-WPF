using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using SmartSchedule.Commands;
using SmartSchedule.Model;

namespace SmartSchedule.ViewModel
{
    public class TCViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<DGModel> _dgDataList;

        private string _header;

        public ICommand AddRowCommand { get; }
        public ICommand DeleteRowCommand { get; }

        public TCViewModel()
        {
            AddRowCommand = new RelayCommand<DGModel>(AddRow);

            DeleteRowCommand = new RelayCommand<DGModel>(DeleteRow);

            Header = "new tab";

            DGDataList = new ObservableCollection<DGModel>();
        }

        public string Header
        {
            get
            {
                return _header;
            }
            set
            {
                if (_header == value)
                    return;
                _header = value;
                OnPropertyChanged("Header");
            }
        }

        public ObservableCollection<DGModel> DGDataList
        {
            get
            {
                return _dgDataList;
            }

            private set
            {
                _dgDataList = value;
            }
        }



        private void AddRow(object parameter)
        {
            var validationView = CollectionViewSource.GetDefaultView(DGDataList) as IEditableCollectionView;

            
            if (validationView != null && (validationView.IsEditingItem || validationView.IsAddingNew))
            {
                if (validationView.IsEditingItem)
                    validationView.CommitEdit();
                if (validationView.IsAddingNew)
                    validationView.CommitNew();
            }


            if (DGDataList.Count >= 30)
            {
                MessageBox.Show("You have reached the task limit. Don't let your tasks pile up.");
                return;
            }

            var view = validationView as ICollectionView;

            if (view != null)
            {
                view.SortDescriptions.Clear();
                DGDataList.Add(new DGModel());
            }
        }


        private void DeleteRow(DGModel item)
        {
            MessageBoxResult deleteConfirmation = MessageBox.Show("Do you want to delete this task?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (deleteConfirmation == MessageBoxResult.Yes)
            {
                DGDataList.Remove(item);
            }

        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
