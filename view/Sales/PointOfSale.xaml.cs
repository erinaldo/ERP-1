﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Data.Entity;
using entity;
using System.Data;

namespace Cognitivo.Sales
{
    public partial class PointOfSale : Page
    {
        /// <summary>
        /// Context
        /// </summary>
        SalesInvoiceDB SalesInvoiceDB = new SalesInvoiceDB();
        PaymentDB PaymentDB = new PaymentDB();
        entity.Brillo.Promotion.Start StartPromo = new entity.Brillo.Promotion.Start(true);

        /// <summary>
        /// CollectionViewSource
        /// </summary>
        CollectionViewSource sales_invoiceViewSource;
        CollectionViewSource paymentViewSource;

        public PointOfSale()
        {
            InitializeComponent();
        }

        #region ActionButtons

        /// <summary>
        /// Navigates to CLIENT Tab
        /// </summary>
        private void btnClient_Click(object sender, EventArgs e)
        {
            tabContact.IsSelected = true;
        }

        /// <summary>
        /// Navigates to ACCOUNT UTILITY Tab
        /// </summary>
        private  void btnAccount_Click(object sender, EventArgs e)
        {

           
            tabAccount.Focus();
            frmaccount.Navigate(new Configs.AccountActive());
        }

        private void btnSales_Click(object sender, EventArgs e)
        {
            tabSales.IsSelected = true;
        }

        private void btnPayment_Click(object sender, EventArgs e)
        {
            tabPayment.IsSelected = true;
            btnPromotion_Click(sender, e);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            sales_invoice sales_invoice = (sales_invoice)sales_invoiceViewSource.View.CurrentItem as sales_invoice;
            payment payment = paymentViewSource.View.CurrentItem as payment;
            /// VALIDATIONS...
            /// 
            /// Validates if Contact is not assigned, then it will take user to the Contact Tab.
            if (sales_invoice.contact == null)
            {
                tabContact.Focus();
                return;
            }

            /// Validates if Sales Detail has 0 rows, then take you to Sales Tab.
            if (sales_invoice.sales_invoice_detail.Count == 0)
            {
                tabSales.Focus();
                return;
            }

            /// Validate Payment <= Sales.GrandTotal
            //if (payment.GrandTotal >= payment.GrandTotal_Detail)
            //{
            if (payment.GrandTotalDetail < Math.Round(sales_invoice.GrandTotal,2))
            {
                tabPayment.Focus();
                return;
            }
            if (payment.GrandTotalDetail > Math.Round(sales_invoice.GrandTotal, 2))
            {
                tabPayment.Focus();
                return;
            }

            /// If all validation is met, then we can start Sales Process.
            if (sales_invoice.contact != null && sales_invoice.sales_invoice_detail.Count > 0)
            {
                ///Approve Sales Invoice.
                ///Note> Approve includes Save Logic. No need to seperately Save.
                ///Plus we are passing True as default because in Point of Sale, we will always discount Stock.
                SalesInvoiceDB.Approve(true);

                List<payment_schedual> payment_schedualList = PaymentDB.payment_schedual.Where(x => x.id_sales_invoice == sales_invoice.id_sales_invoice && x.debit > 0).ToList();
                PaymentDB.Approve(payment_schedualList,true,(bool)chkreceipt.IsChecked);

                //Start New Sale
                New_Sale_Payment();
            }
        }

