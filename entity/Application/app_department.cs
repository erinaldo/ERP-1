﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace entity
{
    public partial class app_department : Audit, IDataErrorInfo
    {
        public enum types
        {

        }
        public app_department()
        {
            is_active = true;
            is_head = true;
            id_company = CurrentSession.Id_Company;
            id_user = CurrentSession.Id_User;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id_department { get; set; }

        [Required]
        public string name { get; set; }

        public types type { get; set; }

        public bool is_active
        {
            get { return _is_active; }
            set
            {
                if (_is_active != value)
                {
                    _is_active = value;
                    RaisePropertyChanged("is_active");
                }
            }
        }

        private bool _is_active;
        public virtual ICollection<hr_position> hr_position { get; set; }
        public virtual ICollection<item_request> item_request { get; set; }
        public virtual IEnumerable<security_role> security_role { get; set; }
        public virtual IEnumerable<purchase_invoice> purchase_invoice { get; set; }

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
                if (columnName == "name")
                {
                    if (string.IsNullOrEmpty(name))
                        return "Name needs to be filled";
                }
                return "";
            }
        }
    }
}