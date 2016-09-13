﻿
namespace entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;
    using System.Linq;

    public partial class item_request : Audit
    {
      
        public item_request()
        {
            item_request_detail = new List<item_request_detail>();
            id_company = CurrentSession.Id_Company;
            id_user =  CurrentSession.Id_User;
                        is_head = true;
            status = Status.Documents_General.Pending;
            request_date = DateTime.Now;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id_item_request { get; set; }
        public int? id_project { get; set; }
        public int? id_sales_order { get; set; }
        public int? id_production_order { get; set; }
        public int? id_currency { get; set; }
        public int? id_branch { get; set; }
        public int? id_department { get; set; }
           
        public DateTime request_date { get; set; }
        public string name { get; set; }
        public string comment { get; set; }
        public Status.Documents_General status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                RaisePropertyChanged("status");
            }
        }
        private Status.Documents_General _status;



        [NotMapped]
        public int TotalSelected { get; set; }

        public virtual sales_order sales_order { get; set; }
        public virtual project project { get; set; }
        public virtual production_order production_order { get; set; }
        public virtual ICollection<item_request_detail> item_request_detail { get; set; }
        public virtual ICollection<item_transfer> item_transfer { get; set; }
        public virtual security_user request_user { get; set; }
        public virtual app_currency app_currency { get; set; }
        public virtual app_department app_department { get; set; }
        public virtual app_branch app_branch { get; set; }

        public void GetTotalDecision()
        {
            int i = 0;
            
            foreach (item_request_detail detail in item_request_detail)
            {
                i += detail.GetTotalDecision();
            }

            TotalSelected = i;
            RaisePropertyChanged("TotalSelected");
        }

        //public string Error
        //{
        //    get
        //    {
        //        StringBuilder error = new StringBuilder();

        //        // iterate over all of the properties
        //        // of this object - aggregating any validation errors
        //        PropertyDescriptorCollection props = TypeDescriptor.GetProperties(this);
        //        foreach (PropertyDescriptor prop in props)
        //        {
        //            String propertyError = this[prop.Name];
        //            if (propertyError != string.Empty)
        //            {
        //                error.Append((error.Length != 0 ? ", " : "") + propertyError);
        //            }
        //        }

        //        return error.Length == 0 ? null : error.ToString();
        //    }
        //}
        //public string this[string columnName]
        //{
        //    get
        //    {
        //        return "";
        //    }
        //}
    }
}
