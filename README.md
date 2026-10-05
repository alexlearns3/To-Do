# To-Do


# To-Do Projekt (WPF & Webseite)

Wir bauen zu viert eine To-Do Anwendung für Desktop (WPF) und Web. Beide Oberflächen greifen auf dasselbe C# Backend zu, damit die Aufgaben überall synchron bleiben. Das Backend läuft auf einem Host-PC mit einer SQLite Datenbank.

---

## Team & Aufgabenverteilung

Wir teilen uns 2x2 auf: Zwei machen die Oberflächen (Design/UI) und zwei kümmern sich um den Code-Behind und die API.

### Design & Frontend (2 Personen)
* **WPF Interface:** XAML-Layout bauen, Eingabefelder für Aufgaben, Datum, Uhrzeit und Knöpfe gestalten.
* **Web Interface:** HTML und CSS schreiben, ein sauberes Layout für den Browser umsetzen.

### Backend & Logik (2 Personen)
* **C# Web API & DB:** Backend mit ASP.NET Core aufsetzen, SQLite Anbindung machen, Controller für User und Todos schreiben, Server im LAN freigeben.
* **Client-Anbindung:** C# Code-Behind für WPF schreiben (`HttpClient`), JavaScript `fetch()` für die Webseite schreiben, damit alles mit der API kommuniziert.

---

## Was die App können muss (Mindestanforderungen)

1. **User auswähledn:** Man kann eingeben/auswählen, wer man ist.
2. **Aufgaben erstellen:** Titel, Beschreibung, Datum und Uhrzeit angeben.
3. **Aufgaben anzeigen:** Alle eigenen To-Dos in einer Liste sehen.
4. **Erledigt markieren & löschen:** Haken setzen wenn fertig, oder löschen.
5. **Speichern:** Läuft zentral in einer SQLite Datei (`app.db`) am Host-PC.
6. **Sync:** Was man in WPF einträgt, sieht man auch auf der Webseite und umgekehrt.

---

## Ideen für spätere Erweiterungen

Falls wir mit den Grundfunktionen schnell fertig werden:

* **Schöner Karlender
* **Google Kalender Feed:** Einen `.ics` Link generieren, den man im Google Kalender eintragen kann, damit die To-Dos automatisch im Handy-Kalender auftauchen.
* **Kategorien & Farben:** Aufgaben farblich markieren (z.B. Rot für Schule, Blau für Privat).
* **Countdown:** Anzeige wie lange man noch Zeit hat (z.B. "noch 3 Stunden").
* **Einfacher Login:** Passwort vergeben mit Hashing.
* **Dark Mode:** Dunkles Design für Web und WPF.

---

## Tech Stack

* **Backend:** C#, ASP.NET Core Web API, Entity Framework Core, SQLite
* **WPF:** C#, XAML
* **Web:** HTML, CSS, JavaScript


