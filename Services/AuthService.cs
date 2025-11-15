namespace tutorial.Services
{
    public class AuthService
    {
        public event Action? OnChange;
        private bool _authState = false;

        private string _userState = "";

        public bool AuthState
        {
            get => _authState;
            set
            {
                _authState = value;
                OnChange?.Invoke();
            }
        }

        public string UserState
        {
            get => _userState;
            set
            {
                _userState = value;
                OnChange?.Invoke();
            }
        }
    }
}