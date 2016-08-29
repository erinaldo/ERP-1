﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Data.Entity;
using entity;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;

namespace cntrl.Curd
{
    public partial class payment_quick : UserControl
    {
        PaymentDB PaymentDB = new PaymentDB();

        CollectionViewSource paymentpayment_detailViewSource;
        CollectionViewSource paymentViewSource;
        CollectionViewSource payment_schedualViewSource;

        public payment_quick(List<payment_schedual> SchedualList)
        {
            InitializeComponent();

            //Setting the Mode for this Window. Result of this variable will determine logic of the certain Behaviours.
            payment_schedualViewSource = (CollectionViewSource)this.FindResource("payment_schedualViewSource");
            paymentViewSource = (CollectionViewSource)this.FindResource("paymentViewSource");
            paymentpayment_detailViewSource = (CollectionViewSource)this.FindResource("paymentpayment_detailViewSource");

            payment_schedualViewSource.Source = SchedualList;

            payment payment = PaymentDB.New(true);
            payment.trans_date = SchedualList.Max(x => x.expire_date);
            payment.IsSelected = true;
            payment.State = EntityState.Added;

            int id_contact = SchedualList.FirstOrDefault().id_contact;
            if (PaymentDB.contacts.Where(x => x.id_contact == id_contact).FirstOrDefault() != null)
            {
                payment.id_contact = id_contact;
                payment.contact = PaymentDB.contacts.Where(x => x.id_contact == id_contact).FirstOrDefault();
            }

            PaymentDB.payments.Add(payment);
            paymentViewSource.Source = PaymentDB.payments.Local;

            payment_detail payment_detail = new payment_detail();
            payment_detail.payment = payment;
            payment_detail.value = SchedualList.FirstOrDefault().AccountPayableBalance;
            payment.payment_detail.Add(payment_detail);

            paymentViewSource.View.MoveCurrentTo(payment);
            paymentpayment_detailViewSource.View.MoveCurrentToFirst();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CollectionViewSource payment_typeViewSource = (CollectionViewSource)this.FindResource("payment_typeViewSource");
            await PaymentDB.payment_type.Where(a => a.is_active && a.id_company == CurrentSession.Id_Company).LoadAsync();
            payment_typeViewSource.Source = PaymentDB.payment_type.Local;

            CollectionViewSource app_accountViewSource = (CollectionViewSource)this.FindResource("app_accountViewSource");
            await PaymentDB.app_account.Where(a => a.is_active && a.id_company == CurrentSession.Id_Company).LoadAsync();
            app_accountViewSource.Source = PaymentDB.app_account.Local;

            cbxDocument.ItemsSource = entity.Brillo.Logic.Range.List_Range(PaymentDB, App.Names.Payment, CurrentSession.Id_Branch, CurrentSession.Id_Company);

            paymentViewSource.View.Refresh();
            paymentpayment_detailViewSource.View.Refresh();
        }

        #region Events

        private void lblCancel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Grid parentGrid = (Grid)this.Parent;
            parentGrid.Children.Clear();
            parentGrid.Visibility = Visibility.Hidden;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            payment_schedual payment_schedual = payment_schedualViewSource.View.CurrentItem as payment_schedual;
            if (payment_schedual != null)
            {
                payment_schedual.status = Status.Documents_General.Approved;
                PaymentDB.SaveChanges();
                lblCancel_MouseDown(sender, null);
            }
        }

        #endregion
    }
}
