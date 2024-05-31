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
            var username = ((LineEdit)GetNode("Background/Margins/VBox/UsernameLineEdit")).Text;
            var password = ((LineEdit)GetNode("Background/Margins/VBox/PasswordLineEdit")).Text;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return;
            }

            var session = new LoginSession(username, password);
            session.MessageUpdate += OnMessageUpdate;
            GetParent().AddChild(session);
        }

        private void OnMessageUpdate(string message) {
            ((Label)GetNode("Background/Margins/VBox/StatusLabel")).Text = message;
        }
    }
}