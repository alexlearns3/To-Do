using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;

namespace WPF___Anw_
{
    /// <summary>
    /// Eine Person mit eigener Farbe und eigener To-Do-Liste (vorerst nur im Speicher).
    /// </summary>
    public class Person : INotifyPropertyChanged
    {
        private bool _ausgewaehlt;

        public Person(string name, Brush farbe)
        {
            Name = name;
            Farbe = farbe;
        }

        public string Name { get; }
        public Brush Farbe { get; }
        public string Initiale => Name.Substring(0, 1).ToUpper();
        public ObservableCollection<TodoItem> Todos { get; } = new();

        // Steuert den roten Ring in der Personen-Leiste
        public bool Ausgewaehlt
        {
            get => _ausgewaehlt;
            set
            {
                if (_ausgewaehlt == value) return;
                _ausgewaehlt = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Ausgewaehlt)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
