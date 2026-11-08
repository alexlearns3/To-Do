using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;

namespace To_Do_App
{
    // Verbindung zur Turso-Datenbank (dieselbe wie die Web-App) über die HTTP-Schnittstelle
    public static class Datenbank
    {
        private const string Url = "https://sever-christophervoglauer.aws-eu-north-1.turso.io";
        // Token liegt in db-token.txt neben dem Projekt und kommt nicht ins Repository
        private static readonly string TokenDatei = Path.Combine(AppContext.BaseDirectory, "db-token.txt");
        private static readonly string Token = File.Exists(TokenDatei) ? File.ReadAllText(TokenDatei) : "";

        private static readonly HttpClient client = new HttpClient();

        // Führt einen SQL-Befehl aus. "?" im SQL wird der Reihe nach durch args ersetzt.
        // Gibt die Zeilen zurück (Spaltenname -> Wert). Bei INSERT/UPDATE/DELETE ist die Liste leer.
        public static async Task<List<Dictionary<string, object?>>> AbfrageAsync(string sql, params object?[] args)
        {
            JsonArray argListe = new JsonArray();
            foreach (object? a in args)
            {
                argListe.Add(WertNachJson(a));
            }

            JsonObject body = new JsonObject
            {
                ["requests"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["type"] = "execute",
                        ["stmt"] = new JsonObject { ["sql"] = sql, ["args"] = argListe }
                    },
                    new JsonObject { ["type"] = "close" }
                }
            };

            using HttpRequestMessage anfrage = new HttpRequestMessage(HttpMethod.Post, Url + "/v2/pipeline");
            anfrage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token.Trim());
            anfrage.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");

            using HttpResponseMessage antwort = await client.SendAsync(anfrage);
            string text = await antwort.Content.ReadAsStringAsync();

            if (antwort.StatusCode == HttpStatusCode.Unauthorized || antwort.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new Exception("Token ungültig oder fehlt.");
            }
            if (!antwort.IsSuccessStatusCode)
            {
                throw new Exception("HTTP " + (int)antwort.StatusCode + ": " + text);
            }

            JsonNode ergebnis = JsonNode.Parse(text)!["results"]![0]!;
            if ((string?)ergebnis["type"] != "ok")
            {
                throw new Exception((string?)ergebnis["error"]?["message"] ?? "Unbekannter Datenbankfehler");
            }

            JsonNode result = ergebnis["response"]!["result"]!;
            List<string> spalten = result["cols"]!.AsArray().Select(c => (string)c!["name"]!).ToList();

            List<Dictionary<string, object?>> zeilen = new List<Dictionary<string, object?>>();
            foreach (JsonNode? zeile in result["rows"]!.AsArray())
            {
                JsonArray zellen = zeile!.AsArray();
                Dictionary<string, object?> d = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < spalten.Count; i++)
                {
                    d[spalten[i]] = WertAusJson(zellen[i]);
                }
                zeilen.Add(d);
            }
            return zeilen;
        }

        // Legt die Tabellen an, falls es sie noch nicht gibt (gleiches Schema wie die Web-App)
        public static async Task TabellenErstellenAsync()
        {
            await AbfrageAsync(
                "CREATE TABLE IF NOT EXISTS users (" +
                "  id INTEGER PRIMARY KEY AUTOINCREMENT," +
                "  username TEXT NOT NULL," +
                "  email TEXT NOT NULL UNIQUE," +
                "  password TEXT NOT NULL)");

            await AbfrageAsync(
                "CREATE TABLE IF NOT EXISTS todos (" +
                "  id INTEGER PRIMARY KEY AUTOINCREMENT," +
                "  user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE," +
                "  description TEXT NOT NULL," +
                "  date TEXT NOT NULL)");

            // Spalte "done" (erledigt) nachtragen, falls sie fehlt
            try
            {
                await AbfrageAsync("ALTER TABLE todos ADD COLUMN done INTEGER NOT NULL DEFAULT 0");
            }
            catch (Exception ex) when (ex.Message.Contains("duplicate column", StringComparison.OrdinalIgnoreCase))
            {
                // Spalte gibt es schon
            }
        }

        private static JsonObject WertNachJson(object? v)
        {
            switch (v)
            {
                case null:
                    return new JsonObject { ["type"] = "null" };
                case long or int:
                    return new JsonObject { ["type"] = "integer", ["value"] = Convert.ToString(v, CultureInfo.InvariantCulture) };
                case bool b:
                    return new JsonObject { ["type"] = "integer", ["value"] = b ? "1" : "0" };
                case double d:
                    return new JsonObject { ["type"] = "float", ["value"] = d };
                default:
                    return new JsonObject { ["type"] = "text", ["value"] = v.ToString() };
            }
        }

        private static object? WertAusJson(JsonNode? zelle)
        {
            string? typ = (string?)zelle?["type"];
            switch (typ)
            {
                case "integer":
                    return long.Parse(zelle!["value"]!.GetValue<string>(), CultureInfo.InvariantCulture);
                case "float":
                    return zelle!["value"]!.GetValue<double>();
                case "text":
                    return zelle!["value"]!.GetValue<string>();
                default:
                    return null;
            }
        }
    }
}
