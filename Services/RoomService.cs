using System;

namespace tutorial.Services
{
    public class RoomService
    {
        public event Action? OnChange;

        private string _currentRoom = "";

        public string CurrentRoom
        {
            get => _currentRoom;
            set
            {
                _currentRoom = value;
                OnChange?.Invoke();
            }
        }

        public void SetRoom(string room) => CurrentRoom = room;
    }
}
