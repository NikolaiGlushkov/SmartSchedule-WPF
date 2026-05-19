using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SmartSchedule.Model;
using SmartSchedule.Services;
using SmartSchedule.ViewModel;

namespace SmartSchedule
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    // 1.1: create MainViewModel for tab collection for clear MVVM architecture.
    public partial class MainWindow : Window
    {

        private ObservableCollection<TCViewModel> _tCDataList;

        private readonly string _path = $"{Environment.CurrentDirectory}\\tCDataList.json";

        private FileIOService _fileIOService;


        public ObservableCollection<TCViewModel> TCDataList
        {
            get
            {
                return _tCDataList;
            }
            private set
            {
                _tCDataList = value;
            }
        }


        public MainWindow()
        {
            InitializeComponent();

            this.Top = Properties.Settings.Default.WindowTop;
            this.Left = Properties.Settings.Default.WindowLeft;
            this.Width = Properties.Settings.Default.WindowWidth;
            this.Height = Properties.Settings.Default.WindowHeight;
            this.WindowState = Properties.Settings.Default.WindowState;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this;

            tabContr.SelectedIndex = Properties.Settings.Default.LastTabIndex;

            _fileIOService = new FileIOService(_path);

            try
            {
                TCDataList = _fileIOService.LoadData();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Close();
            }

            if (TCDataList.Count == 0)
            {
                TCDataList = new ObservableCollection<TCViewModel> { new TCViewModel() };
            }

            TCDataList.CollectionChanged += TCDataList_CollectionChanged;

            tabContr.SourceUpdated += TabContr_SourceUpdated;
        }

        // TabControl--------------------------------------------------------------------------------------

        private void TabContr_SourceUpdated(object? sender, DataTransferEventArgs e)
        {
            try
            {
                _fileIOService.SaveData(TCDataList);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Close();
            }
        }

        private void TCDataList_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (TCDataList.Count == 0)
            {
                Application.Current.Shutdown();
            }

        }

        private void AddTabButton_Click(object sender, RoutedEventArgs e)
        {
            const int MAXQUANTITYOFTABS = 7;
            if (TCDataList.Count <= MAXQUANTITYOFTABS)
            {
                TCDataList.Add(new TCViewModel());
            }
            else
            {
                MessageBox.Show("You have reached the tab limit. Finish up some of your tasks first.");
            }
        }

        private void CloseTabButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;

            var tabData = button.Tag;
            if (tabData == null)
                return;

            MessageBoxResult deleteConfirmation = MessageBox.Show("Do you want to delete this tab?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (deleteConfirmation == MessageBoxResult.Yes)
            {
                TCDataList.Remove(tabData as TCViewModel);
            }
        }

        private void HeaderSendFocusToGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            (sender as FrameworkElement)?.Focus();
        }

        private void HeaderChange_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var panel = sender as StackPanel;
                if (panel == null)
                    return;

                var textBlock = panel.Children.OfType<TextBlock>().FirstOrDefault();
                var textBox = panel.Children.OfType<TextBox>().FirstOrDefault();
                if (textBox == null)
                    return;

                textBox.Tag = textBox.Text;

                if (textBlock != null && textBox != null)
                {
                    textBlock.Visibility = Visibility.Collapsed;
                    textBox.Visibility = Visibility.Visible;

                    textBox.Focus();
                    textBox.SelectAll();

                    e.Handled = true;
                }
            }
        }

        // 1.1 get rid of repeating code
        private void HeaderApprovingOrCancelling_KeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null)
                return;

            var panel = textBox.Parent as StackPanel;
            if (panel == null)
                return;

            var textBlock = panel.Children.OfType<TextBlock>().FirstOrDefault();
            if (textBlock == null)
                return;

            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = textBox.Tag.ToString();
                }
                textBox.Visibility = Visibility.Collapsed;
                textBlock.Visibility = Visibility.Visible;

                textBlock.Focus();

                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                textBox.Text = textBox.Tag.ToString();

                textBox.Visibility = Visibility.Collapsed;
                textBlock.Visibility = Visibility.Visible;

                textBlock.Focus();

                e.Handled = true;
            }
        }

        private void HeaderApproving_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                var panel = textBox.Parent as StackPanel;
                if (panel != null)
                {
                    var textBlock = panel.Children.OfType<TextBlock>().FirstOrDefault();
                    if (textBlock == null)
                        return;

                    if (string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        textBox.Text = textBox.Tag.ToString();
                    }
                    textBox.Visibility = Visibility.Collapsed;
                    textBlock.Visibility = Visibility.Visible;

                    e.Handled = true;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (TCDataList.Count > 0)
            {
                Properties.Settings.Default.LastTabIndex = tabContr.SelectedIndex;
            }
            else
            {
                Properties.Settings.Default.LastTabIndex = 0;
            }

            if (this.WindowState == WindowState.Maximized)
            {
                Properties.Settings.Default.WindowState = WindowState.Maximized;
            }
            else if (this.WindowState == WindowState.Minimized)
            {
                Properties.Settings.Default.WindowState = WindowState.Normal;
            }
            else
            {
                Properties.Settings.Default.WindowTop = this.Top;
                Properties.Settings.Default.WindowLeft = this.Left;
                Properties.Settings.Default.WindowWidth = this.Width;
                Properties.Settings.Default.WindowHeight = this.Height;
                Properties.Settings.Default.WindowState = this.WindowState;
            }
            Properties.Settings.Default.Save();

            try
            {
                _fileIOService.SaveData(TCDataList);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Close();
            }
        }

        // DataGrid--------------------------------------------------------------------------------------

        private void dgSmSchedule_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }


        private void RowHeaderUpdate_UnloadingRow(object sender, DataGridRowEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            dataGrid?.Items.Refresh();

            try
            {
                _fileIOService.SaveData(TCDataList);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Close();
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;

            if (checkBox?.IsChecked == true)
            {
                var item = checkBox.DataContext as DGModel;

                var viewModel = tabContr.SelectedItem as TCViewModel;

                viewModel?.DeleteRowCommand.Execute(item);
            }
        }

        private void SolutionPlaceholderBugAndTextTransfer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                return;
            
            var grid = sender as DataGrid;

            if (grid != null)
            {
                grid.CommitEdit(DataGridEditingUnit.Cell, true);
                grid.CommitEdit(DataGridEditingUnit.Row, true);

                grid.Focus();

                e.Handled = true;
            }
        }

        // DataPicker--------------------------------------------------------------------------------------

        private void DatePicker_CalendarClosed(object sender, RoutedEventArgs e)
        {
            (sender as DatePicker)?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        // 1.1 get rid of repeating code
        private void DatePickerTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }

            var textBox = sender as TextBox;
            if (textBox == null) return;

            string sep = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator;

            int start = textBox.SelectionStart;
            int length = textBox.SelectionLength;

            if (e.Key == Key.Back)
            {
                e.Handled = true;

                if (length > 0)
                {
                    StringBuilder sb = new StringBuilder(textBox.Text);
                    for (int i = start; i < start + length; i++)
                    {
                        if (sb[i].ToString() != sep)
                        {
                            sb[i] = '_';
                        }
                    }
                    textBox.Text = sb.ToString();
                    textBox.SelectionStart = start;
                }

                else if (start > 0)
                {
                    int targetIndex = start - 1;

                    if (textBox.Text[targetIndex].ToString() == sep)
                    {
                        targetIndex--;
                    }

                    if (targetIndex >= 0)
                    {
                        textBox.Text = textBox.Text.Remove(targetIndex, 1).Insert(targetIndex, "_");
                        textBox.SelectionStart = targetIndex;
                    }
                }

            }

            if (e.Key == Key.Delete)
            {
                e.Handled = true;

                if (length > 0)
                {
                    StringBuilder sb = new StringBuilder(textBox.Text);
                    for (int i = start; i < start + length; i++)
                    {
                        if (sb[i].ToString() != sep)
                        {
                            sb[i] = '_';
                        }
                    }
                    textBox.Text = sb.ToString();
                    textBox.SelectionStart = start;
                }

                else if (start < textBox.Text.Length)
                {
                    int targetIndex = start;
                    if (textBox.Text[targetIndex].ToString() == sep) targetIndex++;

                    if (targetIndex < textBox.Text.Length)
                    {
                        textBox.Text = textBox.Text.Remove(targetIndex, 1).Insert(targetIndex, "_");
                        textBox.SelectionStart = targetIndex;
                    }
                }

            }
        }

        private void DatePickerTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
                return;
            }

            var textBox = sender as TextBox;

            if (textBox == null)
                return;

            string sep = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator;

            if (string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = $"__{sep}__{sep}____";

            }

            int index = textBox.SelectionStart;

            if (index >= 10)
            {
                e.Handled = true;
                return;
            }


            if (textBox.Text[index].ToString() == sep)
            {
                index++;
                textBox.SelectionStart = index;
            }

            string currentText = textBox.Text;
            string newText = currentText.Remove(index, 1).Insert(index, e.Text);

            textBox.Text = newText;
            textBox.SelectionStart = index + 1;

            e.Handled = true;

        }

        private void DatePicker_DateValidationError(object sender, DatePickerDateValidationErrorEventArgs e)
        {
            e.ThrowException = false;

            var dp = sender as DatePicker;
            if (dp == null)
                return;

            dp.Tag = dp.Text;

            var textBox = dp.Template.FindName("PART_TextBox", dp) as System.Windows.Controls.Primitives.DatePickerTextBox;

            if (dp?.DataContext is DGModel model)
            {
                dp.SelectedDate = null;
                model.DeadLineError = e.Text;

            }
        }

        private void DatePicker_Loaded(object sender, RoutedEventArgs e)
        {
            var datePicker = sender as DatePicker;

            if (datePicker == null)
            {
                return;
            }

            var textBox = datePicker.Template.FindName("PART_TextBox", datePicker) as DatePickerTextBox;

            if (textBox == null)
            {
                return;
            }

            textBox.MouseEnter += (s, e) => VisualStateManager.GoToState(textBox, "Normal", false);

            textBox.RequestBringIntoView += (s, e) =>
            {
                if (textBox.IsMouseOver || textBox.IsFocused)
                {
                    VisualStateManager.GoToState(textBox, "Normal", false);
                }
            };

            textBox.Focus();

            var watermarkContent = textBox.Template.FindName("PART_Watermark", textBox) as ContentControl;
            if (watermarkContent != null)
            {
                watermarkContent.Content = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
                watermarkContent.FontSize = 14;
            }

            if (datePicker.DataContext is DGModel model && !string.IsNullOrEmpty(model.DeadLineError))
            {
                textBox.Text = model.DeadLineError;

                model.DeadLineError = string.Empty;

                textBox.SelectAll();
            }
        }

        private void DatePicker_PreviewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
            {
                e.CanExecute = false;
                e.Handled = true;
            }
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            var dp = sender as DatePicker;

            if (dp?.DataContext is DGModel model)
            {

                if (model.DeadLine != null)
                {
                    model.DeadLineError = string.Empty;
                }
            }
        }
    }
}

