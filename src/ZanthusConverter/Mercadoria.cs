using System;

namespace ZanthusConverter
{
	public class Mercadoria
	{
		#region Propriedades
		public int CodLoja { get; set; }
		public string CodMercadoria { get; set; }
		public string Descricao { get; set; }
		public decimal PrecoUnitario { get; set; }
		public string CodSitTrib { get; set; }
		public decimal AliqInternaICMS { get; set; }
		public string CodSitTribPIS { get; set; }
		public decimal AliqPIS { get; set; }
		public string CodSitTribCOFINS { get; set; }
		public decimal AliqCOFINS { get; set; }
		public string CFOP { get; set; }
		public string CEST { get; set; }
		public string NCM { get; set; }
		public string CSTOrigemInteg { get; set; }
		public string[] CBNEF { get; set; }
		public int FlagInativo { get; set; }
		#endregion

		#region Construtores
		public Mercadoria()
		{

		}

		public Mercadoria(int codLoja, string codMercadoria)
		{
			CodLoja = codLoja;
			CodMercadoria = codMercadoria;
		}
		#endregion
	}
}
