﻿namespace entity
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class accounting_template : Audit
    {

        public accounting_template()
        {
        
            id_company = CurrentSession.Id_Company;
            id_user =  CurrentSession.Id_User;
            is_head = true;
         
            accounting_template_detail = new List<accounting_template_detail>();
            is_active = true;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id_template { get; set; }

        public string name { get; set; }
        public bool is_active { get; set; }

        public virtual ICollection<accounting_template_detail> accounting_template_detail { get; set; }
    }
}