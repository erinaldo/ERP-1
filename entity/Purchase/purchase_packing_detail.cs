namespace entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;

    public partial class purchase_packing_detail : Audit, IDataErrorInfo
    { 
        public purchase_packing_detail()
        {
            id_company = CurrentSession.Company.id_company;
            id_user = CurrentSession.User.id_user;
            is_head = true;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id_purchase_packing_detail { get; set; }
        public int id_purchase_packing { get; set; }
        public int? id_purchase_order_detail { get; set; }
        public int id_location { get; set; }
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
                    RaisePropertyChanged("unit_price");
                    RaisePropertyChanged("unit_cost");
                }

            }
        }
        private int _id_item;
        [Required]
        [CustomValidation(typeof(Class.EntityValidation), "CheckId")]
        public decimal quantity
        {
            get { return _quantity; }
            set
            {

                if (value > 0)
                {
                    _quantity = value;
                    //RaisePropertyChanged("unit_price");
                }
            }
        }
        private decimal _quantity;
        

        [NotMapped]
        public int id_branch { get; set; }

        public virtual purchase_packing purchase_packing { get; set; }
        public virtual purchase_order_detail purchase_order_detail { get; set; }
        public virtual app_location app_location { get; set; }
        public virtual item item { get; set; }
        public virtual ICollection<purchase_packing_dimension> purchase_packing_dimension { get; set; }

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
