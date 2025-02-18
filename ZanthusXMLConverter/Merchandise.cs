using System;

namespace ZanthusXMLConverter {
	public class Merchandise {
		#region PRODUCT
		public int StoreID { get; set; }
		public string MercID { get; set; }
		public string Description { get; set; }
		public string SaleUnit { get; set; }
		public int ExpirationTime { get; set; }
		#endregion
		#region TAXES
		public int Tax { get; set; }
		public int TaxStatus { get; set; }
		public string UnitTax { get; set; }
		public int QtyTax { get; set; }
		public float TaxPerc { get; set; }
		public float MunTaxPerc { get; set; }
		public float StaTaxPerc { get; set; }
		public float FedTaxPerc { get; set; }
		public int ISSTaxStatus { get; set; }
		#endregion
		#region PIS/COFINS
		public float PVVM { get; set; }
		public int IsPISCOFINSFree { get; set; }
		public int PISTaxStatus { get; set; }
		public int PISPVVMTaxStatus { get; set; }
		public float PISSubstPerc { get; set; }
		public float PISReducPerc { get; set; }
		public double PISAliquot { get; set; }
		public double PISValue { get; set; }
		public int COFINSTaxStatus { get; set; }
		public int COFINSPVVMTaxStatus { get; set; }
		public float COFINSSubstPerc { get; set; }
		public float COFINSReducPerc { get; set; }
		public double COFINSAliquot { get; set; }
		public double COFINSValue { get; set; }
		#endregion
		#region ICMS
		public double ICMSInnerAliquot { get; set; }
		public float ICMSSubstPerc { get; set; }
		public double AccountingCost { get; set; }
		public int NCM { get; set; }
		public int CEST { get; set; }
		public int CFOP { get; set; }
		#endregion
		public int InactiveFlag { get; set; }

		public Merchandise() {

		}

		public Merchandise(int storeID, string mercID) {
			this.StoreID = storeID;
			this.MercID = mercID;
		}
	}
}
