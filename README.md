# TO-DO

Gemeinsame To-Do-Anwendung (Teamprojekt, GitHub-Programmierbeispiel 4).

## Team
| Name | Aufgabe |
|------|---------|
| Alex | Projektleitung, WPF-Anwendung mit SQLite |
| Mia | Weboberfläche (HTML, CSS, JavaScript) |
| Christopher | Layout, Datenbank-Diagramm |
| Liana | Design (Figma), Präsentation |

## Funktionen
- To-Dos pro Person anzeigen (oder alle: "Gesamt")
- To-Do mit Datum, Uhrzeit und Beschreibung hinzufügen
- To-Do als erledigt markieren (nochmal klicken: zurücksetzen)
- To-Do ändern (WPF: Eintrag anklicken, Felder ändern, "Ändern"; Web: Stift-Symbol)
- Web: Wochen blättern, "Heute"-Knopf, überfällige Aufgaben rot
- To-Do löschen
- Daten bleiben gespeichert (WPF: SQLite-Datei `todo.db`, Web: Browser)

## Projektstruktur
```
WPF-Anw/       WPF-Anwendung (C#, SQLite)
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
Die Datenbank `todo.db` wird beim ersten Start automatisch erstellt.

Weboberfläche: `page/index.html` im Browser öffnen.

## Regeln
- Commit-Message: `Person: Feature` plus kurze Beschreibung
- Kein Merge direkt in `main`, immer Pull Request und Personen als Reviewer angeben
- Branch-Namen: `feature/...`, `fix/...`, `docs/...`, `refactor/...`
