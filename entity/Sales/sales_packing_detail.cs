namespace entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;

    public partial class sales_packing_detail : Audit, IDataErrorInfo
    {
        public sales_packing_detail()
        {
            id_company = CurrentSession.Id_Company;
            id_user = CurrentSession.Id_User;
            is_head = true;
            id_item = 0;
            sales_packing_relation = new List<sales_packing_relation>();
            item_movement = new List<item_movement>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id_sales_packing_detail { get; set; }

        public int id_sales_packing { get; set; }
        public int? id_sales_order_detail { get; set; }
        public int? id_location { get; set; }
        public long? id_movement { get; set; }

        [Required]
        [CustomValidation(typeof(Class.EntityValidation), "CheckId")]
        public int id_item
        {
            get
            {
                return _id_item;
            }
            set
            {
                if (value > 0)
                {
                    _id_item = value;
                }
            }
        }

        private int _id_item;

        [Required]
        public decimal quantity
        {
            get { return _quantity; }
            set
            {
                if (value > 0)
                {
                    _quantity = value;
                }
            }
        }

        private decimal _quantity;

        public DateTime? expire_date { get; set; }
        public string batch_code { get; set; }

        public decimal? gross_weight { get; set; }
        public decimal? net_weight { get; set; }
        public decimal? volume { get; set; }

        public int? id_branch { get; set; }

        public bool user_verified
        {
            get { return _user_verified; }
            set { _user_verified = value; RaisePropertyChanged("user_verified"); }
        }

        private bool _user_verified;

        public decimal? user_quantity
        {
            get { return _user_quantity; }
            set { _user_quantity = value; RaisePropertyChanged("user_quantity"); }
        }

        private decimal? _user_quantity;

        public virtual sales_packing sales_packing { get; set; }
        public virtual sales_order_detail sales_order_detail { get; set; }
        public virtual ICollection<sales_packing_relation> sales_packing_relation { get; set; }
        public virtual ICollection<item_movement> item_movement { get; set; }
        public virtual app_measurement measurement_weight { get; set; }
        public virtual app_measurement measurement_volume { get; set; }
        public virtual app_location app_location { get; set; }
        public virtual app_branch app_branch { get; set; }
        public virtual item item { get; set; }

        public string Error
        {
            get
            {
                StringBuilder error = new StringBuilder();

                // iterate over all of the properties
                // of this object - aggregating any validation errors
                PropertyDescriptorCollection props = TypeDescriptor.GetProperties(this);
                foreach (PropertyDescriptor prop in props)
                {
                    String propertyError = this[prop.Name];
                    if (propertyError != string.Empty)
                    {
                        error.Append((error.Length != 0 ? ", " : "") + propertyError);
                    }
                }

                return error.Length == 0 ? null : error.ToString();
            }
        }

        public string this[string columnName]
        {
            get
            {
                // apply property level validation rules
                if (columnName == "id_item")
                {
                    if (id_item == 0)
                        return "Item needs to be selected";
                }
                if (columnName == "quantity")
                {
                    if (quantity == 0)
                        return "Quantity cannot be zero.";
                }
                return "";
            }
        }
    }
}