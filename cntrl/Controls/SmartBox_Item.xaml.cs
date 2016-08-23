﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace cntrl.Controls
{
    public partial class SmartBox_Item : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
        public bool can_New
        {
            get { return _can_new; }
            set
            {
                entity.Brillo.Security Sec = new entity.Brillo.Security(entity.App.Names.Items);
                if (Sec.create)
                {
                    _can_new = value;
                    RaisePropertyChanged("can_New");
                }
                else
                {
                    _can_new = false;
                    RaisePropertyChanged("can_New");
                }
            }
        }
        bool _can_new;
        public bool Is_Stock
        {
            get { return _Is_Stock; }
            set
            {

                _Is_Stock = value;
                RaisePropertyChanged("Is_Stock");

            }
        }
        bool _Is_Stock;
        public bool can_Edit
        {
            get { return _can_new; }
            set
            {
                entity.Brillo.Security Sec = new entity.Brillo.Security(entity.App.Names.Items);
                if (Sec.edit)
                {
                    _can_edit = value;
                    RaisePropertyChanged("can_Edit");
                }
                else
                {
                    _can_edit = false;
                    RaisePropertyChanged("can_Edit");
                }
            }
        }
        bool _can_edit;

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(SmartBox_Item));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public event RoutedEventHandler Select;
        private void ItemGrid_MouseDoubleClick(object sender, EventArgs e)
        {
            if (itemViewSource != null)
            {
                if (itemViewSource.View != null)
                {
                    Item = itemViewSource.View.CurrentItem as entity.item;

                    if (Item != null)
                    {
                        ItemID = Item.id_item;
                        ItemPopUp.IsOpen = false;
                        Text = Item.name;
                    }
                    else
                    {
                        ItemID = 0;
                        Text = tbxSearch.Text;
                    }

                    tbxSearch.SelectAll();
                }
            }

            if (Select != null)
            { Select(this, new RoutedEventArgs()); }
        }

        public int ItemID { get; set; }
        public entity.item Item { get; set; }
        public entity.item.item_type? item_types { get; set; }
        entity.dbContext db = new entity.dbContext();
        Task taskSearch;
        CancellationTokenSource tokenSource;
        CancellationToken token;

        CollectionViewSource itemViewSource;

        public SmartBox_Item()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                InitializeComponent();
                this.IsVisibleChanged += new DependencyPropertyChangedEventHandler(LoginControl_IsVisibleChanged);
                itemViewSource = ((CollectionViewSource)(FindResource("itemViewSource")));
             
            }
        }
        void LoginControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true)
            {
                Dispatcher.BeginInvoke(
                DispatcherPriority.ContextIdle,
                new Action(delegate()
                {
                    tbxSearch.Focus();
                }));
            }
        }

        private void StartSearch(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ItemGrid_MouseDoubleClick(sender, e);
            }

            else if (e.Key == Key.Up)
            {
                if (itemViewSource!=null)
                {
                    if (itemViewSource.View != null)
                    {
                        itemViewSource.View.MoveCurrentToPrevious();
                        itemViewSource.View.Refresh();
                    }
                }
              
            
            }
            else if (e.Key == Key.Down)
            {
                if (itemViewSource != null)
                {
                    if (itemViewSource.View != null)
                    {
                        itemViewSource.View.MoveCurrentToNext();
                        itemViewSource.View.Refresh();
                    }
                }
               
            }
            else
            {

                string SearchText = tbxSearch.Text;

                if (SearchText.Count() >= 3)
                {
                    if (taskSearch != null)
                    {
                        if (taskSearch.Status == TaskStatus.Running)
                        {
                            tokenSource.Cancel();
                        }
                    }

                    progBar.IsActive = true;

                    tokenSource = new CancellationTokenSource();
                    token = tokenSource.Token;
                    taskSearch = Task.Factory.StartNew(() => Search_OnThread(SearchText), token);
                }
                else
                {
                    //contactViewSource.Source = null;
                }
            }
        }

        private void Search_OnThread(string SearchText)
        {
            using (entity.db db = new entity.db())
            {
              

                List<entity.item> results;
                var param = smartBoxContactSetting.Default.SearchFilter;
                var predicate = PredicateBuilder.True<entity.item>();

                //var predicateOR = PredicateBuilder.False<entity.item>();

                //if (param.Contains("Code"))
                //{
                //    predicateOR = predicateOR.Or(x => x.code == SearchText);
                //}

                //if (param.Contains("Name"))
                //{
                //    predicateOR = predicateOR.Or(x => x.name.Contains(SearchText));
                //}
                
                predicate = (x => x.is_active && x.id_company == entity.CurrentSession.Id_Company &&
                                         (
                                             x.code.Contains(SearchText) ||
                                             x.name.Contains(SearchText) ||
                                             x.item_brand.name.Contains(SearchText)
                                         ));

                if (item_types != null)
                {
                    predicate = predicate.And(x => x.id_item_type == item_types);
                }

            //      predicate = predicate.And
            //(
            //    predicateOR
            //);
                results = db.items.Where(predicate).OrderBy(x => x.name).ToList();
                if (Is_Stock)
                {
                    results = results.Where(x => (x.item_product.FirstOrDefault() != null ? x.item_product.FirstOrDefault().stock : 1) > 0)

                          .OrderBy(x => x.name)
                          .ToList(); 
                }
              


                Dispatcher.InvokeAsync(new Action(() =>
                {
                    //itemViewSource = ((CollectionViewSource)(FindResource("itemViewSource")));
                    itemViewSource.Source = results;
                    Item = itemViewSource.View.CurrentItem as entity.item;

                    ItemPopUp.IsOpen = true;
                    progBar.IsActive = false;
                }));
            }
        }

        private void Add_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            entity.Brillo.Security Sec = new entity.Brillo.Security(entity.App.Names.Items);
            if (Sec.create)
            {
                cntrl.Curd.item item = new Curd.item();
                item.itemobject = new entity.item();
                popCrud.IsOpen = true;
                popCrud.Visibility = System.Windows.Visibility.Visible;
                ContactPopUp.Children.Add(item);
            }
        }

        private void crudItem_btnCancel_Click(object sender)
        {
            popCrud.IsOpen = false;
        }

        private void popupCustomize_Closed(object sender, EventArgs e)
        {
            popupCustomize.IsOpen = false;
            popupCustomize.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void Label_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            popupCustomize.IsOpen = true;
            popupCustomize.Visibility = System.Windows.Visibility.Visible;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (Controls.smartBoxItemSetting.Default.SearchFilter != null)
            {
                Controls.smartBoxItemSetting.Default.SearchFilter.Clear();
            }

            if (rbtnCode.IsChecked == true)
            {
                Controls.smartBoxItemSetting.Default.SearchFilter.Add("Code");
            }
            if (rbtnName.IsChecked == true)
            {
                Controls.smartBoxItemSetting.Default.SearchFilter.Add("Name");
            }
            if (rbtnTag.IsChecked == true)
            {
                Controls.smartBoxItemSetting.Default.SearchFilter.Add("Tag");
            }

            Controls.smartBoxItemSetting.Default.Save();

        }


    }
}
