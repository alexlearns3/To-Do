using System.ComponentModel;

namespace WPF___Anw_
{
    /// <summary>
    /// Ein einzelnes Todo. Wird vorerst nur im Speicher gehalten (SQLite kommt später).
    /// </summary>
    public class TodoItem : INotifyPropertyChanged
    {
        private bool _erledigt;

        public TodoItem(string titel, DateOnly datum, TimeOnly uhrzeit)
        {
            Titel = titel;
            Datum = datum;
            Uhrzeit = uhrzeit;
        }

        public string Titel { get; }
        public DateOnly Datum { get; }
        public TimeOnly Uhrzeit { get; }

        public bool Erledigt
        {
            get => _erledigt;
            set
            {
                if (_erledigt == value) return;
                _erledigt = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Erledigt)));
            }
        }

        // Zum Sortieren der Liste
        public DateTime Zeitpunkt => Datum.ToDateTime(Uhrzeit);

        // Anzeige-Texte für die Oberfläche
        public string DatumText => Datum.ToString("dd.MM.yyyy");
        public string UhrzeitText => $"{Uhrzeit:HH:mm} Uhr";

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
