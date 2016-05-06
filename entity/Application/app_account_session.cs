namespace entity
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;

    public partial class app_account_session : Audit, IDataErrorInfo
    {
        public app_account_session()
        {
            id_company = CurrentSession.Id_Company;
            id_user =  CurrentSession.Id_User;
            is_head = true;
            trans_date = DateTime.Now;
            op_date = DateTime.Now;
            cl_date = DateTime.Now;
            app_account_detail = new List<app_account_detail>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id_session { get; set; }
        public int? id_payment_detail { get; set; }
        [Required]
        public DateTime op_date { get; set; }
        [Required]
        public DateTime cl_date { get; set; }
        [Required]
        public DateTime trans_date { get; set; }

        public virtual ICollection<app_account_detail> app_account_detail { get; set; }
       

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
              
                if (columnName == "trans_date")
                {
                    if (trans_date == null)
                        return "Transaction date needs to be filled";
                }
                return "";
            }
        }
    }
}
