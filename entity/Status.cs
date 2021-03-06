using entity.Class;
using System.ComponentModel;

namespace entity
{
    public static class Status
    {
        public enum CommercialMethods
        {
            DiscountOnInvoice,
            DiscountOnPacking
        }

        [TypeConverter(typeof(EnumDescriptionTypeConverter))]
        public enum ReturnTypes
        {
            [LocalizedDescription("ItemDefect")]
            ItemDefect = 1,

            [LocalizedDescription("ItemExpired")]
            ItemExpired = 2,

            [LocalizedDescription("Discount")]
            Discount = 3,

            [LocalizedDescription("Bonus")]
            Bonus = 4,

            [LocalizedDescription("Error")]
            Error = 5,

            [LocalizedDescription("Freight")]
            Freight = 6
        }

        [TypeConverter(typeof(EnumDescriptionTypeConverter))]
        public enum PackingTypes
        {
            [LocalizedDescription("Sample")]
            Sample = 1,

            [LocalizedDescription("Sale")]
            Sale = 2,

            [LocalizedDescription("Repair")]
            Repair = 3,
        }

        //[TypeConverter(typeof(EnumDescriptionTypeConverter))]
        public enum TransactionTypes
        {
            Normal,
            Bonificacion,
            Cambio,
            Marketing,
            Muestra,
            Other
        }

        [TypeConverter(typeof(EnumDescriptionTypeConverter))]
        public enum geo_types
        {
            [LocalizedDescription("Continent")]
            Continent,

            [LocalizedDescription("Country")]
            Country,

            [LocalizedDescription("State")]
            State,

            [LocalizedDescription("City")]
            City,

            [LocalizedDescription("Zone")]
            Zone
        }

        public enum Documents
        {
            Pending,
            Issued,
            Returned
        }

        public enum Documents_General
        {
            Pending = 1,
            Approved = 2,
            Annulled = 3,
            Rejected = 4
        }

        public enum Transfer
        {
            Pending = 1,
            Approved = 2,
            Annulled = 3,
            Transit = 4
        }

        public enum Production
        {
            Pending = 1,
            Approved = 2,
            InProcess = 3,
            Executed = 4,
            QA_Check = 5,
            QA_Rejected = 6,
            Anull = 7
        }

        public enum Project
        {
            Pending = 1,
            Approved = 2,
            Management_Approved = 6,
            InProcess = 3,
            Executed = 4,
            Rejected = 5
        }

        public enum Stock
        {
            //  [LocalizedDescription("OnTheWay")]
            OnTheWay = 1,

            // [LocalizedDescription("InStock")]
            InStock = 2,

            // [LocalizedDescription("Reserved")]
            Reserved = 3
        }
    }
}