﻿using entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cognitivo.Menu
{
    public partial class MainLogIn : Page
    {
        private Frame myFrame;
        private Task taskAuth;
        private MainWindow myWindow;// = Window.GetWindow(this);// as MainWindow;

        public MainLogIn()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            myWindow = Window.GetWindow(this) as MainWindow;
            myFrame = myWindow.mainFrame;

            entity.Properties.Settings Settings = new entity.Properties.Settings();

            if (Settings.user_Name != null || Settings.user_UserName != "")
            {
                tbxUser.Text = Settings.user_UserName;
                tbxPassword.Focus();
                chbxRemember.IsChecked = true;
            }
            else
            {
                tbxUser.Focus();
            }
        }

        private void btnLogIn_Click(object sender, EventArgs e)
        {
            if (taskAuth == null)
            {
                string u = tbxUser.Text;
                string p = tbxPassword.Password;
                taskAuth = Task.Factory.StartNew(() => auth_Login(u, p));
            }
        }

        private void auth_Login(string u, string p)
        {
            Dispatcher.BeginInvoke((Action)(() => { this.Cursor = Cursors.AppStarting; }));
            Dispatcher.BeginInvoke((Action)(() => { progBar.IsIndeterminate = true; }));

            security_user User = null;
            security_role Role = null;

            //Originally inside Current Session. I have removed it to take advantage of Warm Queries, to help speed up process.
            using (db db = new db())
            {
                db.Configuration.AutoDetectChangesEnabled = false;

                User = db.security_user.Where(x => x.name == u
                                                 && x.password == p
                                                 && x.id_company == CurrentSession.Id_Company && x.is_active)
                                       .FirstOrDefault();
                if (User != null)
                {
                    Role = User.security_role;
					User.trans_date = DateTime.Now;

                    
                }
                else
                {
                    //Incorrect user credentials.
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        tbxPassword.Focus();
                        Cursor = Cursors.Arrow;
                        progBar.IsIndeterminate = false;
                    }));
                    taskAuth = null;
                    return;
                }
				db.SaveChanges();
            }

            try
            {
                Properties.Settings ViewSettings = new Properties.Settings();
                CurrentSession.ConnectionString = ViewSettings.MySQLconnString;

                CurrentSession.Start(User, Role);

                Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (chbxRemember.IsChecked == true)
                    {
                        entity.Properties.Settings Settings = new entity.Properties.Settings()
                        {
                            user_UserName = tbxUser.Text
                        };

                        Settings.Save();
                    }
                }));

                myWindow.is_LoggedIn = true;
                Dispatcher.BeginInvoke((Action)(() => myFrame.Navigate(new mainMenu_Corporate())));
            }
            catch (Exception ex) { throw ex; } //Do Nothing
            finally
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    Cursor = Cursors.Arrow;
                    progBar.IsIndeterminate = false;
                }));
                taskAuth = null;
            }
        }

        private void SetUp_MouseUp(object sender, EventArgs e)
        {
            myFrame.Navigate(new StartUp());
        }

        private void Settings_MouseUp(object sender, EventArgs e)
        {
            myFrame.Navigate(new Configs.Settings());
        }

    }
}