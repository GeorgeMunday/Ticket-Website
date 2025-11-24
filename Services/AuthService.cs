namespace tutorial.Services
{
    public class AuthService
    {
        public event Action? OnChange;

        private bool _authState = false;
        private string _userState = "";
        private int _userId = 0;
        private int _roomId = 0;

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

        public int UserId
        {
            get => _userId;
            set
            {
                _userId = value;
                OnChange?.Invoke();
            }
        }

        public int RoomId
        {
            get => _roomId;
            set
            {
                _roomId = value;
                OnChange?.Invoke();
            }
        }
    }
}
