﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Data.Entity;
using entity;
using System;
using System.Windows.Input;
using System.Windows.Media;

namespace Cognitivo.Production
{
    public partial class Execution : Page
    {
        ExecutionDB ExecutionDB = new ExecutionDB();

        //Production EXECUTION CollectionViewSource
        CollectionViewSource
            projectViewSource,
            production_executionViewSource,
            production_execution_detailProductViewSource,
            production_execution_detailRawViewSource,
            production_execution_detailAssetViewSource,
            production_execution_detailServiceViewSource,
            production_execution_detailSupplyViewSource;

        //Production ORDER CollectionViewSource
        CollectionViewSource
            production_orderViewSource,
            production_order_detaillProductViewSource,
            production_order_detaillRawViewSource,
            production_order_detaillServiceViewSource,
            production_order_detaillAssetViewSource,
            production_order_detaillSupplyViewSource,
            item_dimensionViewSource;

        public Execution()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {

            item_dimensionViewSource = FindResource("item_dimensionViewSource") as CollectionViewSource;
            item_dimensionViewSource.Source = await ExecutionDB.item_dimension.Where(x => x.id_company == CurrentSession.Id_Company).ToListAsync();

            production_executionViewSource = FindResource("production_executionViewSource") as CollectionViewSource;
            await ExecutionDB.production_execution.Where(a => a.id_company == CurrentSession.Id_Company).Include(x => x.production_execution_detail).LoadAsync();
            production_executionViewSource.Source = ExecutionDB.production_execution.Local;

            production_execution_detailProductViewSource = FindResource("production_execution_detailProductViewSource") as CollectionViewSource;
            production_execution_detailRawViewSource = FindResource("production_execution_detailRawViewSource") as CollectionViewSource;
            production_execution_detailServiceViewSource = FindResource("production_execution_detailServiceViewSource") as CollectionViewSource;
            production_execution_detailAssetViewSource = FindResource("production_execution_detailAssetViewSource") as CollectionViewSource;
            production_execution_detailSupplyViewSource = FindResource("production_execution_detailSupplyViewSource") as CollectionViewSource;

            production_order_detaillProductViewSource = FindResource("production_order_detaillProductViewSource") as CollectionViewSource;
            production_order_detaillServiceViewSource = FindResource("production_order_detaillServiceViewSource") as CollectionViewSource;
            production_order_detaillRawViewSource = FindResource("production_order_detaillRawViewSource") as CollectionViewSource;
            production_order_detaillAssetViewSource = FindResource("production_order_detaillAssetViewSource") as CollectionViewSource;
            production_order_detaillSupplyViewSource = FindResource("production_order_detaillSupplyViewSource") as CollectionViewSource;

            production_orderViewSource = FindResource("production_orderViewSource") as CollectionViewSource;
            await ExecutionDB.production_order.Where(x => x.id_company == CurrentSession.Id_Company).LoadAsync();
            production_orderViewSource.Source = ExecutionDB.production_order.Local;

            projectViewSource = FindResource("projectViewSource") as CollectionViewSource;
            await ExecutionDB.projects.Where(a => a.id_company == CurrentSession.Id_Company).LoadAsync();
            projectViewSource.Source = ExecutionDB.projects.Local;

            CollectionViewSource production_lineViewSource = FindResource("production_lineViewSource") as CollectionViewSource;
            await ExecutionDB.production_line.Where(x => x.id_company == CurrentSession.Id_Company).LoadAsync();
            production_lineViewSource.Source = ExecutionDB.production_line.Local;

            CollectionViewSource hr_time_coefficientViewSource = FindResource("hr_time_coefficientViewSource") as CollectionViewSource;
            await ExecutionDB.hr_time_coefficient.Where(x => x.id_company == CurrentSession.Id_Company).LoadAsync();
            hr_time_coefficientViewSource.Source = ExecutionDB.hr_time_coefficient.Local;

            cmbcoefficient.SelectedIndex = -1;

            filter_Product();
            filter_Service();
            filter_Supply();
            filter_Raw();
            filter_Asset();

            Filter_Production(item.item_type.Product);
            Filter_Production(item.item_type.RawMaterial);
            Filter_Production(item.item_type.Supplies);
            Filter_Production(item.item_type.Service);
            Filter_Production(item.item_type.FixedAssets);
        }

