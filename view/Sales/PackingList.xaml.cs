﻿using entity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace Cognitivo.Sales
{
    public partial class PackingList : Page
    {
        private PackingListDB dbContext = new PackingListDB();
        private Sales.Settings SalesSettings = new Settings();
        private CollectionViewSource sales_packingViewSource, sales_packingsales_packinglist_detailViewSource, sales_orderViewSource;
        private cntrl.PanelAdv.pnlSalesOrder pnlSalesOrder;
        private cntrl.Panels.pnl_ItemMovementExpiry pnl_ItemMovementExpiry;

        public PackingList()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //entity.Properties.Settings _setting = new entity.Properties.Settings();

            sales_packingViewSource = FindResource("sales_packingViewSource") as CollectionViewSource;
            dbContext.sales_packing.Where(a => a.id_company == CurrentSession.Id_Company).Include("sales_packing_detail").Load();
            sales_packingViewSource.Source = dbContext.sales_packing.Local;
            sales_packingsales_packinglist_detailViewSource = FindResource("sales_packingsales_packing_detailViewSource") as CollectionViewSource;

            CollectionViewSource app_branchViewSource = FindResource("app_branchViewSource") as CollectionViewSource;
            app_branchViewSource.Source = dbContext.app_branch.Where(a => a.is_active == true && a.id_company == CurrentSession.Id_Company).OrderBy(a => a.name).ToList();

            CollectionViewSource app_terminalViewSource = FindResource("app_terminalViewSource") as CollectionViewSource;
            app_terminalViewSource.Source = dbContext.app_terminal.Where(a => a.is_active == true && a.id_company == CurrentSession.Id_Company).OrderBy(a => a.name).ToList();
            dbContext.app_measurement.Where(a => a.is_active == true && a.id_company == CurrentSession.Id_Company).OrderBy(a => a.name).Load();
            CollectionViewSource app_measurevolume = FindResource("app_measurevolume") as CollectionViewSource;
            CollectionViewSource app_measureweight = FindResource("app_measureweight") as CollectionViewSource;
            app_measurevolume.Source = dbContext.app_measurement.Local;
            app_measureweight.Source = dbContext.app_measurement.Local;

            sales_orderViewSource = FindResource("sales_orderViewSource") as CollectionViewSource;
            sales_orderViewSource.Source = dbContext.sales_order.Where(a => a.id_company == CurrentSession.Id_Company && a.status == Status.Documents_General.Approved).Include(a => a.sales_order_detail).ToList();

            Dispatcher.InvokeAsync(new Action(() =>
            {
                cbxDocument.ItemsSource = entity.Brillo.Logic.Range.List_Range(dbContext, entity.App.Names.PackingList, CurrentSession.Id_Branch, CurrentSession.Id_Terminal);
                cbxPackingType.ItemsSource = Enum.GetValues(typeof(Status.PackingTypes));
            }));
        }

        #region Toolbar Events

        private void toolBar_btnNew_Click(object sender)
        {
            sales_packing sales_packing = dbContext.New();
            sales_packing.trans_date = DateTime.Now.AddDays(SalesSettings.TransDate_Offset);

            dbContext.sales_packing.Add(sales_packing);
            sales_packingViewSource.View.Refresh();
            sales_packingViewSource.View.MoveCurrentToLast();
        }

        private void toolBar_btnEdit_Click(object sender)
        {
            if (sales_packingDataGrid.SelectedItem != null)
            {
                sales_packing sales_packing = (sales_packing)sales_packingDataGrid.SelectedItem;
                sales_packing.IsSelected = true;
                sales_packing.State = System.Data.Entity.EntityState.Unchanged;
                dbContext.Entry(sales_packing).State = EntityState.Modified;
                sales_packingViewSource.View.Refresh();
            }
            else
            {
                toolBar.msgWarning("Please Select an Item");
            }
        }

        private void toolBar_btnDelete_Click(object sender)
        {
            MessageBoxResult res = MessageBox.Show("Are you sure want to Delete?", "Cognitivo", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                dbContext.sales_packing.Remove((sales_packing)sales_packingDataGrid.SelectedItem);
                sales_packingViewSource.View.MoveCurrentToFirst();
                toolBar_btnSave_Click(sender);
            }
        }

        private void toolBar_btnSave_Click(object sender)
        {
            if (dbContext.SaveChanges() > 0)
            {
                sales_packingViewSource.View.Refresh();
                toolBar.msgSaved(dbContext.NumberOfRecords);
            }
        }

        private void toolBar_btnCancel_Click(object sender)
        {
            sales_packinglist_detailDataGrid.CancelEdit();
            sales_packingViewSource.View.MoveCurrentToFirst();
            dbContext.CancelAllChanges();
            if (sales_packingsales_packinglist_detailViewSource.View != null)
                sales_packingsales_packinglist_detailViewSource.View.Refresh();
        }

        #endregion Toolbar Events

        private void popupCustomize_Closed(object sender, EventArgs e)
        {
            popupCustomize.PopupAnimation = System.Windows.Controls.Primitives.PopupAnimation.Fade;
            Sales.Settings.Default.Save();
            popupCustomize.IsOpen = false;
        }

        private void item_Select(object sender, EventArgs e)
        {
            app_branch app_branch = null;
            if (sbxItem.ItemID > 0)
            {
                sales_packing sales_packing = sales_packingViewSource.View.CurrentItem as sales_packing;

                item item = dbContext.items.Where(x => x.id_item == sbxItem.ItemID).FirstOrDefault();

                if (item != null && item.id_item > 0 && sales_packing != null)
                {
                    if (cbxBranch.SelectedItem != null)
                    { app_branch = cbxBranch.SelectedItem as app_branch; }

                    item_product item_product = item.item_product.FirstOrDefault();

                    if (item_product != null && item_product.can_expire)
                    {
                        crud_modalExpire.Visibility = Visibility.Visible;
                        pnl_ItemMovementExpiry = new cntrl.Panels.pnl_ItemMovementExpiry(sales_packing.id_branch, null, item.item_product.FirstOrDefault().id_item_product);

                        crud_modalExpire.Children.Add(pnl_ItemMovementExpiry);
                    }
                    else
                    {
                        Task Thread = Task.Factory.StartNew(() => select_Item(sales_packing, item, app_branch, null, sbxItem.Quantity));
                    }
                }
            }
        }

        private void select_Item(sales_packing sales_packing, item item, app_branch app_branch, item_movement item_movement, decimal quantity)
        {
            long id_movement = item_movement != null ? item_movement.id_movement : 0;
            if (sales_packing.sales_packing_detail.Where(a => a.id_item == item.id_item && a.id_movement == id_movement).FirstOrDefault() == null)
            {
                sales_packing_detail _sales_packing_detail = new sales_packing_detail();
                _sales_packing_detail.sales_packing = sales_packing;
                _sales_packing_detail.item = item;
                _sales_packing_detail.quantity = 1;
                _sales_packing_detail.id_item = item.id_item;
                if (item_movement != null)
                {
                    _sales_packing_detail.batch_code = item_movement.code;
                    _sales_packing_detail.expire_date = item_movement.expire_date;
                    _sales_packing_detail.id_movement = (int)item_movement.id_movement;
                }
                if (app_branch != null)
                {
                    _sales_packing_detail.id_location = app_branch.app_location.Where(x => x.is_default).FirstOrDefault().id_location;
                    _sales_packing_detail.app_location = app_branch.app_location.Where(x => x.is_default).FirstOrDefault();
                }

                sales_packing.sales_packing_detail.Add(_sales_packing_detail);
            }
            else
            {
                sales_packing_detail sales_packing_detail = sales_packing.sales_packing_detail.Where(a => a.id_item == item.id_item).FirstOrDefault();
                sales_packing_detail.quantity += 1;
            }

            Dispatcher.BeginInvoke((Action)(() =>
            {
                sales_packingsales_packinglist_detailViewSource.View.Refresh();
            }));
        }

        private void tbCustomize_MouseUp(object sender, MouseButtonEventArgs e)
        {
            popupCustomize.PopupAnimation = System.Windows.Controls.Primitives.PopupAnimation.Fade;
            popupCustomize.StaysOpen = false;
            popupCustomize.IsOpen = true;
        }

        private void Hyperlink_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            crud_modal.Visibility = Visibility.Visible;
            cntrl.Curd.contact contact = new cntrl.Curd.contact();
            crud_modal.Children.Add(contact);
        }

        private void sales_packingDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void btnApprove_Click(object sender)
        {
            Settings SalesSettings = new Settings();
            dbContext.Approve(SalesSettings.DiscountStock_Packing);
        }

        private void cbxDocument_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void DeleteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter as sales_packing_detail != null)
            {
                //sales_packing_detail sales_packing_detail = e.Parameter as sales_packing_detail;
                //if (string.IsNullOrEmpty(sales_packing_detail.Error))
                //{
                e.CanExecute = true;
                //}
            }
        }

        private void DeleteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                MessageBoxResult result = MessageBox.Show("Are you sure want to Delete?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    //DeleteDetailGridRow
                    sales_packinglist_detailDataGrid.CancelEdit();
                    dbContext.sales_packing_detail.Remove(e.Parameter as sales_packing_detail);
                    sales_packingsales_packinglist_detailViewSource.View.Refresh();
                }
            }
            catch (Exception ex)
            {
                toolBar.msgError(ex);
                //throw;
            }
        }

        private void toolBar_btnAnull_Click(object sender)
        {
            dbContext.Annull();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int id_contact = sbxContact.ContactID;
            if (sales_orderViewSource != null)
            {
                if (sales_orderViewSource.View != null)
                {
                    if (sales_orderViewSource.View.Cast<sales_order>().Count() > 0)
                    {
                        sales_orderViewSource.View.Filter = i =>
                        {
                            sales_order sales_order = (sales_order)i;
                            if (sales_order.id_contact == id_contact)
                                return true;
                            else
                                return false;
                        };
                    }
                }
            }
        }

        private void btnSalesOrder_Click(object sender, RoutedEventArgs e)
        {
            crud_modal.Visibility = Visibility.Visible;
            pnlSalesOrder = new cntrl.PanelAdv.pnlSalesOrder();
            pnlSalesOrder._entity = dbContext;
            if (sbxContact.ContactID > 0)
            {
                contact contact = dbContext.contacts.Where(x => x.id_contact == sbxContact.ContactID).FirstOrDefault();
                pnlSalesOrder._contact = contact;
            }
            pnlSalesOrder.mode = cntrl.PanelAdv.pnlSalesOrder.module.sales_invoice;
            pnlSalesOrder.SalesOrder_Click += SalesOrder_Click;
            crud_modal.Children.Add(pnlSalesOrder);
        }

        public void SalesOrder_Click(object sender)
        {
            sales_packing sales_packing = (sales_packing)sales_packingViewSource.View.CurrentItem;

            foreach (sales_order item in pnlSalesOrder.selected_sales_order)
            {
                foreach (sales_order_detail _sales_order_detail in item.sales_order_detail)
                {
                    if (sales_packing.sales_packing_detail.Where(x => x.id_item == _sales_order_detail.id_item).Count() == 0)
                    {
                        sales_packing_detail sales_packing_detail = new sales_packing_detail();
                        sales_packing_detail.id_sales_order_detail = _sales_order_detail.id_sales_order_detail;
                        sales_packing_detail.id_item = _sales_order_detail.id_item;
                        sales_packing_detail.item = _sales_order_detail.item;
                        sales_packing_detail.id_movement = _sales_order_detail.movement_id;
                        sales_packing_detail.quantity = _sales_order_detail.quantity;
                        sales_packing.sales_packing_detail.Add(sales_packing_detail);
                    }

                    sales_packingsales_packinglist_detailViewSource.View.Refresh();
                    dbContext.Entry(sales_packing).Entity.State = EntityState.Added;
                    crud_modal.Children.Clear();
                    crud_modal.Visibility = Visibility.Collapsed;
                }
            }
        }

        #region Filter Data

        private void set_ContactPrefKeyStroke(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                set_ContactPref(sender, e);
            }
        }

        private void set_ContactPref(object sender, EventArgs e)
        {
            try
            {
                if (sbxContact.ContactID > 0)
                {
                    contact contact = dbContext.contacts.Where(x => x.id_contact == sbxContact.ContactID).FirstOrDefault();
                    sales_packing sales_packing = (sales_packing)sales_packingDataGrid.SelectedItem;
                    sales_packing.id_contact = contact.id_contact;
                    sales_packing.contact = contact;
                }
            }
            catch (Exception ex)
            {
                toolBar.msgError(ex);
            }
        }

        #endregion Filter Data

        private void salesorder_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Hyperlink Hyperlink = (Hyperlink)sender;
            sales_order sales_order = (sales_order)Hyperlink.Tag;
            if (sales_order != null)
            {
                entity.Brillo.Document.Start.Manual(sales_order, sales_order.app_document_range);
            }
        }

        private void toolBar_btnPrint_Click(object sender, MouseButtonEventArgs e)
        {
            sales_packing sales_packing = sales_packingDataGrid.SelectedItem as sales_packing;
            if (sales_packing != null)
            {
                entity.Brillo.Document.Start.Manual(sales_packing, sales_packing.app_document_range);
            }
            else
            {
                toolBar.msgWarning("Please select");
            }
        }

        private void cbxBranch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxBranch.SelectedItem != null)
            {
                app_branch app_branch = cbxBranch.SelectedItem as app_branch;
                cbxLocation.ItemsSource = app_branch.app_location.ToList();
            }
        }

        private async void crud_modalExpire_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (crud_modalExpire.Visibility == Visibility.Collapsed || crud_modalExpire.Visibility == Visibility.Hidden)
            {
                sales_packing sales_packing = sales_packingDataGrid.SelectedItem as sales_packing;
                item item = await dbContext.items.FindAsync(sbxItem.ItemID);
                app_branch app_branch = null;
                if (item != null && item.id_item > 0 && sales_packing != null)
                {
                    if (cbxBranch.SelectedItem != null)
                    { app_branch = cbxBranch.SelectedItem as app_branch; }

                    item_movement item_movement = dbContext.item_movement.Find(pnl_ItemMovementExpiry.MovementID);
                    Task Thread = Task.Factory.StartNew(() => select_Item(sales_packing, item, app_branch, item_movement, sbxItem.Quantity));
                }
            }
        }
    }
}