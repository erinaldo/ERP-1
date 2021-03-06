﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace entity.Brillo.Promotion
{
    /// <summary>
    /// Interaction logic for PromotionProduct.xaml
    /// </summary>
    public partial class PromotionProduct : Window
    {
        public List<entity.Brillo.Promotion.DetailProduct> ProductList { get; set; }
        public int TagID { get; set; }
        public Decimal TotalQuantity { get; set; }

        private List<entity.Brillo.Promotion.DetailProduct> TotalProduct = new List<DetailProduct>();
        private db db = new db();

        public PromotionProduct()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (TotalProduct.Where(x => x.Quantity > 0).Sum(x => x.Quantity) != TotalQuantity)
            {
                MessageBox.Show("Invalid quantity.. Total Quantity Is:" + TotalQuantity + " You have Selectd :-" + TotalProduct.Where(x => x.Quantity > 0).Sum(x => x.Quantity));
            }
            else
            {
                ProductList = TotalProduct.Where(x => x.Quantity > 0).ToList();
                this.Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //IQueryable<entity.BrilloQuery.GetItem> ItemList;

            //entity.BrilloQuery.GetItems Execute = new entity.BrilloQuery.GetItems();

            List<item> items = db.items.Where(a => a.item_tag_detail.Where(x => x.id_tag == TagID).Count() > 0 && a.id_company == CurrentSession.Id_Company).OrderBy(a => a.name).ToList();
            //ItemList = Execute.List.AsQueryable();
            //ItemList = ItemList.Where(x => x.InStock > 0).AsQueryable();
            foreach (item _items in items)
            {
                entity.Brillo.Promotion.DetailProduct DetailProduct = new Promotion.DetailProduct();
                DetailProduct.Name = _items.name;
                DetailProduct.Code = _items.code;
                DetailProduct.ProductId = _items.id_item;
                TotalProduct.Add(DetailProduct);
            }
            Item_detailDataGrid.ItemsSource = TotalProduct;
        }
    }
}