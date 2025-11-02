namespace tutorial.Services
{
    public class HeaderService
    {
        public event Action? OnChange;
        private string _heading = "";

        public string Heading
        {
            get => _heading;
            set
            {
                _heading = value;
                OnChange?.Invoke();
            }
        }
    }
}