using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.Sqlite;

namespace To_Do_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Die Datenbank-Datei liegt neben der .exe und wird beim ersten Start erstellt
        private string connectionString = "Data Source=todo.db";

        public MainWindow()
        {
            InitializeComponent();

            DatenbankErstellen();

            // "Gesamt" auswählen -> lädt automatisch alle To-Dos
            lstPersonen.SelectedIndex = 0;
        }

        // Tabellen erstellen (wie im drawio-Diagramm) und die 4 Personen einfügen
        private void DatenbankErstellen()
        {
            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

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

        // To-Dos aus der Datenbank in die ListBox laden
        private void TodosLaden()
        {
            lstTodos.Items.Clear();

            string person = GewaehltePerson();

            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText =
                "SELECT Todo.TodoId, Todo.Datum, Todo.Beschreibung, User.Name " +
                "FROM Todo JOIN User ON Todo.Creat = User.ID";

            // Bei "Gesamt" alle anzeigen, sonst nur die To-Dos von der Person
            if (person != "Gesamt")
            {
                cmd.CommandText += " WHERE User.Name = $name";
                cmd.Parameters.AddWithValue("$name", person);
            }

            using SqliteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Todo todo = new Todo(reader.GetInt64(0), reader.GetString(1),
                                     reader.GetString(2), reader.GetString(3));
                lstTodos.Items.Add(todo);
            }
        }

        // Liefert den Namen der links ausgewählten Person
        private string GewaehltePerson()
        {
            ListBoxItem item = (ListBoxItem)lstPersonen.SelectedItem;
            return item.Content.ToString()!;
        }

        // Andere Person links ausgewählt -> Liste neu laden
        private void lstPersonen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TodosLaden();
        }

        private void btnHinzufuegen_Click(object sender, RoutedEventArgs e)
        {
            string person = GewaehltePerson();

            // Eingaben prüfen
            if (person == "Gesamt")
            {
                MessageBox.Show("Bitte links eine Person auswählen.");
                return;
            }
            if (tbBeschreibung.Text == "" || tbDatum.Text == "")
            {
                MessageBox.Show("Bitte Beschreibung und Datum eingeben.");
                return;
            }

            // Datum und Uhrzeit zusammen in die Spalte "Datum" speichern
            string datum = tbDatum.Text;
            if (tbUhrzeit.Text != "")
            {
                datum = datum + " " + tbUhrzeit.Text;
            }

            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText =
                "INSERT INTO Todo (Datum, Beschreibung, Creat) " +
                "VALUES ($datum, $beschreibung, (SELECT ID FROM User WHERE Name = $name))";
            cmd.Parameters.AddWithValue("$datum", datum);
            cmd.Parameters.AddWithValue("$beschreibung", tbBeschreibung.Text);
            cmd.Parameters.AddWithValue("$name", person);
            cmd.ExecuteNonQuery();

            // Eingabefelder leeren und Liste neu laden
            tbBeschreibung.Text = "";
            tbDatum.Text = "";
            tbUhrzeit.Text = "";
            TodosLaden();
        }

        private void btnLoeschen_Click(object sender, RoutedEventArgs e)
        {
            if (lstTodos.SelectedItem == null)
            {
                MessageBox.Show("Bitte zuerst ein To-Do auswählen.");
                return;
            }

            Todo todo = (Todo)lstTodos.SelectedItem;

            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Todo WHERE TodoId = $id";
            cmd.Parameters.AddWithValue("$id", todo.Id);
            cmd.ExecuteNonQuery();

            TodosLaden();
        }
    }
}