        public void Filter_Production(item.item_type Type)
        {
            if (production_execution_detailSupplyViewSource != null)
            {
                if (production_execution_detailSupplyViewSource.View != null)
                {
                    production_execution_detailSupplyViewSource.View.Filter = i =>
                    {
                        production_execution_detail objproduction_execution_detail = i as production_execution_detail;
                        if (objproduction_execution_detail != null && objproduction_execution_detail.item != null)
                        {
                            if (objproduction_execution_detail.item.id_item_type == Type)
                            {
                                return true;
                            }
                            else { return false; }
                        }
                        else { return false; }
                    };
                }
            }
        }

        public void filter_Product()
        {
            int id_production_order = 0;
            if (production_executionViewSource.View.CurrentItem != null)
            {
                id_production_order = ((production_execution)production_executionViewSource.View.CurrentItem).id_production_order;
            }

            if (production_order_detaillProductViewSource != null)
            {

                List<production_order_detail> _production_order_detail =
                    ExecutionDB.production_order_detail.Where(a =>
                          a.status == Status.Project.Approved
                         && (a.item.id_item_type == item.item_type.Product
                         || a.item.id_item_type == item.item_type.Task)
                         && a.id_production_order == id_production_order)
                         .ToList();
                if (_production_order_detail.Count() > 0)
                {
                    production_order_detaillProductViewSource.Source = _production_order_detail;
                }
                else
                {
                    production_order_detaillProductViewSource.Source = null;
                }
            }

        }
        public void filter_Service()
        {
            int id_production = 0;
            if (production_executionViewSource.View.CurrentItem != null)
            {
                id_production = ((production_execution)production_executionViewSource.View.CurrentItem).id_production_order;
            }

            if (production_order_detaillServiceViewSource != null)
            {
                List<production_order_detail> _production_order_detail = ExecutionDB.production_order_detail.Where(a =>
                         a.parent == null &&
                         a.status == Status.Project.Approved &&
                        (a.item.id_item_type == item.item_type.Service || a.item.id_item_type == item.item_type.ServiceContract || a.item.id_item_type == item.item_type.Task) &&
                         a.id_production_order == id_production)
                          .ToList();
                if (_production_order_detail.Count() > 0)
                {
                    production_order_detaillServiceViewSource.Source = _production_order_detail;
                }
                else
                {
                    production_order_detaillServiceViewSource.Source = null;
                }
            }

        }
        public void filter_Supply()
        {
            int id_production = 0;
            if (production_executionViewSource.View.CurrentItem != null)
            {
                id_production = ((production_execution)production_executionViewSource.View.CurrentItem).id_production_order;
            }
            if (production_execution_detailSupplyViewSource != null)
            {

                List<production_order_detail> _production_order_detail = ExecutionDB.production_order_detail.Where(a => a.parent == null && a.status == Status.Project.Approved
                           && (a.item.id_item_type == item.item_type.Supplies || a.item.id_item_type == item.item_type.Task) && a.id_production_order == id_production).ToList();
                if (_production_order_detail.Count() > 0)
                {
                    production_order_detaillSupplyViewSource.Source = _production_order_detail;
                }

                else
                {
                    production_order_detaillSupplyViewSource.Source = null;

                }
            }

        }
        public void filter_Raw()
        {
            int id_production = 0;
            if (production_executionViewSource.View.CurrentItem != null)
            {
                id_production = ((production_execution)production_executionViewSource.View.CurrentItem).id_production_order;
            }
            if (production_order_detaillRawViewSource != null)
            {

                List<production_order_detail> _production_order_detail = ExecutionDB.production_order_detail.Where(a => a.parent == null && a.status == Status.Project.Approved
                            && (a.item.id_item_type == item.item_type.RawMaterial || a.item.id_item_type == item.item_type.Task) && a.id_production_order == id_production).ToList();
                if (_production_order_detail.Count() > 0)
                {
                    production_order_detaillRawViewSource.Source = _production_order_detail;
                }
                else
                {
                    production_order_detaillRawViewSource.Source = null;

                }
            }
        }
        public void filter_Asset()
        {
            int id_production = 0;
            if (production_executionViewSource.View.CurrentItem != null)
            {
                id_production = ((production_execution)production_executionViewSource.View.CurrentItem).id_production_order;
            }
            if (production_order_detaillAssetViewSource != null)
            {

                List<production_order_detail> _production_order_detail = ExecutionDB.production_order_detail.Where(a => a.parent == null
                && a.status == Status.Project.Approved
                         && (a.item.id_item_type == item.item_type.FixedAssets || a.item.id_item_type == item.item_type.Task) && a.id_production_order == id_production).ToList();
                if (_production_order_detail.Count() > 0)
                {
                    production_order_detaillAssetViewSource.Source = _production_order_detail;
                }
                else
                {
                    production_order_detaillAssetViewSource.Source = null;

                }
            }
        }

