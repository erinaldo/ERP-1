﻿using entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cognitivo.Configs
{
    /// <summary>
    /// Interaction logic for itemMovementFraction.xaml
    /// </summary>
    public partial class itemMovement: UserControl
    {
        CollectionViewSource item_movementViewSource;
        public db db { get; set; }
        public int id_location { get; set; }
       public int id_item { get; set; }
        public long id_movement { get; set; }
        public item_movement item_movement { get; set; }
       
       
        CollectionViewSource app_measurementViewSource;
        public itemMovement()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CollectionViewSource app_dimensionViewSource = ((CollectionViewSource)(FindResource("app_dimensionViewSource")));
             app_measurementViewSource = ((CollectionViewSource)(FindResource("app_measurementViewSource")));

            item_movementViewSource = ((CollectionViewSource)(FindResource("item_movementViewSource")));
            List<item_movement> Items_InStockLIST = null;

           
                app_dimensionViewSource.Source = db.app_dimension.Where(a => a.id_company == CurrentSession.Id_Company).ToList();
                app_measurementViewSource.Source = db.app_measurement.Where(a => a.id_company == CurrentSession.Id_Company).ToList();

                //Items_InStockLIST = ExecutionDB.item_movement.ToList();
                Items_InStockLIST = db.item_movement.Where(x => x.id_location == id_location &&
                                                                         x.item_product.id_item == id_item
                                                                         && x.status == entity.Status.Stock.InStock
                                                                         && (x.credit - (x._child.Count() > 0 ? x._child.Sum(y => y.debit) : 0)) > 0).ToList();



            if (Items_InStockLIST.Count() > 0)
            {
                item_movementViewSource.Source = Items_InStockLIST;
                item_movementViewSource.View.MoveCurrentToFirst();

                id_movement = (item_movementViewSource.View.CurrentItem as item_movement).id_movement;
                item_movement = item_movementViewSource.View.CurrentItem as item_movement;
            }

        }

        private void item_movement_detailDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (item_movementViewSource != null)
            {
                if (item_movementViewSource.View != null)
                {
                    if (item_movementViewSource.View.CurrentItem as item_movement != null)
                    {
                        id_movement = (item_movementViewSource.View.CurrentItem as item_movement).id_movement;
                        item_movement = item_movementViewSource.View.CurrentItem as item_movement;
                    }
                }

            }

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Grid parentGrid = (Grid)this.Parent;
            parentGrid.Visibility = System.Windows.Visibility.Hidden;
        }

    

        private void btnCancel_Click(object sender, MouseButtonEventArgs e)
        {
            id_movement = 0;
            Grid parentGrid = (Grid)this.Parent;
            parentGrid.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}