        private void New_Sale_Payment()
        {
            ///Creating new SALES INVOICE for upcomming sale. 
            ///TransDate = 0 because in Point of Sale we are assuming sale will always be done today.
            sales_invoice sales_invoice = SalesInvoiceDB.New(0, false);
            SalesInvoiceDB.sales_invoice.Add(sales_invoice);

            Dispatcher.BeginInvoke((Action)(() =>
            {
                sales_invoiceViewSource = ((CollectionViewSource)(FindResource("sales_invoiceViewSource")));
                sales_invoiceViewSource.Source = SalesInvoiceDB.sales_invoice.Local;
                sales_invoiceViewSource.View.MoveCurrentTo(sales_invoice);
            }));

            PaymentDB = new PaymentDB();
            ///Creating new PAYMENT for upcomming sale. 
            payment payment = PaymentDB.New(true);
            payment.id_currencyfx = sales_invoice.id_currencyfx;
            PaymentDB.payments.Add(payment);

            Dispatcher.BeginInvoke((Action)(() =>
            {
                paymentViewSource = ((CollectionViewSource)(FindResource("paymentViewSource")));
                paymentViewSource.Source = PaymentDB.payments.Local;
                paymentViewSource.View.MoveCurrentTo(payment);

                tabContact.Focus();
                sbxContact.Text = "";
            }));
        }

        #endregion

        #region SmartBox Selection

        private async void sbxContact_Select(object sender, RoutedEventArgs e)
        {
            if (sbxContact.ContactID > 0)
            {
                contact contact = await SalesInvoiceDB.contacts.FindAsync(sbxContact.ContactID);
                if (contact != null)
                {
                    sales_invoice sales_invoice = (sales_invoice)sales_invoiceViewSource.View.CurrentItem as sales_invoice;
                    payment payment = (payment)paymentViewSource.View.CurrentItem as payment;
                    sales_invoice.id_contact = contact.id_contact;
                    sales_invoice.contact = contact;
                    payment.id_contact = contact.id_contact;
                }
            }
        }

        private async void sbxItem_Select(object sender, RoutedEventArgs e)
        {
            if (sbxItem.ItemID > 0)
            {
                sales_invoice sales_invoice = sales_invoiceViewSource.View.CurrentItem as sales_invoice;

                if (sales_invoice != null)
                {
                    item item = await SalesInvoiceDB.items.FindAsync(sbxItem.ItemID);       
                    item_product item_product = item.item_product.FirstOrDefault();

                    if (item_product != null && item_product.can_expire)
                    {
                        crud_modalExpire.Visibility = Visibility.Visible;
                        cntrl.Panels.pnl_ItemMovementExpiry pnl_ItemMovementExpiry = new cntrl.Panels.pnl_ItemMovementExpiry(sales_invoice.id_branch, null, item.item_product.FirstOrDefault().id_item_product);
                        crud_modalExpire.Children.Add(pnl_ItemMovementExpiry);
                    }
                    else
                    {
                        decimal QuantityInStock = sbxItem.QuantityInStock;
                        sales_invoice_detail _sales_invoice_detail = SalesInvoiceDB.Select_Item(ref sales_invoice, item, QuantityInStock, false, null,sbxItem.Quantity);
                    }

                    sales_invoiceViewSource.View.Refresh();
                    CollectionViewSource sales_invoicesales_invoice_detailViewSource = FindResource("sales_invoicesales_invoice_detailViewSource") as CollectionViewSource;
                    sales_invoicesales_invoice_detailViewSource.View.Refresh();
                    paymentViewSource.View.Refresh();
                }
            }
        }

        #endregion

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //This code helps protect wrong Terminal and Branch PC from making same invoice.
            tabPOS.IsSelected = true;
            if (CurrentSession.Id_Branch > 0)
            {
                // Branch Exist, check if Terminal Exists.
                if (CurrentSession.Id_Terminal > 0)
                {
                    //app_branch app_branch = SalesInvoiceDB.app_branch.Where(x => x.id_branch == CurrentSession.Id_Branch).Include(y => y.app_terminal).FirstOrDefault();
                    //if (app_branch != null)
                    //{
                    //    if (app_branch.app_terminal.Where(x => x.id_terminal == CurrentSession.Id_Terminal).Count() == 0)
                    //    {
                    //        cbxTerminal.ItemsSource = app_branch.app_terminal.Where(x => x.is_active).ToList();
                    //        tabTerminal.IsSelected = true;
                    //    }
                    //}
                }
                else
                {
                    //Branch Exist, but Terminal Doesn't.
                    cbxTerminal.ItemsSource = SalesInvoiceDB.app_terminal.Where(x => x.id_branch == CurrentSession.Id_Branch).ToList();
                    tabTerminal.IsSelected = true;
                }

                stackBranch.Visibility = Visibility.Hidden;
            }
            else
            {
                tabTerminal.IsSelected = true;
                cbxBranch.ItemsSource = SalesInvoiceDB.app_branch.Where(x => x.id_company == CurrentSession.Id_Company && x.is_active).ToList();
                stackBranch.Visibility = Visibility.Visible;
            }

