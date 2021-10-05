namespace CsvImporter.Models
{
    public class Flag
    {
        private bool _event = false;
        public bool Event
        {
            get => _event;
            set => _event = value;
        }
    }
}
