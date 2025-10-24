namespace Kiss.Bff.EndToEndTest.AfhandelingForm.Models
{
    public class Rootobject
    {
        public Hits hits { get; set; }
    }

    public class Hits
    {
        public Hit[] hits { get; set; }
    }

    public class Hit
    {
        public _Source _source { get; set; }
    }

    public class _Source
    {
        public Kennisbank Kennisbank { get; set; }
        public VAC VAC { get; set; }
    }

    public class VAC
    {
        public Afdelingen[] afdelingen { get; set; }
    }

    public class Kennisbank
    {
        public Afdelingen[] afdelingen { get; set; }
    }

    public class Afdelingen
    {
        public string afdelingnaam { get; set; }
        public string afdelingNaam { get; set; }
    }
}