namespace To_Do_App
{
    // Ein To-Do aus der Tabelle "Todo"
    public class Todo
    {
        public long Id { get; set; }
        public string Datum { get; set; }
        public string Beschreibung { get; set; }
        public string Person { get; set; }

        public Todo(long id, string datum, string beschreibung, string person)
        {
            Id = id;
            Datum = datum;
            Beschreibung = beschreibung;
            Person = person;
        }

        // So wird das To-Do in der ListBox angezeigt
        public override string ToString()
        {
            return Datum + "  -  " + Beschreibung + "  (" + Person + ")";
        }
    }
}
