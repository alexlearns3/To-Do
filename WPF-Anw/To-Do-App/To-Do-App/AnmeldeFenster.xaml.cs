using System.Text.RegularExpressions;
using System.Windows;

namespace To_Do_App
{
    // Erstes Fenster: anmelden oder neu registrieren (Daten liegen in der Turso-Datenbank)
    public partial class AnmeldeFenster : Window
    {
        public AnmeldeFenster()
        {
            InitializeComponent();
            Loaded += async (s, e) => await Sicher(Datenbank.TabellenErstellenAsync);
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

        // Hauptfenster für die Person öffnen und dieses Fenster schließen
        private void HauptfensterOeffnen(long id, string name)
        {
            MainWindow fenster = new MainWindow(id, name);
            fenster.Show();
            Close();
        }

        private async void btnAnmelden_Click(object sender, RoutedEventArgs e)
        {
            string name = tbName.Text.Trim();
            string mail = tbMail.Text.Trim().ToLower();
            string passwort = pbPasswort.Password;

            if ((name == "" && mail == "") || passwort == "")
            {
                MessageBox.Show("Bitte E-Mail oder Name und das Passwort eingeben.");
                return;
            }

            await Sicher(async () =>
            {
                List<Dictionary<string, object?>> zeilen = mail != ""
                    ? await Datenbank.AbfrageAsync("SELECT id, username FROM users WHERE email = ? AND password = ?", mail, passwort)
                    : await Datenbank.AbfrageAsync("SELECT id, username FROM users WHERE username = ? AND password = ? LIMIT 1", name, passwort);

                if (zeilen.Count == 0)
                {
                    MessageBox.Show("Anmeldedaten stimmen nicht.");
                    return;
                }

                HauptfensterOeffnen((long)zeilen[0]["id"]!, Convert.ToString(zeilen[0]["username"]) ?? "");
            });
        }

        private async void btnRegistrieren_Click(object sender, RoutedEventArgs e)
        {
            string name = tbName.Text.Trim();
            string mail = tbMail.Text.Trim().ToLower();
            string passwort = pbPasswort.Password;

            if (name == "" || mail == "" || passwort == "")
            {
                MessageBox.Show("Zum Registrieren bitte Name, E-Mail und Passwort eingeben.");
                return;
            }
            if (!Regex.IsMatch(mail, @"^\S+@\S+\.\S+$"))
            {
                MessageBox.Show("Bitte eine gültige E-Mail-Adresse eingeben.");
                return;
            }
            if (passwort.Length < 6)
            {
                MessageBox.Show("Das Passwort braucht mindestens 6 Zeichen.");
                return;
            }
            if (name == "Gesamt")
            {
                MessageBox.Show("Dieser Name ist nicht erlaubt.");
                return;
            }

            await Sicher(async () =>
            {
                var vorhanden = await Datenbank.AbfrageAsync("SELECT id FROM users WHERE email = ?", mail);
                if (vorhanden.Count > 0)
                {
                    MessageBox.Show("Zu dieser E-Mail gibt es schon ein Konto.");
                    return;
                }

                var neu = await Datenbank.AbfrageAsync(
                    "INSERT INTO users (username, email, password) VALUES (?, ?, ?) RETURNING id", name, mail, passwort);

                HauptfensterOeffnen((long)neu[0]["id"]!, name);
            });
        }
    }
}
