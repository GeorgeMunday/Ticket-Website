using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using tutorial.Services;

namespace tutorial.Components.Pages
{
    public partial class Home : ComponentBase
    {
        private bool isSignUp = false;
        private bool dbAvailable = false;

        private string username = "";
        private string email = "";
        private string password = "";
        private string confirmPassword = "";

        [Inject] public HeaderService? HeaderService { get; set; }
        [Inject] public AuthService? AuthService { get; set; }
        [Inject] public NavigationManager? NavManager { get; set; }
        [Inject] public DbService? DbService { get; set; }

        protected override void OnInitialized()
        {
            HeaderService!.Heading = "Auth";
            dbAvailable = DbService!.TestConnection();
        }

        private void ToggleForm() => isSignUp = !isSignUp;

        private async Task HandleAuth()
        {
            if (!dbAvailable) return;

            if (isSignUp)
                await HandleSignUp();
            else
                await HandleSignIn();
        }

        private async Task HandleSignIn()
        {
            var (Success, UserId, RoomId) = await DbService!.SignInAsync(email, password);
            if (!Success)
            {
                Console.WriteLine("Invalid email or password");
                return;
            }

            ApplyAuth(UserId, email, RoomId);
            ClearInputs();
            NavManager!.NavigateTo("/ticketboard");
        }

        private async Task HandleSignUp()
        {
            var (Success, UserId) = await DbService!.SignUpAsync(username, email, password, confirmPassword);
            if (!Success) return;

            ApplyAuth(UserId, email, 0);
            ClearInputs();
            Console.WriteLine("Signed up successfully");
            NavManager!.NavigateTo("/ticketboard");
        }

        private void ApplyAuth(int userId, string userEmail, int roomId)
        {
            if (AuthService == null) return;

            AuthService.AuthState = true;
            AuthService.UserId = userId;
            AuthService.UserState = userEmail;
            AuthService.RoomId = roomId;
        }

        private void ClearInputs()
        {
            username = "";
            email = "";
            password = "";
            confirmPassword = "";
        }
    }
}
