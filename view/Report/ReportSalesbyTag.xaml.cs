﻿using System;
using System.Collections.Generic;
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
using entity;
using System.Data.SqlClient;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
namespace Cognitivo.Report
{
    /// <summary>
    /// Interaction logic for ReportSalesbyTerminal.xaml
    /// </summary>
    public partial class ReportSalesbyTag : Page
    {
        db db = new db();
        string _connString = string.Empty;
        public ReportSalesbyTag()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Cognitivo.Properties.Settings Settings = new Properties.Settings();
            _connString = Settings.MySQLconnString;

            DataTable dt = exeDT(sql());
            dgvreport.ItemsSource = dt.DefaultView;
        }
        public DataTable exeDT(string sql)
        {
            DataTable dt = new DataTable();
            try
            {
                MySqlConnection sqlConn = new MySqlConnection(_connString);
                sqlConn.Open();
                MySqlCommand cmd = new MySqlCommand(sql, sqlConn);
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                dt = new DataTable();
                da.Fill(dt);
                sqlConn.Close();
            }
            catch
            {
                MessageBox.Show("Unable to Connect to Database. Please Check your credentials.");
            }
            return dt;
        }
        private string sql()
        {

            string sql = string.Empty;
            sql = " select * from (select ";
            sql += " (select code from items where id_item=sales_invoice_detail.id_item) as code,";
            sql += " (select name from items where id_item=sales_invoice_detail.id_item) as Description,";
            sql += " sum(quantity) as qty,sum(unit_price) as price,";
            sql += " (sum(item_movement.credit)-sum(item_movement.debit))as profit";
            sql += " ,sum(discount) as discount,(select max(id_item_tag_detail) from item_tag_detail where id_item=sales_invoice_detail.id_item) as tag_detail";
            sql += " from sales_invoice_detail left outer join item_movement on item_movement.id_sales_invoice_detail=sales_invoice_detail.id_sales_invoice_detail   ";
            if (dtpTrans_Date.SelectedDate != null)
            {
                sql += " where trans_date = '" + dtpTrans_Date.SelectedDate + "'";

            }
            else
            {
                sql += " where 1=1 ";
            }

            sql += " group by id_item) as itemgroup group by itemgroup.tag_detail";
            return sql;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = exeDT(sql());
            dgvreport.ItemsSource = dt.DefaultView;
            //cbxTerminal.SelectedValue = null;
        }
    }
}
