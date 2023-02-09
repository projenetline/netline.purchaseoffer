using System;
using System.Collections.Generic;

namespace netline.purchaseoffer.Models
{

    public class Ntl_BrwsrTalep
    {
        public string DemandNr { get; set; } = string.Empty;
        public DateTime Date_ { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = string.Empty;
        public int UrunSayisi { get; set; } = 0;
        public double ToplamTalepMiktari { get; set; } = 0;
        public double ToplamSatinAlmaMiktari { get; set; } = 0;
    }

    public class Ntl_Talep
    {

        public string ProjeKodu { get; set; } = string.Empty;
        public int ProjeId { get; set; } = 0;
        
        public DateTime Date_ { get; set; } = DateTime.Now;
        public List<Ntl_Talep_Detay> Lines { get; set; } = new List<Ntl_Talep_Detay>();

    }
    public class Ntl_Talep_Detay
    {
        public int Id { get; set; } = 0;
        public string DemandNr { get; set; } = string.Empty;
        public List<int> Transrefs { get; set; } = new List<int>();
        public string ProjeKodu { get; set; } = string.Empty;
        public string UrunGrupKodu { get; set; } = string.Empty;
        public string UrunTuru { get; set; } = string.Empty;
        public string UrunKodu { get; set; } = string.Empty;
        public string UrunAciklamasi { get; set; } = string.Empty;
        public double TalepMiktar { get; set; } = 0;
        public double Miktar { get; set; } = 0;
        public string Birim { get; set; } = string.Empty;
        public double BirimFiyat { get; set; } = 0;
        public double Tutar { get; set; } = 0;
        public string KullanimYeri { get; set; } = string.Empty;
        public string TalepEdenKisi { get; set; } = string.Empty;
        public string HediyePromosyonTipi { get; set; } = string.Empty;
        public int AnalizBoyutuRef { get; set; } = 0;
        public string AnalizBoyutuKodu { get; set; } = string.Empty;
        public string AnalizBoyutuAdi { get; set; } = string.Empty;
        public string Acente { get; set; } = string.Empty;
        public int Unitsetref { get; set; } = 0;
        public int Uomref { get; set; } = 0;
        public int Uinfo1 { get; set; } = 0;
        public int Uinfo2 { get; set; } = 0;
        public int StokMiktari { get; set; } = 0;
        public double KritikStokSeviyesi { get; set; } = 0;
        public int Stockref { get; set; } = 0;
        public string Uomsetaciklama { get; set; } = string.Empty;
        public int KartTipi { get; set; } = 0;
        public string MuhKod { get; set; } = string.Empty;
    }
    public class Ntl_Talep_DetayStockInfo
    {
        public int StokMiktari { get; set; } = 0;
        public int KritikStokSeviyesi { get; set; } = 0;
        public double BirimFiyat { get; set; } = 0;
    }
}
