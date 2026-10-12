using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace WPF___Anw_
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Vorerst nur im Speicher – die SQLite-Anbindung kommt später
        private readonly ObservableCollection<TodoItem> _todos = new();

        public MainWindow()
        {
            InitializeComponent();
            TodoListe.ItemsSource = _todos;
            DatumAuswahl.SelectedDate = DateTime.Today;

            // Beispieldaten aus dem Figma-Design
            Hinzufuegen(new TodoItem("Essen gehen", new DateOnly(2026, 10, 12), new TimeOnly(15, 0)));
            Hinzufuegen(new TodoItem("Kerzen kaufen", new DateOnly(2026, 10, 14), new TimeOnly(19, 0)));
        }

        private void AddTodo_Click(object sender, RoutedEventArgs e)
        {
            string titel = TitelEingabe.Text.Trim();
            if (titel == "")
            {
                TitelEingabe.Focus();
                return;
            }

            TimeOnly? uhrzeit = null;
            string uhrzeitText = UhrzeitEingabe.Text.Trim();
            if (uhrzeitText != "")
            {
                if (!TimeOnly.TryParseExact(uhrzeitText, new[] { "H:mm", "HH:mm" },
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly zeit))
                {
                    MessageBox.Show("Bitte die Uhrzeit im Format HH:mm eingeben (z. B. 15:00).",
                        "Ungültige Uhrzeit", MessageBoxButton.OK, MessageBoxImage.Warning);
                    UhrzeitEingabe.Focus();
                    return;
                }
                uhrzeit = zeit;
            }

            DateOnly datum = DateOnly.FromDateTime(DatumAuswahl.SelectedDate ?? DateTime.Today);
            Hinzufuegen(new TodoItem(titel, datum, uhrzeit));

            TitelEingabe.Clear();
            UhrzeitEingabe.Clear();
            TitelEingabe.Focus();
        }

        // Fügt das Todo nach Datum sortiert ein
        private void Hinzufuegen(TodoItem neu)
        {
            int i = 0;
            while (i < _todos.Count && _todos[i].Zeitpunkt <= neu.Zeitpunkt)
                i++;
            _todos.Insert(i, neu);
        }

        // "•••" öffnet das Menü auch per Linksklick
        private void MehrButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.IsOpen = true;
        }

        private void Loeschen_Click(object sender, RoutedEventArgs e)
        {
            if (((MenuItem)sender).DataContext is TodoItem todo)
                _todos.Remove(todo);
        }
    }
}
