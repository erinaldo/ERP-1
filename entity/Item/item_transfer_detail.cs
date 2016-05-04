
namespace entity
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public partial class item_transfer_detail : Audit
    {
        public item_transfer_detail()
        {
            id_company = CurrentSession.Id_Company;
            id_user =  CurrentSession.Id_User;
            is_head = true;
            item_movement = new List<item_movement>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id_transfer_detail { get; set; }
        public Status.Documents_General status { get; set; }
        public int id_transfer { get; set; }
        public int? id_project_task { get; set; }
        public int id_item_product { get; set; }

        public decimal quantity_origin 
        {
            get { return _quantity_origin; }
            set
            {
                if (_quantity_origin != value)
                {
                    _quantity_origin = value;
                    RaisePropertyChanged("quantity_origin");

                    //Updates the Destination automatically. 
                    quantity_destination = _quantity_origin;
                }
            }
        }
        decimal _quantity_origin;

        public decimal quantity_destination 
        {
            get { return _quantity_destination; }
            set
            {
                if (_quantity_destination != value)
                {
                    _quantity_destination = value;
                    RaisePropertyChanged("quantity_destination");
                }
            }
        }
        decimal _quantity_destination;

        public virtual item_transfer item_transfer { get; set; }
        public virtual item_product item_product { get; set; }
        public virtual project_task project_task { get; set; }
        public virtual ICollection<item_movement> item_movement { get; set; }
    }
}
