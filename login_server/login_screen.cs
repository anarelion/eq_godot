using EQGodot2.network_manager.login_server;
using EQGodot2.network_manager.network_session;
using Godot;
using System;
using System.IO;

namespace EQGodot2.login_screen
{

    public partial class login_screen : Control
    {

        private void OnLoginButtonPressed()
        {
            var username = GetNode<LineEdit>("Background/Margins/VBox/UsernameLineEdit").Text;
            var password = GetNode<LineEdit>("Background/Margins/VBox/PasswordLineEdit").Text;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return;
            }

            var session = new LoginSession(username, password);
            session.MessageUpdate += OnMessageUpdate;
            GetTree().Root.AddChild(session);
        }

        private void OnMessageUpdate(string message) {
            GetNode<Label>("Background/Margins/VBox/StatusLabel").Text = message;
        }
    }
}