        private void toolBar_btnSave_Click(object sender)
        {
            ExecutionDB.SaveChanges();
        }

        private void itemserviceComboBox_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (CmbService.ContactID > 0)
            {
                contact contact = ExecutionDB.contacts.Where(x => x.id_contact == CmbService.ContactID).FirstOrDefault();
                adddatacontact(contact, treeService);
            }
        }

        public void adddatacontact(contact Data, TreeView treeview)
        {
            production_order_detail production_order_detail = (production_order_detail)treeview.SelectedItem;
            if (production_order_detail != null)
            {
                if (Data != null)
                {

                    //Product
                    int id = Convert.ToInt32(((contact)Data).id_contact);
                    if (id > 0)
                    {
                        production_execution _production_execution = (production_execution)production_executionViewSource.View.CurrentItem;
                        production_execution_detail _production_execution_detail = new entity.production_execution_detail();

                        //Check for contact
                        _production_execution_detail.id_contact = ((contact)Data).id_contact;
                        _production_execution_detail.contact = Data;
                        _production_execution_detail.quantity = 1;
                        _production_execution.RaisePropertyChanged("quantity");

                        _production_execution_detail.item = production_order_detail.item;
                        _production_execution_detail.id_item = production_order_detail.item.id_item;

                        if (cmbcoefficient.SelectedValue != null)
                        {
                            _production_execution_detail.id_time_coefficient = (int)cmbcoefficient.SelectedValue;
                        }

                        string start_date = string.Format("{0} {1}", dtpstartdate.Text, dtpstarttime.Text);
                        _production_execution_detail.start_date = Convert.ToDateTime(start_date);
                        string end_date = string.Format("{0} {1}", dtpenddate.Text, dtpendtime.Text);
                        _production_execution_detail.end_date = Convert.ToDateTime(end_date);

                        _production_execution_detail.id_production_execution = _production_execution.id_production_execution;
                        _production_execution_detail.production_execution = _production_execution;
                        _production_execution_detail.id_order_detail = production_order_detail.id_order_detail;
                        _production_execution_detail.production_order_detail = production_order_detail;

                        ExecutionDB.production_execution_detail.Add(_production_execution_detail);

                        production_execution_detailServiceViewSource.View.Refresh();
                        production_execution_detailServiceViewSource.View.MoveCurrentToLast();

                        decimal actuallqty = _production_execution.production_execution_detail.Where(x => x.item.id_item_type == item.item_type.Service && x.id_order_detail == production_order_detail.id_order_detail).Sum(x => x.quantity);
                        decimal projectedqty = production_order_detail.quantity;
                        lblProjectedempqty.Content = "Total:-" + projectedqty.ToString();
                        lblTotalemp.Content = "Total:-" + actuallqty.ToString();
                        if (actuallqty > projectedqty)
                        {
                            lblTotalemp.Foreground = Brushes.Red;
                        }
                    }
                }
            }
            else
            {
                toolBar.msgWarning("select Production order for insert");
            }
        }


        private void toolBar_btnNew_Click(object sender)
        {
            production_execution production_execution = new production_execution();
            production_execution.State = System.Data.Entity.EntityState.Added;
            production_execution.IsSelected = true;
            ExecutionDB.Entry(production_execution).State = EntityState.Added;

            production_executionViewSource.View.MoveCurrentToLast();
        }

