﻿using System;

namespace Reports
{
    public class Purchase
    {
        public int Status { get; set; }
        public string Number { get; set; }
        public bool Import { get; set; }
        public DateTime Date { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Branch { get; set; }
        public string Terminal { get; set; }
        public string Contract { get; set; }
        public string Conditions { get; set; }
        public string Supplier { get; set; }
        public string GovCode { get; set; }
        public string ContactCode { get; set; }
        public string Address { get; set; }
        public string Currency { get; set; }
        public decimal Rate { get; set; }
        public string Comment { get; set; }
        public string Code { get; set; }
        public string Items { get; set; }
        public string CostCenter { get; set; }
        public string Tag { get; set; }
        public string Vat { get; set; }
        public string Project { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitPriceVat { get; set; }
        public decimal SubTotal { get; set; }
        public decimal SubTotalVat { get; set; }
        public decimal Discount { get; set; }
        public decimal DiscountVat { get; set; }
        public string GeoLevel1 { get; set; }
        public string GeoLevel2 { get; set; }
        public string GeoLevel3 { get; set; }
        public string GeoLevel4 { get; set; }
        public string GeoLevel5 { get; set; }
    }
}