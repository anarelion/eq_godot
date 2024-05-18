using Godot;
using System;

public partial class login_screen : Control
{
    private string Username;
    private string Password;

    private void OnLoginButtonPressed()
    {
        GD.Print("Login");
    }

    private void OnExitButtonPressed()
    {
        GD.Print("Exit");
    }

    private void OnLoginChanged(string newValue)
    {
        Username = newValue;
        GD.Print($"Username {Username} changed");
    }
    
    private void OnPasswordChanged(string newValue)
    {
        Password = newValue;
        GD.Print($"Password {Password} changed");
    }

}
