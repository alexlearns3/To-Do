using System.Windows;
using System.Windows.Controls;

namespace To_Do_App
{
    public partial class MainWindow : Window
    {
        private readonly long angemeldeteId;
        private bool laedt;

        // Wird vom Anmelde-Fenster mit der ID und dem Namen der angemeldeten Person geöffnet
        public MainWindow(long userId, string name)
        {
            InitializeComponent();

            angemeldeteId = userId;
            Title = "To-Do-App - " + name;
            dpDatum.SelectedDate = DateTime.Today;
            Loaded += async (s, e) => await Sicher(PersonenLadenAsync);
        }

        // Führt eine Datenbank-Aktion aus, sperrt solange das Fenster und zeigt Fehler an
        private async Task Sicher(Func<Task> aktion)
        {
            IsEnabled = false;
            try
            {
                await aktion();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Datenbankfehler:\n" + ex.Message);
            }
            finally
            {
                IsEnabled = true;
            }
        }

        // ---------- Datenbank ----------

        // Personen aus der Datenbank in die Auswahl laden und die angemeldete Person wählen
        private async Task PersonenLadenAsync()
        {
            laedt = true;
            try
            {
                cbPerson.Items.Clear();
                ComboBoxItem gesamt = new ComboBoxItem { Content = "Gesamt", Tag = null };
                cbPerson.Items.Add(gesamt);
                ComboBoxItem? gewaehlt = null;

                var zeilen = await Datenbank.AbfrageAsync("SELECT id, username FROM users ORDER BY id");
                foreach (var z in zeilen)
                {
                    long id = (long)z["id"]!;
                    ComboBoxItem item = new ComboBoxItem { Content = Convert.ToString(z["username"]), Tag = id };
                    cbPerson.Items.Add(item);

                    if (id == angemeldeteId)
                    {
                        gewaehlt = item;
                    }
                }

                cbPerson.SelectedItem = gewaehlt ?? gesamt;
            }
            finally
            {
                laedt = false;
            }

            await TodosLadenAsync();
        }

        // To-Dos aus der Datenbank in die Liste laden
        private async Task TodosLadenAsync()
        {
            string sql =
                "SELECT todos.id, todos.date, todos.description, users.username, todos.done " +
                "FROM todos JOIN users ON todos.user_id = users.id";

            // Bei "Gesamt" alle anzeigen, sonst nur die To-Dos der Person
            long? userId = GewaehlteId();
            var zeilen = userId == null
                ? await Datenbank.AbfrageAsync(sql + " ORDER BY todos.date")
                : await Datenbank.AbfrageAsync(sql + " WHERE todos.user_id = ? ORDER BY todos.date", userId.Value);

            lstTodos.Items.Clear();
            foreach (var z in zeilen)
            {
                Todo todo = new Todo((long)z["id"]!,
                                     Convert.ToString(z["date"]) ?? "",
                                     Convert.ToString(z["description"]) ?? "",
                                     Convert.ToString(z["username"]) ?? "");
                todo.Erledigt = Convert.ToInt64(z["done"] ?? 0L) == 1;
                lstTodos.Items.Add(todo);
            }
        }

        // ---------- Hilfsmethoden ----------

        // ID der oben gewählten Person, oder null bei "Gesamt"
        private long? GewaehlteId()
        {
            return (cbPerson.SelectedItem as ComboBoxItem)?.Tag as long?;
        }

        // Prüft die Eingaben. Gibt true zurück und liefert das Datum als Text "JJJJ-MM-TT HH:MM:SS"
        private bool EingabePruefen(out string datum)
        {
            datum = "";

            if (tbBeschreibung.Text.Trim() == "")
            {
                MessageBox.Show("Bitte eine Beschreibung eingeben.");
                return false;
            }
            if (dpDatum.SelectedDate == null)
            {
                MessageBox.Show("Bitte ein Datum auswählen.");
                return false;
            }
            if (!TimeSpan.TryParse(tbUhrzeit.Text, out TimeSpan uhrzeit) || uhrzeit.TotalHours >= 24)
            {
                MessageBox.Show("Bitte die Uhrzeit so eingeben: HH:MM (zum Beispiel 14:30).");
                return false;
            }

            // gleiches Format wie die Web-App
            datum = dpDatum.SelectedDate.Value.ToString("yyyy-MM-dd") + " " + uhrzeit.ToString(@"hh\:mm") + ":00";
            return true;
        }

        private void FelderLeeren()
        {
            tbBeschreibung.Text = "";
            dpDatum.SelectedDate = DateTime.Today;
            tbUhrzeit.Text = "12:00";
        }

        // ---------- Ereignisse ----------

        private async void cbPerson_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (laedt)
            {
                return;
            }
            await Sicher(TodosLadenAsync);
        }

        // Ein To-Do in der Liste angeklickt -> Werte in die Eingabefelder schreiben
        private void lstTodos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstTodos.SelectedItem == null)
            {
                return;
            }

            Todo todo = (Todo)lstTodos.SelectedItem;
            tbBeschreibung.Text = todo.Beschreibung;

            if (DateTime.TryParse(todo.Datum, out DateTime zeit))
            {
                dpDatum.SelectedDate = zeit.Date;
                tbUhrzeit.Text = zeit.ToString("HH:mm");
            }
        }

        private async void btnHinzufuegen_Click(object sender, RoutedEventArgs e)
        {
            long? userId = GewaehlteId();
            if (userId == null)
            {
                MessageBox.Show("Bitte oben eine Person auswählen.");
                return;
            }
            if (!EingabePruefen(out string datum))
            {
                return;
            }
            string beschreibung = tbBeschreibung.Text.Trim();

            await Sicher(async () =>
            {
                await Datenbank.AbfrageAsync(
                    "INSERT INTO todos (user_id, description, date) VALUES (?, ?, ?)",
                    userId.Value, beschreibung, datum);
                FelderLeeren();
                await TodosLadenAsync();
            });
        }

        private async void btnAendern_Click(object sender, RoutedEventArgs e)
        {
            if (lstTodos.SelectedItem == null)
            {
                MessageBox.Show("Bitte zuerst ein To-Do in der Liste auswählen.");
                return;
            }
            if (!EingabePruefen(out string datum))
            {
                return;
            }

            Todo todo = (Todo)lstTodos.SelectedItem;
            string beschreibung = tbBeschreibung.Text.Trim();

            await Sicher(async () =>
            {
                await Datenbank.AbfrageAsync(
                    "UPDATE todos SET date = ?, description = ? WHERE id = ?",
                    datum, beschreibung, todo.Id);
                FelderLeeren();
                await TodosLadenAsync();
            });
        }

        // Häkchen in der Liste angeklickt
        private async void chkErledigt_Click(object sender, RoutedEventArgs e)
        {
            CheckBox haekchen = (CheckBox)sender;
            Todo todo = (Todo)haekchen.DataContext;

            await Sicher(async () =>
            {
                await Datenbank.AbfrageAsync("UPDATE todos SET done = 1 - done WHERE id = ?", todo.Id);
                await TodosLadenAsync();
            });
        }

        private async void btnLoeschen_Click(object sender, RoutedEventArgs e)
        {
            if (lstTodos.SelectedItem == null)
            {
                MessageBox.Show("Bitte zuerst ein To-Do in der Liste auswählen.");
                return;
            }

            Todo todo = (Todo)lstTodos.SelectedItem;

            await Sicher(async () =>
            {
                await Datenbank.AbfrageAsync("DELETE FROM todos WHERE id = ?", todo.Id);
                FelderLeeren();
                await TodosLadenAsync();
            });
        }
    }
}
