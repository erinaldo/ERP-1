
namespace entity
{
    using Brillo;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class payment_schedual : Audit
    {

        //clsPayment objClsPayment = new clsPayment();
        //decimal final_amount;

        public payment_schedual()
        {
            id_company = CurrentSession.Company.id_company;
            id_user = CurrentSession.User.id_user;
            is_head = true;
            can_calculate = true;
            child = new List<payment_schedual>();
            expire_date = DateTime.Now;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id_payment_schedual { get; set; }
        public int? id_purchase_invoice { get; set; }
        public int? id_purchase_return { get; set; }
        public int? id_sales_invoice { get; set; }
        public int? id_sales_return { get; set; }
        public int? id_sales_order { get; set; }
        public int? id_purchase_order { get; set; }
        public int? id_note { get; set; }
        public int? id_payment_detail { get; set; }
        public Status.Documents_General status { get; set; }
        public int id_contact { get; set; }
        public int id_currencyfx { get; set; }

        public decimal debit { get { return _debit; } set { _debit = value; RaisePropertyChanged("debit"); RaisePropertyChanged("AccountReceivableBalance"); } }
        Decimal _debit;
        public decimal credit { get; set; }
        public bool can_calculate { get; set; }
       
        //   Not Mapped Properties
        [NotMapped]
        public decimal AccountPayableBalance
        {
            get
            {
                using (db db = new db())
                {
                    _AccountPayableBalance = credit - (child.Count() > 0 ? child.Sum(y => y.debit) : 0);
                }

                return _AccountPayableBalance;
            }
            set
            {
                _AccountPayableBalance = value;
            }
        }
        decimal _AccountPayableBalance;

        [NotMapped]
        public decimal AccountReceivableBalance
        {
            get
            {
                using (db db = new db())
                {
                    _AccountReceivableBalance = debit - (child.Count() > 0 ? child.Sum(y => y.credit) : 0);
                }
                return _AccountReceivableBalance;
            }
            set
            {
                _AccountReceivableBalance = value;
                RaisePropertyChanged("AccountReceivableBalance");
            }
        }
        decimal _AccountReceivableBalance;

        public DateTime trans_date { get; set; }
        public DateTime expire_date { get; set; }

        //Hierarchy
        public virtual ICollection<payment_schedual> child { get; set; }
        public virtual payment_schedual parent { get; set; }

        public virtual payment_detail payment_detail { get; set; }
        public virtual app_currencyfx app_currencyfx { get; set; }
        public virtual sales_invoice sales_invoice { get; set; }
        public virtual purchase_invoice purchase_invoice { get; set; }
        public virtual sales_order sales_order { get; set; }
        public virtual purchase_order purchase_order { get; set; }
        public virtual purchase_return purchase_return { get; set; }
        public virtual sales_return sales_return { get; set; }
        public virtual payment_promissory_note payment_promissory_note { get; set; }
        public virtual contact contact { get; set; }

        //public  IQueryable<payment_schedual> AccountPayable_WithBalance_ToList(this IQueryable<payment_schedual> payment_schedual)
        //{
        //    return payment_schedual.Where(x => x.child.Sum(y => y.credit - y.debit)>0);
        //}
    }
}