        private void toolBar_btnEdit_Click(object sender)
        {
            if (projectDataGrid.SelectedItem != null)
            {
                production_execution production_execution = (production_execution)projectDataGrid.SelectedItem;
                production_execution.IsSelected = true;
                production_execution.State = EntityState.Modified;
                ExecutionDB.Entry(production_execution).State = EntityState.Modified;
            }
            else
            {
                toolBar.msgWarning("Please Select an Item");
            }
        }

        private void toolBar_btnCancel_Click(object sender)
        {
            ExecutionDB.CancelAllChanges();
        }

        private void toolBar_btnDelete_Click(object sender)
        {
            if (MessageBox.Show("Are you sure want to Delete?", "Cognitivo", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                ExecutionDB.production_execution.Remove((production_execution)production_executionViewSource.View.CurrentItem);
                production_executionViewSource.View.MoveCurrentToFirst();
            }
        }

        private void toolBar_btnApprove_Click(object sender)
        {
            ExecutionDB.Approve();
        }

        private void toolBar_btnAnull_Click(object sender)
        {
            //ExecutionDB.ann
        }

        private void projectDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            filter_Product();
            filter_Service();
            filter_Supply();
            filter_Raw();
            filter_Asset();

            Filter_Production(item.item_type.Product);
            Filter_Production(item.item_type.RawMaterial);
            Filter_Production(item.item_type.Supplies);
            Filter_Production(item.item_type.Service);
            Filter_Production(item.item_type.FixedAssets);
        }

