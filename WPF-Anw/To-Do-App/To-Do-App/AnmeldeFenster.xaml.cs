using System.Windows;
using Microsoft.Data.Sqlite;

namespace To_Do_App
{
    // Erstes Fenster: anmelden oder neu registrieren
    public partial class AnmeldeFenster : Window
    {
        public AnmeldeFenster()
        {
            InitializeComponent();
            MainWindow.DatenbankErstellen();
        }

        // Prüft, ob Name und Passwort ausgefüllt sind
        private bool EingabePruefen()
        {
            if (tbName.Text.Trim() == "" || pbPasswort.Password == "")
            {
                MessageBox.Show("Bitte Name und Passwort eingeben.");
                return false;
            }
            return true;
        }

        // Hauptfenster für die Person öffnen und dieses Fenster schließen
        private void HauptfensterOeffnen()
        {
            MainWindow fenster = new MainWindow(tbName.Text.Trim());
            fenster.Show();
            Close();
        }

        private void btnAnmelden_Click(object sender, RoutedEventArgs e)
        {
            if (!EingabePruefen())
            {
                return;
            }

            using SqliteConnection connection = MainWindow.VerbindungOeffnen();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM User WHERE Name = $name AND Passwort = $passwort";
            cmd.Parameters.AddWithValue("$name", tbName.Text.Trim());
            cmd.Parameters.AddWithValue("$passwort", pbPasswort.Password);

            if ((long)cmd.ExecuteScalar()! == 0)
            {
                MessageBox.Show("Name oder Passwort stimmt nicht.");
                return;
            }

            HauptfensterOeffnen();
        }

        private void btnRegistrieren_Click(object sender, RoutedEventArgs e)
        {
            if (!EingabePruefen())
            {
                return;
            }

            string name = tbName.Text.Trim();

            if (name == "Gesamt")
            {
                MessageBox.Show("Dieser Name ist nicht erlaubt.");
                return;
            }

            using SqliteConnection connection = MainWindow.VerbindungOeffnen();

            SqliteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM User WHERE Name = $name";
            cmd.Parameters.AddWithValue("$name", name);

            if ((long)cmd.ExecuteScalar()! > 0)
            {
                MessageBox.Show("Diesen Namen gibt es schon.");
                return;
            }

            cmd.CommandText = "INSERT INTO User (Name, Passwort) VALUES ($name, $passwort)";
            cmd.Parameters.AddWithValue("$passwort", pbPasswort.Password);
            cmd.ExecuteNonQuery();

            HauptfensterOeffnen();
        }
    }
}
