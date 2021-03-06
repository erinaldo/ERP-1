﻿using entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Cognitivo.Configs
{
    /// <summary>
    /// Interaction logic for Version.xaml
    /// </summary>
    public partial class Version : Page
    {
        public int UserNumber { get; set; }
        public CurrentSession.Versions version { get; set; }

        public Version()
        {
            using (db db = new db())
            {
                UserNumber = db.security_user.Where(x => x.id_company == CurrentSession.Id_Company).Count();
            }

            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            entity.Brillo.Activation Activation = new entity.Brillo.Activation();
            Activation.VersionEncrypt(version);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem tabitem = TabVersion.SelectedItem as TabItem;
            if (tabitem != null)
            {
                if (tabitem.Header.ToString().ToUpper() == "LITE".ToString())
                {
                    version = CurrentSession.Versions.Lite;
                }
                if (tabitem.Header.ToString().ToUpper() == "BASIC".ToString())
                {
                    version = CurrentSession.Versions.Basic;
                }
                else if (tabitem.Header.ToString().ToUpper() == "PYMES")
                {
                    version = CurrentSession.Versions.Medium;
                }
                else if (tabitem.Header.ToString().ToUpper() == "FULL")
                {
                    version = CurrentSession.Versions.Full;
                }
            }
        }
    }
}