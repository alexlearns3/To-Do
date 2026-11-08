# TO-DO

Gemeinsame To-Do-Anwendung (Teamprojekt, GitHub-Programmierbeispiel 4).

## Team
| Name | Aufgabe |
|------|---------|
| Alex | Projektleitung, WPF-Anwendung |
| Mia | Weboberfläche (HTML, CSS, JavaScript) |
| Christopher | Layout, Datenbank-Diagramm, Turso-Datenbank |
| Liana | Design (Figma), Präsentation |

## Funktionen
- To-Dos pro Person anzeigen (oder alle: "Gesamt")
- To-Do mit Datum, Uhrzeit und Beschreibung hinzufügen
- To-Do mit Häkchen als erledigt markieren, auch am Handy
- To-Do ändern (WPF: Eintrag anklicken, Felder ändern, "Ändern")
- Web: Wochen blättern, "Heute"-Knopf
- To-Do löschen
- Web und WPF teilen dieselbe Online-Datenbank (Turso, basiert auf SQLite)

## Projektstruktur
```
WPF-Anw/       WPF-Anwendung (C#, Turso)
page/          Weboberfläche
files/         HTML und CSS (Anmeldung, Registrierung, Kalender)
sqlite_db/     Datenbank-Diagramm (drawio)
Figma design/  Entwürfe
```

## Starten
WPF-Anwendung (Windows, Visual Studio oder .NET SDK):
```
cd WPF-Anw/To-Do-App/To-Do-App
dotnet run
```
Vor dem Start den Turso-Token in `WPF-Anw/To-Do-App/To-Do-App/db-token.txt` speichern.

Weboberfläche: `page/config.example.js` als `page/config.js` kopieren, den Token eintragen und `page/index.html` im Browser öffnen.

`config.js` und `db-token.txt` kommen nicht ins Repository.

## Regeln
- Commit-Message: `Person: Feature` plus kurze Beschreibung
- Kein Merge direkt in `main`, immer Pull Request und Personen als Reviewer angeben
- Branch-Namen: `feature/...`, `fix/...`, `docs/...`, `refactor/...`