            New_Sale_Payment();

            //PAYMENT TYPE
            await SalesInvoiceDB.payment_type.Where(a => a.is_active == true && a.id_company == CurrentSession.Id_Company && a.payment_behavior == payment_type.payment_behaviours.Normal).LoadAsync();
            CollectionViewSource payment_typeViewSource = (CollectionViewSource)this.FindResource("payment_typeViewSource");
            payment_typeViewSource.Source = SalesInvoiceDB.payment_type.Local;

            cbxSalesRep.ItemsSource = CurrentSession.SalesReps; //await SalesInvoiceDB.sales_rep.Where(x => x.is_active && x.id_company == CurrentSession.Id_Company).ToListAsync(); //CurrentSession.Get_SalesRep();

            CollectionViewSource app_currencyViewSource = (CollectionViewSource)this.FindResource("app_currencyViewSource");
            app_currencyViewSource.Source = CurrentSession.Currencies;

            int Id_Account = CurrentSession.Id_Account;
            app_account app_account = await SalesInvoiceDB.app_account.FindAsync(CurrentSession.Id_Account);

            if (app_account != null)
            {
                if (app_account.app_account_session.Where(x => x.cl_date == null).Any() == false)
                {
                    btnAccount_Click(sender, null);
                 
                }
            }
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                tabContact.IsSelected = true;
            }
            else if (e.Key == Key.F2)
            {
                tabSales.IsSelected = true;
            }
            else if (e.Key == Key.F3)
            {
                tabPayment.IsSelected = true;
            }
            else if (e.Key == Key.F12)
            {
                btnSave_Click(sender, e);
            }
            else if (e.Key == Key.F5)
            {
                boderdiscount_MouseDown(sender, e);
            }
        }

        private void Cancel_MouseDown(object sender, EventArgs e)
        {
            SalesInvoiceDB.CancelAllChanges();
            PaymentDB.CancelAllChanges();

            New_Sale_Payment();

            //Clean up Contact Data.
            sbxContact.Text = "";
            sbxContact.ContactID = 0;

            sbxItem.Text = string.Empty;
            sbxItem.ItemID = 0;

            tabContact.IsSelected = true;
        }

        #region Details

        private void dgvSalesDetail_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            sales_invoiceViewSource.View.Refresh();
        }

        private void dgvPaymentDetail_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            sales_invoice sales_invoice = sales_invoiceViewSource.View.CurrentItem as sales_invoice;
            payment payment = paymentViewSource.View.CurrentItem as payment;
            payment_detail payment_detail = e.NewItem as payment_detail;
            
            payment_detail.State = EntityState.Added;
            payment_detail.IsSelected = true;
            payment_detail.Default_id_currencyfx = CurrentSession.Get_Currency_Default_Rate().id_currencyfx;
            payment_detail.id_currencyfx = CurrentSession.Get_Currency_Default_Rate().id_currencyfx;
            payment_detail.id_currency = CurrentSession.Currency_Default.id_currency;

            payment_detail.id_payment = payment.id_payment;
            payment_detail.payment = payment;
        }

        private void DeleteCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter as sales_invoice_detail != null)
            {
                e.CanExecute = true;
            }
            else if (e.Parameter as payment_detail != null)
            {
                e.CanExecute = true;
            }
        }

        private void DeleteCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure want to Delete?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                sales_invoice_detail sales_invoice_detail = e.Parameter as sales_invoice_detail;
                if (sales_invoice_detail != null)
                {
                    //DeleteDetailGridRow
                    dgvSalesDetail.CancelEdit();

                    sales_invoice sales_invoice = sales_invoiceViewSource.View.CurrentItem as sales_invoice;

                    sales_invoice.sales_invoice_detail.Remove(sales_invoice_detail);
                    sales_invoiceViewSource.View.Refresh();
                    CollectionViewSource sales_invoicesales_invoice_detailViewSource = FindResource("sales_invoicesales_invoice_detailViewSource") as CollectionViewSource;
                    sales_invoicesales_invoice_detailViewSource.View.Refresh();

                   // btnPromotion_Click(sender, e);

                }
                else if (e.Parameter as payment_detail != null)
                {
                    payment payment = paymentViewSource.View.CurrentItem as payment;
                    //DeleteDetailGridRow
                    dgvPaymentDetail.CancelEdit();
                    payment.payment_detail.Remove(e.Parameter as payment_detail);

                    paymentViewSource.View.Refresh();
                    CollectionViewSource paymentpayment_detailViewSource = FindResource("paymentpayment_detailViewSource") as CollectionViewSource;
                    paymentpayment_detailViewSource.View.Refresh();
                }
            }
        }

        #endregion

        #region Discount

        private void boderdiscount_MouseDown(object sender, EventArgs e)
        {
            popupDiscount.IsOpen = true;
        }

        private void popupCustomize_Closed(object sender, EventArgs e)
        {
            popupDiscount.IsOpen = false;
        }

        #endregion

        #region Contact CRUD

        private void btnNewCustomer_MouseDown(object sender, EventArgs e)
        {
            entity.Brillo.Security Sec = new entity.Brillo.Security(entity.App.Names.Contact);

            if (Sec.create)
            {
                popCrud.IsOpen = true;
                popCrud.Visibility = Visibility.Visible;

                //Add CRUD Panel into View.
                cntrl.Curd.contact ContactCURD = new cntrl.Curd.contact();
                ContactCURD.btnSave_Click += crudContact_btnCancel_Click;
                ContactCURD.IsCustomer = true;
                stackCustomer.Children.Add(ContactCURD);
            }
        }

        private void crudContact_btnCancel_Click(object sender)
        {
            sbxContact.LoadData();
        }

        #endregion


        private void lblGrandTotalsales_DataContextChanged(object sender, EventArgs e)
        {
            if (sales_invoiceViewSource != null && paymentViewSource != null)
            {
                if (sales_invoiceViewSource.View != null && paymentViewSource.View != null)
                {
                    if (sales_invoiceViewSource.View.CurrentItem != null && paymentViewSource.View.CurrentItem != null)
                    {
                        sales_invoice sales_invoice = sales_invoiceViewSource.View.CurrentItem as sales_invoice;
                        payment payment = paymentViewSource.View.CurrentItem as payment;
                        payment.GrandTotal = sales_invoice.GrandTotal;
                    }
                }
            }
        }

        private void NewPayment_MouseUp(object sender, MouseButtonEventArgs e)
        {
            payment payment = paymentViewSource.View.CurrentItem as payment;
            payment_detail payment_detail = new payment_detail();
            payment.payment_detail.Add(payment_detail);
        }



        private void Clear_MouseDown(object sender, EventArgs e)
        {
            if (sales_invoiceViewSource != null && paymentViewSource != null)
            {
                if (sales_invoiceViewSource.View != null && paymentViewSource.View != null)
                {
                    if (sales_invoiceViewSource.View.CurrentItem != null && paymentViewSource.View.CurrentItem != null)
                    {
                        sales_invoice sales_invoice = sales_invoiceViewSource.View.CurrentItem as sales_invoice;
                        if (sales_invoice.GrandTotal > 0)
                        {
                            decimal TrailingDecimals = sales_invoice.GrandTotal - Math.Floor(sales_invoice.GrandTotal);
                            sales_invoice.DiscountWithoutPercentage += TrailingDecimals;
                        }
                    }
                }
            }
        }

        private void btnPromotion_Click(object sender, EventArgs e)
        {
            sales_invoice sales_invoice = sales_invoiceViewSource.View.CurrentItem as sales_invoice;


            List<sales_invoice_detail> promoList = sales_invoice.sales_invoice_detail.Where(x => x.IsPromo).ToList();
            if (promoList.Count() > 0)
            {
                foreach (sales_invoice_detail sales_invoice_detail in promoList)
                {
                    if (sales_invoice_detail.id_sales_invoice_detail != sales_invoice_detail.PromoID)
                    {
                        SalesInvoiceDB.sales_invoice_detail.Remove(sales_invoice_detail);
                    }

                }

            }
            StartPromo.Calculate_SalesInvoice(ref sales_invoice);
            foreach (sales_invoice_detail sales_invoice_detail in sales_invoice.sales_invoice_detail)
            {
                if (sales_invoice_detail.item==null)
                {
                    sales_invoice_detail.item = SalesInvoiceDB.items.Find(sales_invoice_detail.id_item);
                }
            }

            CollectionViewSource sales_invoicesales_invoice_detailViewSource = (CollectionViewSource)this.FindResource("sales_invoicesales_invoice_detailViewSource");
            sales_invoicesales_invoice_detailViewSource.View.Refresh();
            sales_invoice.RaisePropertyChanged("GrandTotal");
        }

        private void btnSaveBranchTerminal_Click(object sender, RoutedEventArgs e)
        {
            app_terminal app_terminal = cbxTerminal.SelectedItem as app_terminal;
            if (app_terminal != null)
            {
                CurrentSession.Id_Terminal = app_terminal.id_terminal;
                CurrentSession.Id_Branch = app_terminal.id_branch;

                entity.Properties.Settings Settings = new entity.Properties.Settings();
                Settings.branch_ID = CurrentSession.Id_Branch;
                Settings.terminal_ID = CurrentSession.Id_Terminal;
                Settings.terminal_Name = app_terminal.name;
                Settings.Save();

                tabPOS.IsSelected = true;
                tabContact.IsSelected = true;
            }    
        }

        private void cbxBranch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            app_branch app_branch = (app_branch)cbxBranch.SelectedItem;
            if (app_branch != null)
            {
                cbxTerminal.ItemsSource = SalesInvoiceDB.app_terminal.Where(x => x.id_branch == app_branch.id_branch).ToList();
            }
        }
        private void crud_modalExpire_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (crud_modalExpire.Visibility == Visibility.Collapsed || crud_modalExpire.Visibility == Visibility.Hidden)
            {
                sales_invoice sales_invoice = sales_invoiceViewSource.View.CurrentItem as sales_invoice;
                item item = SalesInvoiceDB.items.Find(sbxItem.ItemID);

                cntrl.Panels.pnl_ItemMovementExpiry pnl_ItemMovementExpiry = crud_modalExpire.Children.OfType<cntrl.Panels.pnl_ItemMovementExpiry>().FirstOrDefault();

                if (item != null && item.id_item > 0 && sales_invoice != null)
                {
                    Settings SalesSettings = new Settings();

                    if (pnl_ItemMovementExpiry.MovementID > 0)
                    {
                        item_movement item_movement = SalesInvoiceDB.item_movement.Find(pnl_ItemMovementExpiry.MovementID);
                        decimal QuantityInStock = sbxItem.QuantityInStock;

                        sales_invoice_detail _sales_invoice_detail = SalesInvoiceDB.Select_Item(ref sales_invoice, item, QuantityInStock, false, item_movement,sbxItem.Quantity);
                        sales_invoice.RaisePropertyChanged("GrandTotal");
                    }
                 
                }
                sales_invoiceViewSource.View.Refresh();
                CollectionViewSource sales_invoicesales_invoice_detailViewSource = FindResource("sales_invoicesales_invoice_detailViewSource") as CollectionViewSource;
                sales_invoicesales_invoice_detailViewSource.View.Refresh();
                paymentViewSource.View.Refresh();

                //Cleans for reuse.
                crud_modalExpire.Children.Clear();
            }

        }
    }
}
