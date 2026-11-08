using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.Sqlite;

namespace To_Do_App
{
    public partial class MainWindow : Window
    {
        // Die Datenbank-Datei liegt neben der .exe und wird beim ersten Start erstellt
        private static string connectionString = "Data Source=todo.db";

        // Wird vom Anmelde-Fenster mit dem Namen der angemeldeten Person geöffnet
        public MainWindow(string angemeldetePerson)
        {
            InitializeComponent();

            Title = "To-Do-App - " + angemeldetePerson;
            dpDatum.SelectedDate = DateTime.Today;
            PersonenLaden(angemeldetePerson);
        }

        // Personen aus der Datenbank in die Auswahl laden und die angemeldete Person wählen
        private void PersonenLaden(string angemeldetePerson)
        {
            cbPerson.Items.Add(new ComboBoxItem { Content = "Gesamt" });
            ComboBoxItem? gewaehlt = null;

            using SqliteConnection connection = VerbindungOeffnen();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Name FROM User ORDER BY ID";

            using SqliteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                ComboBoxItem item = new ComboBoxItem { Content = reader.GetString(0) };
                cbPerson.Items.Add(item);

                if (reader.GetString(0) == angemeldetePerson)
                {
                    gewaehlt = item;
                }
            }

            cbPerson.SelectedItem = gewaehlt;
        }

        // ---------- Datenbank ----------

        public static SqliteConnection VerbindungOeffnen()
        {
            SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();
            return connection;
        }

        // Tabellen User und Todo erstellen (wie im drawio-Diagramm)
        public static void DatenbankErstellen()
        {
            using SqliteConnection connection = VerbindungOeffnen();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText =
                "CREATE TABLE IF NOT EXISTS User (" +
                "  ID INTEGER PRIMARY KEY AUTOINCREMENT," +
                "  Name TEXT NOT NULL," +
                "  Passwort TEXT NOT NULL);" +
                "CREATE TABLE IF NOT EXISTS Todo (" +
                "  TodoId INTEGER PRIMARY KEY AUTOINCREMENT," +
                "  Datum TEXT NOT NULL," +
                "  Beschreibung TEXT NOT NULL," +
                "  Creat INTEGER NOT NULL REFERENCES User(ID));";
            cmd.ExecuteNonQuery();

            // Spalte "Erledigt" nachtragen (bei einer alten todo.db fehlt sie)
            try
            {
                cmd.CommandText = "ALTER TABLE Todo ADD COLUMN Erledigt INTEGER NOT NULL DEFAULT 0";
                cmd.ExecuteNonQuery();
            }
            catch (SqliteException)
            {
                // Spalte gibt es schon
            }

            // Personen nur einfügen, wenn die Tabelle User noch leer ist
            cmd.CommandText = "SELECT COUNT(*) FROM User";
            long anzahl = (long)cmd.ExecuteScalar()!;

            if (anzahl == 0)
            {
                cmd.CommandText =
                    "INSERT INTO User (Name, Passwort) VALUES " +
                    "('Mia', '1234'), ('Liana', '1234'), ('Alex', '1234'), ('Vogli', '1234')";
                cmd.ExecuteNonQuery();
            }
        }

        // To-Dos aus der Datenbank in die Liste laden
        private void TodosLaden()
        {
            lstTodos.Items.Clear();

            using SqliteConnection connection = VerbindungOeffnen();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText =
                "SELECT Todo.TodoId, Todo.Datum, Todo.Beschreibung, User.Name, Todo.Erledigt " +
                "FROM Todo JOIN User ON Todo.Creat = User.ID";

            // Bei "Gesamt" alle anzeigen, sonst nur die To-Dos der Person
            if (GewaehltePerson() != "Gesamt")
            {
                cmd.CommandText += " WHERE User.Name = $name";
                cmd.Parameters.AddWithValue("$name", GewaehltePerson());
            }
            cmd.CommandText += " ORDER BY Todo.Datum";

            using SqliteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Todo todo = new Todo(reader.GetInt64(0), reader.GetString(1),
                                     reader.GetString(2), reader.GetString(3));
                todo.Erledigt = reader.GetInt64(4) == 1;
                lstTodos.Items.Add(todo);
            }
        }

        // ---------- Hilfsmethoden ----------

        private string GewaehltePerson()
        {
            ComboBoxItem item = (ComboBoxItem)cbPerson.SelectedItem;
            return item.Content.ToString()!;
        }

        // Prüft die Eingaben. Gibt true zurück und liefert das Datum als Text "JJJJ-MM-TT HH:MM"
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

            datum = dpDatum.SelectedDate.Value.ToString("yyyy-MM-dd") + " " + uhrzeit.ToString(@"hh\:mm");
            return true;
        }

        private void FelderLeeren()
        {
            tbBeschreibung.Text = "";
            dpDatum.SelectedDate = DateTime.Today;
            tbUhrzeit.Text = "12:00";
        }

        // ---------- Ereignisse ----------

        private void cbPerson_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TodosLaden();
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

        private void btnHinzufuegen_Click(object sender, RoutedEventArgs e)
        {
            if (GewaehltePerson() == "Gesamt")
            {
                MessageBox.Show("Bitte oben eine Person auswählen.");
                return;
            }
            if (!EingabePruefen(out string datum))
            {
                return;
            }

            using SqliteConnection connection = VerbindungOeffnen();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText =
                "INSERT INTO Todo (Datum, Beschreibung, Creat) " +
                "VALUES ($datum, $beschreibung, (SELECT ID FROM User WHERE Name = $name))";
            cmd.Parameters.AddWithValue("$datum", datum);
            cmd.Parameters.AddWithValue("$beschreibung", tbBeschreibung.Text.Trim());
            cmd.Parameters.AddWithValue("$name", GewaehltePerson());
            cmd.ExecuteNonQuery();

            FelderLeeren();
            TodosLaden();
        }

        private void btnAendern_Click(object sender, RoutedEventArgs e)
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

            using SqliteConnection connection = VerbindungOeffnen();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE Todo SET Datum = $datum, Beschreibung = $beschreibung WHERE TodoId = $id";
            cmd.Parameters.AddWithValue("$datum", datum);
            cmd.Parameters.AddWithValue("$beschreibung", tbBeschreibung.Text.Trim());
            cmd.Parameters.AddWithValue("$id", todo.Id);
            cmd.ExecuteNonQuery();

            FelderLeeren();
            TodosLaden();
        }

        // Häkchen in der Liste angeklickt
        private void chkErledigt_Click(object sender, RoutedEventArgs e)
        {
            CheckBox haekchen = (CheckBox)sender;
            ErledigtUmschalten((Todo)haekchen.DataContext);
        }

        private void ErledigtUmschalten(Todo todo)
        {
            using SqliteConnection connection = VerbindungOeffnen();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE Todo SET Erledigt = 1 - Erledigt WHERE TodoId = $id";
            cmd.Parameters.AddWithValue("$id", todo.Id);
            cmd.ExecuteNonQuery();

            TodosLaden();
        }

        private void btnLoeschen_Click(object sender, RoutedEventArgs e)
        {
            if (lstTodos.SelectedItem == null)
            {
                MessageBox.Show("Bitte zuerst ein To-Do in der Liste auswählen.");
                return;
            }

            Todo todo = (Todo)lstTodos.SelectedItem;

            using SqliteConnection connection = VerbindungOeffnen();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Todo WHERE TodoId = $id";
            cmd.Parameters.AddWithValue("$id", todo.Id);
            cmd.ExecuteNonQuery();

            FelderLeeren();
            TodosLaden();
        }
    }
}
