using Godot;
using System;

public partial class login_screen : Control
{
    private string Username;
    private string Password;

    public void _OnLoginButtonPressed()
    {
        GD.Print("Login");
    }

    public void _OnExitButtonPressed()
    {
        GD.Print("Exit");
    }

    public void _OnLoginChanged(string newValue)
    {
        Username = newValue;
    }
    
    public void _OnPasswordChanged(string newValue)
    {
        Password = newValue;
    }

}
