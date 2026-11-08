namespace To_Do_App
{
    // Ein To-Do aus der Tabelle "Todo"
    public class Todo
    {
        public long Id { get; set; }
        public string Datum { get; set; }
        public string Beschreibung { get; set; }
        public string Person { get; set; }
        public bool Erledigt { get; set; }

        public Todo(long id, string datum, string beschreibung, string person)
        {
            Id = id;
            Datum = datum;
            Beschreibung = beschreibung;
            Person = person;
        }

        // So wird das Datum in der Liste angezeigt
        public string DatumAnzeige
        {
            get
            {
                if (DateTime.TryParse(Datum, out DateTime zeit))
                {
                    return zeit.ToString("dd.MM.yyyy HH:mm");
                }
                return Datum;
            }
        }
    }
}
