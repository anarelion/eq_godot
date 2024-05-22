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

            if (GlobalVariables.LoginServerSession == null)
            {
                GlobalVariables.LoginServerSession = new LoginSession(username, password);
                GlobalVariables.LoginServerSession.MessageUpdate += OnMessageUpdate;
            }
        }

        private void OnMessageUpdate(string message) {
            ((Label)GetNode("Background/Margins/VBox/StatusLabel")).Text = message;
        }

        public override void _Process(double delta)
        {
            GlobalVariables.LoginServerSession?._Process(delta);
        }
    }
}