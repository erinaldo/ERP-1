﻿using System;
using System.Collections.Generic;
using System.Data;

namespace entity.BrilloQuery
{
	public class Return
	{
		public int ReturnDetailID { get; set; }
		public int SalesInvoiceID { get; set; }
		public decimal SubTotalVAT { get; set; }
	}

	class Sales
	{
		private List<Return> GenerateReturn(DataTable dt)
		{
			List<Return> ReturnList = new List<Return>();

			foreach (DataRow DataRow in dt.Rows)
			{
				Return Return = new Return();
				Return.ReturnDetailID = Convert.ToInt16(DataRow["ReturnDetailID"]);
				Return.SalesInvoiceID = Convert.ToInt16(DataRow["SalesInvoiceID"]);
				Return.SubTotalVAT = Convert.ToDecimal(DataRow["SubTotalVAT"]);

				ReturnList.Add(Return);
			}

			return ReturnList;
		}

		public List<Return> ReturnInvoice_Integration(int ReturnID)
		{
			string query = @" select 
							 round(srd.quantity * srd.unit_price * vatco.vat,4) as SubTotalVAT,
							 si.id_sales_invoice as SalesInvoiceID,
							 srd.id_sales_return_detail as ReturnDetailID
							 
							from sales_return_detail as srd
 
							LEFT OUTER JOIN 
							(SELECT app_vat_group.id_vat_group, sum(app_vat.coefficient) as vat, sum(app_vat.coefficient) + 1 AS coef
								FROM app_vat_group 
									LEFT OUTER JOIN 
									app_vat_group_details ON app_vat_group.id_vat_group = app_vat_group_details.id_vat_group 
									LEFT OUTER JOIN 
									app_vat ON app_vat_group_details.id_vat = app_vat.id_vat
								GROUP BY app_vat_group.id_vat_group) 
								vatco ON vatco.id_vat_group = srd.id_vat_group

							 left join sales_invoice_detail as sid on srd.sales_invoice_detail_id_sales_invoice_detail = sid.id_sales_invoice_detail
							 left join sales_invoice as si on sid.id_sales_invoice = si.id_sales_invoice
							 where srd.id_sales_return = {0}";

			query = string.Format(query, ReturnID);
			return GenerateReturn(QueryExecutor.DT(query));
		}


	}
}