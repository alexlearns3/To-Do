using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace WPF___Anw_
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int StundenHoehe = 80;
        private static readonly CultureInfo De = new("de-DE");
        private static readonly string[] TagKuerzel = { "MO", "DI", "MI", "DO", "FR", "SA", "SO" };
        private static readonly string[] Farben = { "#A8E6BE", "#B9D84C", "#A9C3FF", "#FFC58A", "#F5A9D0", "#D7B5FF" };
        private static readonly Brush Rot = Pinsel("#FF3B3B");
        private static readonly Brush Gruen = Pinsel("#B9D84C");

        // Vorerst nur im Speicher – die SQLite-Anbindung kommt später
        private readonly ObservableCollection<Person> _personen = new();
        private Person _aktuellePerson = null!;
        private DateOnly _datum = DateOnly.FromDateTime(DateTime.Today); // ausgewählter Tag
        private DateOnly _monat;                                          // Monat im Mini-Kalender

        public MainWindow()
        {
            InitializeComponent();
            PersonenListe.ItemsSource = _personen;
            _monat = new DateOnly(_datum.Year, _datum.Month, 1);
            RasterVorbereiten();

            // Beispieldaten aus dem Figma-Design (in der aktuellen Woche)
            DateOnly montag = Montag(_datum);
            Person mia = PersonAnlegen("Mia");
            Person tom = PersonAnlegen("Tom");
            TodoHinzufuegen(mia, new TodoItem("Essen gehen", montag.AddDays(1), new TimeOnly(17, 0)));
            TodoHinzufuegen(mia, new TodoItem("Mathe lernen", montag.AddDays(5), new TimeOnly(13, 0)));
            TodoHinzufuegen(tom, new TodoItem("laufen gehen", montag, new TimeOnly(10, 0)));
            TodoHinzufuegen(tom, new TodoItem("Deutsch lernen", montag.AddDays(3), new TimeOnly(11, 0)));

            PersonAuswaehlen(mia);
            Loaded += (_, _) => WochenScroll.ScrollToVerticalOffset(8 * StundenHoehe);
        }

        // ===== Personen =====

        private Person PersonAnlegen(string name)
        {
            var person = new Person(name, Pinsel(Farben[_personen.Count % Farben.Length]));
            _personen.Add(person);
            return person;
        }

        private void PersonAuswaehlen(Person person)
        {
            foreach (Person p in _personen)
                p.Ausgewaehlt = p == person;

            _aktuellePerson = person;
            TodoListe.ItemsSource = person.Todos;
            TodoTitel.Text = $"{person.Name} TO-DO:";
            Aktualisieren();
        }

        private void Person_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is Person person)
                PersonAuswaehlen(person);
        }

        private void PersonHinzufuegen_Click(object sender, RoutedEventArgs e)
        {
            string? name = NameAbfragen();
            if (!string.IsNullOrWhiteSpace(name))
                PersonAuswaehlen(PersonAnlegen(name));
        }

        // Kleines Fenster zum Eingeben des Namens
        private string? NameAbfragen()
        {
            var eingabe = new TextBox { Style = (Style)FindResource("DunklesEingabefeld"), Tag = "Name", Width = 260 };
            var ok = new Button
            {
                Content = "Hinzufügen",
                Style = (Style)FindResource("GruenerButton"),
                IsDefault = true,
                Margin = new Thickness(0, 12, 0, 0)
            };
            var fenster = new Window
            {
                Title = "Person hinzufügen",
                Owner = this,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = Pinsel("#333333"),
                Content = new StackPanel { Margin = new Thickness(20), Children = { eingabe, ok } }
            };
            ok.Click += (_, _) => fenster.DialogResult = true;
            fenster.Loaded += (_, _) => eingabe.Focus();

            return fenster.ShowDialog() == true ? eingabe.Text.Trim() : null;
        }

        // ===== To-Dos =====

        private void TodoHinzufuegen_Click(object sender, RoutedEventArgs e)
        {
            string titel = TitelEingabe.Text.Trim();
            if (titel == "")
            {
                TitelEingabe.Focus();
                return;
            }

            if (!TimeOnly.TryParseExact(UhrzeitEingabe.Text.Trim(), new[] { "H:mm", "HH:mm" },
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly uhrzeit))
            {
                MessageBox.Show("Bitte die Uhrzeit im Format HH:mm eingeben (z. B. 15:00).",
                    "Ungültige Uhrzeit", MessageBoxButton.OK, MessageBoxImage.Warning);
                UhrzeitEingabe.Focus();
                return;
            }

            TodoHinzufuegen(_aktuellePerson, new TodoItem(titel, _datum, uhrzeit));
            WocheAufbauen();

            TitelEingabe.Clear();
            UhrzeitEingabe.Clear();
            TitelEingabe.Focus();
        }

        // Fügt das Todo nach Datum sortiert ein
        private void TodoHinzufuegen(Person person, TodoItem neu)
        {
            neu.PropertyChanged += (_, _) => WocheAufbauen();

            int i = 0;
            while (i < person.Todos.Count && person.Todos[i].Zeitpunkt <= neu.Zeitpunkt)
                i++;
            person.Todos.Insert(i, neu);
        }

        private void Loeschen_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is TodoItem todo)
            {
                _aktuellePerson.Todos.Remove(todo);
                WocheAufbauen();
            }
        }

        // ===== Datum / Navigation =====

        private void DatumSetzen(DateOnly datum)
        {
            _datum = datum;
            _monat = new DateOnly(datum.Year, datum.Month, 1);
            Aktualisieren();
        }

        private void Aktualisieren()
        {
            MiniKalenderAufbauen();
            WocheAufbauen();
            NeuesDatumText.Text = $"Neues To-Do am {_datum.ToString("dd.MM.yyyy", De)}";
        }

        private void Tag_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is DateOnly datum)
                DatumSetzen(datum);
        }

        private void MonatZurueck_Click(object sender, RoutedEventArgs e)
        {
            _monat = _monat.AddMonths(-1);
            MiniKalenderAufbauen();
        }

        private void MonatVor_Click(object sender, RoutedEventArgs e)
        {
            _monat = _monat.AddMonths(1);
            MiniKalenderAufbauen();
        }

        private void WocheZurueck_Click(object sender, RoutedEventArgs e) => DatumSetzen(_datum.AddDays(-7));
        private void WocheVor_Click(object sender, RoutedEventArgs e) => DatumSetzen(_datum.AddDays(7));

        // ===== Mini-Kalender =====

        private void MiniKalenderAufbauen()
        {
            MiniMonatTitel.Text = _monat.ToString("MMMM yyyy", De).ToUpper(De);
            MiniKalender.Children.Clear();

            for (int i = 0; i < 7; i++)
            {
                MiniKalender.Children.Add(new TextBlock
                {
                    Text = TagKuerzel[i].Substring(0, 1),
                    Foreground = i == 6 ? Rot : Brushes.White,
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                });
            }

            // Leere Felder bis zum ersten Tag (Woche beginnt am Montag)
            int leer = ((int)_monat.DayOfWeek + 6) % 7;
            for (int i = 0; i < leer; i++)
                MiniKalender.Children.Add(new Border());

            DateOnly heute = DateOnly.FromDateTime(DateTime.Today);
            int tage = DateTime.DaysInMonth(_monat.Year, _monat.Month);
            for (int t = 1; t <= tage; t++)
            {
                var tag = new DateOnly(_monat.Year, _monat.Month, t);
                bool gewaehlt = tag == _datum;

                var knopf = new Button
                {
                    Content = t.ToString(),
                    Tag = tag,
                    Style = (Style)FindResource("TagButton"),
                    Background = gewaehlt ? Brushes.White : Brushes.Transparent,
                    Foreground = gewaehlt ? Brushes.Black
                               : tag.DayOfWeek == DayOfWeek.Sunday ? Rot
                               : tag == heute ? Gruen
                               : Brushes.White
                };
                knopf.Click += Tag_Click;
                MiniKalender.Children.Add(knopf);
            }
        }

        // ===== Wochenansicht =====

        private void RasterVorbereiten()
        {
            WochenRaster.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            for (int i = 0; i < 7; i++)
                WochenRaster.ColumnDefinitions.Add(new ColumnDefinition());
            for (int h = 0; h < 24; h++)
                WochenRaster.RowDefinitions.Add(new RowDefinition { Height = new GridLength(StundenHoehe) });
        }

        private void WocheAufbauen()
        {
            DateOnly montag = Montag(_datum);
            WochenTitel.Text = _datum.ToString("MMMM yyyy", De);

            // Tages-Köpfe (MO 12, DI 13, ...)
            TagesKoepfe.Children.Clear();
            for (int i = 0; i < 7; i++)
            {
                DateOnly tag = montag.AddDays(i);
                bool gewaehlt = tag == _datum;

                var inhalt = new StackPanel();
                inhalt.Children.Add(new TextBlock { Text = TagKuerzel[i], FontSize = 22, HorizontalAlignment = HorizontalAlignment.Center });
                inhalt.Children.Add(new TextBlock { Text = tag.Day.ToString(), FontSize = 24, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center });

                var knopf = new Button
                {
                    Content = inhalt,
                    Tag = tag,
                    Style = (Style)FindResource("TagKopf"),
                    Background = gewaehlt ? Pinsel("#222222") : Pinsel("#B5C792"),
                    Foreground = gewaehlt ? Brushes.White : Brushes.Black
                };
                knopf.Click += Tag_Click;
                TagesKoepfe.Children.Add(knopf);
            }

            WochenRaster.Children.Clear();

            // Ausgewählten Tag leicht hervorheben
            var markierung = new Border { Background = Pinsel("#22B9D84C") };
            Grid.SetColumn(markierung, _datum.DayNumber - montag.DayNumber + 1);
            Grid.SetRowSpan(markierung, 24);
            WochenRaster.Children.Add(markierung);

            // Uhrzeiten und Linien
            for (int h = 0; h < 24; h++)
            {
                var zeit = new TextBlock
                {
                    Text = $"{h}:00",
                    FontSize = 12,
                    Foreground = Pinsel("#999999"),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 4, 10, 0)
                };
                Grid.SetRow(zeit, h);
                WochenRaster.Children.Add(zeit);

                var linie = new Border { BorderBrush = Pinsel("#E8E8E8"), BorderThickness = new Thickness(0, 0, 0, 1) };
                Grid.SetRow(linie, h);
                Grid.SetColumn(linie, 1);
                Grid.SetColumnSpan(linie, 7);
                WochenRaster.Children.Add(linie);
            }
            for (int s = 1; s <= 7; s++)
            {
                var linie = new Border { BorderBrush = Pinsel("#E8E8E8"), BorderThickness = new Thickness(1, 0, 0, 0) };
                Grid.SetColumn(linie, s);
                Grid.SetRowSpan(linie, 24);
                WochenRaster.Children.Add(linie);
            }

            // To-Dos aller Personen als farbige Blöcke
            foreach (Person person in _personen)
            {
                foreach (TodoItem todo in person.Todos)
                {
                    int tag = todo.Datum.DayNumber - montag.DayNumber;
                    if (tag < 0 || tag > 6) continue;

                    var text = new TextBlock { FontSize = 13, Foreground = Pinsel("#1A1A1A"), TextWrapping = TextWrapping.Wrap };
                    text.Inlines.Add(new Run(person.Name + ":") { FontWeight = FontWeights.Bold });
                    text.Inlines.Add(new LineBreak());
                    text.Inlines.Add(new Run(todo.Titel));

                    var block = new Border
                    {
                        Background = person.Farbe,
                        CornerRadius = new CornerRadius(14),
                        Padding = new Thickness(10, 6, 10, 6),
                        Height = StundenHoehe - 10,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(4, todo.Uhrzeit.Minute * StundenHoehe / 60.0 + 5, 4, 0),
                        Opacity = todo.Erledigt ? 0.45 : 1,
                        ToolTip = $"{todo.UhrzeitText} – {todo.Titel}",
                        Child = text
                    };
                    Grid.SetRow(block, todo.Uhrzeit.Hour);
                    Grid.SetColumn(block, tag + 1);
                    Grid.SetRowSpan(block, 2);
                    Panel.SetZIndex(block, 1);
                    WochenRaster.Children.Add(block);
                }
            }
        }

        // ===== Hilfsmethoden =====

        private static DateOnly Montag(DateOnly datum) => datum.AddDays(-(((int)datum.DayOfWeek + 6) % 7));

        private static SolidColorBrush Pinsel(string hex) => new((Color)ColorConverter.ConvertFromString(hex));
    }
}
