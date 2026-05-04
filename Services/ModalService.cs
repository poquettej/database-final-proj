namespace db_final_proj.Services;

public class ModalService
{
    public event Action? OnShowLogin;
    public void ShowLogin() => OnShowLogin?.Invoke();
}