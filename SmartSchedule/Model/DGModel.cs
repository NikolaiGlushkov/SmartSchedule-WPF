using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SmartSchedule.Model
{
    // 1.1: add time
    public class DGModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private DateTime? _deadLine;
        private bool _isDone;
        private bool _isImportant;
        private string _expenses;
        private string _text;
        private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

        public DGModel()
        {

        }

        public DateTime CreationDate { get; set; } = DateTime.Now;

        public DateTime? DeadLine
        {
            get
            {
                return _deadLine;
            }
            set
            {
                if (_deadLine == value)
                    return;

                if (value.HasValue)
                {

                    _deadLine = value.Value.Date + DateTime.Now.TimeOfDay;
                }
                else
                {
                    _deadLine = null;
                    
                }
                ValidateDate();
                OnPropertyChanged("DeadLine");
            }
        }


        private string _deadLineError;
        public string DeadLineError
        {
            get => _deadLineError;
            set
            {
                if (_deadLineError == value)
                    return;
                _deadLineError = value;

                ValidateDate();
                OnPropertyChanged("DeadLineError");
            }
        }


        public bool IsDone
        {
            get
            {
                return _isDone;
            }
            set
            {
                if (_isDone == value)
                    return;
                _isDone = value;
                ValidateDate();
                OnPropertyChanged("IsDone");
            }
        }

        public bool IsImportant
        {
            get
            {
                return _isImportant;
            }
            set
            {
                if (_isImportant == value)
                    return;

                _isImportant = value;
                OnPropertyChanged("IsImportant");
            }
        }

        public string Expenses
        {
            get
            {
                return _expenses;
            }
            set
            {
                if (_expenses == value)
                    return;
                _expenses = value;
                OnPropertyChanged("Expenses");
            }
        }

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if (_text == value)
                    return;
                _text = value;
                OnPropertyChanged("Text");
            }
        }


        public bool HasErrors => _errors.Any();

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;


        public IEnumerable GetErrors(string? propertyName)
        {
            return _errors.TryGetValue(propertyName ?? "", out var errors) ? errors : Enumerable.Empty<string>();
        }


        private void ValidateDate()
        {
            var propertyErrors = new List<string>();

            if (!string.IsNullOrEmpty(DeadLineError) && DeadLineError.Any(char.IsDigit) && !IsDone)
            {
                propertyErrors.Add($"'{DeadLineError}' is not a valid deadline date.");
            }



            else if (DeadLine.HasValue)
            {
                var daysLeft = (DeadLine.Value.Date - DateTime.Today).TotalDays;


                if (daysLeft <= 3 && daysLeft >= 0)
                {
                    IsImportant = true;
                }

                if (DeadLine.Value.Date <= DateTime.Today && !IsDone)
                    propertyErrors.Add("The deadline must be a future date.");

                if (DeadLine.Value.Date > DateTime.Today.AddYears(1) && !IsDone)
                    propertyErrors.Add("Plan ahead, but not too far. Deadlines must be within a year.");
            }



            if (propertyErrors.Any())
            {
                _errors[nameof(DeadLine)] = propertyErrors;
                _errors[nameof(DeadLineError)] = propertyErrors;
            }
            else
            {
                _errors.Remove(nameof(DeadLine));
                _errors.Remove(nameof(DeadLineError));
            }
            OnErrorsChanged(nameof(DeadLine));
            OnErrorsChanged(nameof(DeadLineError));
        }
        private void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }



        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}