        private void treeraw_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            production_order_detail production_order_detail = (production_order_detail)treeRaw.SelectedItem;
            if (production_order_detail != null)
            {
                production_execution_detailRawViewSource.View.Filter = i =>
                {
                    production_execution_detail production_execution_detail = (production_execution_detail)i;
                    if (production_execution_detail.id_order_detail == production_order_detail.id_order_detail && production_execution_detail.item.id_item_type == item.item_type.RawMaterial)
                    {
                        return true;
                    }
                    else { return false; }
                };


                production_execution _production_execution = (production_execution)projectDataGrid.SelectedItem;
                decimal actuallqty = _production_execution.production_execution_detail.Where(x => x.item.id_item_type == item.item_type.RawMaterial).Sum(x => x.quantity);
                decimal projectedqty = production_order_detail.quantity;
                lblProjectedRawQty.Content = "Total:-" + projectedqty.ToString();
                lblTotalRaw.Content = "Total:-" + actuallqty.ToString();
                if (actuallqty > projectedqty)
                {
                    lblTotalRaw.Foreground = Brushes.Red;
                }
            }
        }

        private void treeservice_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            production_order_detail production_order_detail = (production_order_detail)treeService.SelectedItem;
            if (production_order_detail != null)
            {
                production_execution_detailServiceViewSource.View.Filter = i =>
                {
                    production_execution_detail production_execution_detail = (production_execution_detail)i;

                    if (production_execution_detail.id_order_detail == production_order_detail.id_order_detail)
                    {
                        return true;
                    }
                    else { return false; }
                };

                production_execution _production_execution = (production_execution)projectDataGrid.SelectedItem;
                decimal actuallqty = _production_execution.production_execution_detail.Where(x => x.item.id_item_type == item.item_type.Service).Sum(x => x.quantity);
                decimal projectedqty = production_order_detail.quantity;
                lblProjectedempqty.Content = "Total Projectado: " + projectedqty.ToString();
                lblTotalemp.Content = "Total Real: " + actuallqty.ToString();
                if (actuallqty > projectedqty)
                {
                    lblTotalemp.Foreground = Brushes.Red;
                }

            }
        }

        private void treecapital_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            production_order_detail production_order_detail = (production_order_detail)treeAsset.SelectedItem;
            if (production_order_detail != null)
            {
                production_execution_detailAssetViewSource.View.Filter = i =>
                {
                    production_execution_detail production_execution_detail = (production_execution_detail)i;
                    if (production_execution_detail.id_order_detail == production_order_detail.id_order_detail && production_execution_detail.item.id_item_type == item.item_type.FixedAssets)
                    {
                        return true;
                    }
                    else { return false; }
                };


                production_execution _production_execution = (production_execution)projectDataGrid.SelectedItem;
                decimal actuallqty = _production_execution.production_execution_detail.Where(x => x.item.id_item_type == item.item_type.FixedAssets).Sum(x => x.quantity);
                decimal projectedqty = production_order_detail.quantity;
                lblProjectedassetqty.Content = "Total:-" + projectedqty.ToString();
                lblTotalasset.Content = "Total:-" + actuallqty.ToString();
                if (actuallqty > projectedqty)
                {
                    lblTotalasset.Foreground = Brushes.Red;
                }

                production_execution_detailProductViewSource.View.Filter = null;
            }
        }

        private void dgSupplier_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (production_execution_detailSupplyViewSource != null)
            {
                if (production_execution_detailSupplyViewSource.View != null)
                {
                    production_execution_detail obj = production_execution_detailSupplyViewSource.View.CurrentItem as production_execution_detail;

                    if (obj != null)
                    {
                        if (obj.id_item != null)
                        {
                            int _id_item = (int)obj.id_item;
                            item_dimensionViewSource.View.Filter = i =>
                            {
                                item_dimension item_dimension = i as item_dimension;
                                if (item_dimension.id_item == _id_item)
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            };
                        }

                    }
                }
            }
        }

        private void dgproduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (production_execution_detailProductViewSource != null)
            {
                if (production_execution_detailProductViewSource.View != null)
                {
                    production_execution_detail obj = (production_execution_detail)production_execution_detailProductViewSource.View.CurrentItem;
                    if (obj != null)
                    {
                        if (obj.id_item != null)
                        {
                            int _id_item = (int)obj.id_item;
                            item_dimensionViewSource.View.Filter = i =>
                            {
                                item_dimension item_dimension = i as item_dimension;
                                if (item_dimension.id_item == _id_item)
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            };
                        }

                    }
                }
            }

        }

        private void dgRaw_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (production_execution_detailRawViewSource != null)
            {
                if (production_execution_detailRawViewSource.View != null)
                {
                    production_execution_detail obj = (production_execution_detail)production_execution_detailRawViewSource.View.CurrentItem;
                    if (obj != null)
                    {
                        if (obj.id_item != null)
                        {
                            int _id_item = (int)obj.id_item;
                            item_dimensionViewSource.View.Filter = i =>
                            {
                                item_dimension item_dimension = i as item_dimension;
                                if (item_dimension.id_item == _id_item)
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            };
                        }

                    }
                }
            }
        }

        private void dgCapital_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (production_execution_detailAssetViewSource != null)
            {
                if (production_execution_detailAssetViewSource.View != null)
                {
                    production_execution_detail obj = (production_execution_detail)production_execution_detailAssetViewSource.View.CurrentItem;
                    if (obj != null)
                    {
                        if (obj.id_item != null)
                        {
                            int _id_item = (int)obj.id_item;
                            item_dimensionViewSource.View.Filter = i =>
                            {
                                item_dimension item_dimension = i as item_dimension;
                                if (item_dimension.id_item == _id_item)
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            };
                        }

                    }
                }
            }
        }

        private void treeSupply_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            production_order_detail production_order_detail = (production_order_detail)treeSupply.SelectedItem;
            if (production_order_detail != null)
            {
                production_execution_detailSupplyViewSource.View.Filter = i =>
                {
                    production_execution_detail production_execution_detail = (production_execution_detail)i;
                    if (production_execution_detail.id_order_detail == production_order_detail.id_order_detail)
                    {
                        return true;
                    }
                    else { return false; }
                };


                production_execution _production_execution = (production_execution)projectDataGrid.SelectedItem;
                decimal actuallqty = _production_execution.production_execution_detail.Where(x => x.item.id_item_type == item.item_type.Supplies).Sum(x => x.quantity);
                decimal projectedqty = production_order_detail.quantity;
                lblProjectedSuppliesqty.Content = "Total:-" + projectedqty.ToString();
                lblTotalsupplies.Content = "Total:-" + actuallqty.ToString();
                if (actuallqty > projectedqty)
                {
                    lblTotalsupplies.Foreground = Brushes.Red;
                }

            }
        }

        private void treeProduct_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            production_order_detail production_order_detail = (production_order_detail)treeProduct.SelectedItem;
            if (production_order_detail != null)
            {
                production_execution_detailProductViewSource.View.Filter = i =>
                {
                    production_execution_detail production_execution_detail = (production_execution_detail)i;
                    if (production_execution_detail.item != null)
                    {
                        if (production_execution_detail.id_order_detail == production_order_detail.id_order_detail && production_execution_detail.item.id_item_type == item.item_type.Product)
                        {
                            return true;
                        }
                        else { return false; }
                    }
                    else { return false; }
                };

                production_execution _production_execution = (production_execution)projectDataGrid.SelectedItem;
                decimal actuallqty = _production_execution.production_execution_detail.Where(x => x.item.id_item_type == item.item_type.Product).Sum(x => x.quantity);
                decimal projectedqty = production_order_detail.quantity;
                lblProjectedProductQty.Content = "Total:-" + projectedqty.ToString();
                lblTotalProduct.Content = "Total:-" + actuallqty.ToString();
                if (actuallqty > projectedqty)
                {
                    lblTotalProduct.Foreground = Brushes.Red;
                }

            }
        }

        private void toolBar_btnSearch_Click(object sender, string query)
        {
            try
            {
                if (!string.IsNullOrEmpty(query))
                {
                    production_executionViewSource.View.Filter = i =>
                    {
                        production_execution production_execution = i as production_execution;
                        if (production_execution.production_order.name.ToLower().Contains(query.ToLower()))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    };
                }
                else
                {
                    production_executionViewSource.View.Filter = null;
                }
            }
            catch (Exception ex)
            {
                toolBar.msgError(ex);
            }

        }

        private void txtsupplier_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Button btn = new Button();
                btn.Name = "Supp";
                btnInsert_Click(btn, e);
            }

        }

        private void DeleteCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Parameter as production_execution_detail != null)
            {
                e.CanExecute = true;
            }
        }

        private void DeleteCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                DataGrid exexustiondetail = (DataGrid)e.Source;
                MessageBoxResult result = MessageBox.Show("Are you sure want to Delete?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    production_execution production_execution = production_executionViewSource.View.CurrentItem as production_execution;
                    //DeleteDetailGridRow
                    exexustiondetail.CancelEdit();
                    production_execution_detail production_execution_detail = e.Parameter as production_execution_detail;
                    production_execution_detail.State = EntityState.Deleted;
                    //production_execution.production_execution_detail.Remove(production_execution_detail);
                    ExecutionDB.production_execution_detail.Remove(production_execution_detail);
                    production_execution_detailAssetViewSource.View.Refresh();
                    production_execution_detailProductViewSource.View.Refresh();
                    production_execution_detailServiceViewSource.View.Refresh();
                    production_order_detaillAssetViewSource.View.Refresh();
                    production_execution_detailRawViewSource.View.Refresh();
                }
            }
            catch (Exception ex)
            {
                toolBar.msgError(ex);
            }
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            production_order_detail production_order_detail = null;
            Button btn = sender as Button;
            decimal Quantity = 0M;


            //
            if (btn.Name.Contains("Prod"))
            {
                Quantity = Convert.ToDecimal(txtProduct.Text);
                production_order_detail = treeProduct.SelectedItem as production_order_detail;
            }
            else if (btn.Name.Contains("Raw"))
            {
                Quantity = Convert.ToDecimal(txtRaw.Text);
                production_order_detail = treeRaw.SelectedItem as production_order_detail;
            }
            else if (btn.Name.Contains("Asset"))
            {
                Quantity = Convert.ToDecimal(txtAsset.Text);
                production_order_detail = treeAsset.SelectedItem as production_order_detail;
            }
            else if (btn.Name.Contains("Supp"))
            {
                Quantity = Convert.ToDecimal(txtSupply.Text);
                production_order_detail = treeSupply.SelectedItem as production_order_detail;
            }

            try
            {

                if (production_order_detail != null && Quantity > 0)
                {
                    Insert_IntoDetail(production_order_detail, Quantity);

                   
                    production_execution_detailRawViewSource.View.Refresh();
                    production_execution_detailRawViewSource.View.MoveCurrentToLast();

                    production_execution_detailSupplyViewSource.View.Refresh();
                    production_execution_detailSupplyViewSource.View.MoveCurrentToLast();

                    production_execution_detailProductViewSource.View.Refresh();
                    production_execution_detailProductViewSource.View.MoveCurrentToLast();

                    production_execution_detailAssetViewSource.View.Refresh();
                    production_execution_detailAssetViewSource.View.MoveCurrentToLast();

                    if (btn.Name.Contains("Prod"))
                    {
                        production_execution _production_execution = (production_execution)projectDataGrid.SelectedItem;
                        decimal actuallqty = _production_execution.production_execution_detail.Where(x => x.item.id_item_type == item.item_type.Product && x.id_order_detail == production_order_detail.id_order_detail).Sum(x => x.quantity);
                        decimal projectedqty = production_order_detail.quantity;
                        lblProjectedProductQty.Content = "Total:-" + projectedqty.ToString();
                        lblTotalProduct.Content = "Total:-" + actuallqty.ToString();
                        if (actuallqty > projectedqty)
                        {
                            lblTotalProduct.Foreground = Brushes.Red;
                        }
                    }
                    else if (btn.Name.Contains("Raw"))
                    {
                        production_execution _production_execution = (production_execution)projectDataGrid.SelectedItem;
                        decimal actuallqty = _production_execution.production_execution_detail.Where(x => x.item.id_item_type == item.item_type.RawMaterial && x.id_order_detail == production_order_detail.id_order_detail).Sum(x => x.quantity);
                        decimal projectedqty = production_order_detail.quantity;
                        lblProjectedRawQty.Content = "Total:-" + projectedqty.ToString();
                        lblTotalRaw.Content = "Total:-" + actuallqty.ToString();
                        if (actuallqty > projectedqty)
                        {
                            lblTotalRaw.Foreground = Brushes.Red;
                        }

                    }
                    else if (btn.Name.Contains("Asset"))
                    {
                        production_execution _production_execution = (production_execution)projectDataGrid.SelectedItem;
                        decimal actuallqty = _production_execution.production_execution_detail.Where(x => x.item.id_item_type == item.item_type.FixedAssets && x.id_order_detail == production_order_detail.id_order_detail).Sum(x => x.quantity);
                        decimal projectedqty = production_order_detail.quantity;
                        lblProjectedassetqty.Content = "Total:-" + projectedqty.ToString();
                        lblTotalasset.Content = "Total:-" + actuallqty.ToString();
                        if (actuallqty > projectedqty)
                        {
                            lblTotalasset.Foreground = Brushes.Red;
                        }

                    }
                    else if (btn.Name.Contains("Supp"))
                    {
                        production_execution _production_execution = (production_execution)projectDataGrid.SelectedItem;
                        decimal actuallqty = _production_execution.production_execution_detail.Where(x => x.item.id_item_type == item.item_type.Supplies && x.id_order_detail == production_order_detail.id_order_detail).Sum(x => x.quantity);
                        decimal projectedqty = production_order_detail.quantity;
                        lblProjectedSuppliesqty.Content = "Total:-" + projectedqty.ToString();
                        lblTotalsupplies.Content = "Total:-" + actuallqty.ToString();
                        if (actuallqty > projectedqty)
                        {
                            lblTotalsupplies.Foreground = Brushes.Red;
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                toolBar.msgError(ex);
            }
        }

        private void Insert_IntoDetail(production_order_detail production_order_detail, decimal Quantity)
        {
            production_execution _production_execution = (production_execution)projectDataGrid.SelectedItem;
            production_execution_detail _production_execution_detail = new entity.production_execution_detail();
            _production_execution_detail.State = EntityState.Added;
            _production_execution_detail.id_item = production_order_detail.id_item;
            _production_execution_detail.item = production_order_detail.item;
            _production_execution_detail.quantity = Quantity;
            _production_execution_detail.unit_cost = (decimal)production_order_detail.item.unit_cost;
            _production_execution_detail.production_execution = _production_execution;
            _production_execution_detail.id_order_detail = production_order_detail.id_order_detail;
            if (production_order_detail.item.is_autorecepie)
            {
                _production_execution_detail.is_input = false;
            }
            else
            {
                _production_execution_detail.is_input = true;
            }
            _production_execution.production_execution_detail.Add(_production_execution_detail);
        }
    }
}
