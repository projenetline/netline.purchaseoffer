using Dapper;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using netline.purchaseoffer.Models;
using netline.purchaseoffer.Models.BudgetModels;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web.Configuration;

namespace netline.purchaseoffer.BusinessLayer
{
    public class ProjectUtil
    {
        string logoConStr = string.Empty;
        string BudgetConStr = string.Empty;
        string BudgetControlConStr = string.Empty;
        string FlowConStr = string.Empty;

        string oracleConnStr = "";
        string crypText = "";





        string FirmNr = string.Empty;

        public ProjectUtil()
        {
            logoConStr = WebConfigurationManager.ConnectionStrings["logoConStr"].ConnectionString;
            BudgetConStr = WebConfigurationManager.ConnectionStrings["BudgetConStr"].ConnectionString;
            BudgetControlConStr = WebConfigurationManager.ConnectionStrings["BudgetControlConStr"].ConnectionString;
            FlowConStr = WebConfigurationManager.ConnectionStrings["FlowConStr"].ConnectionString;
            FirmNr = WebConfigurationManager.AppSettings["firmNr"];
            oracleConnStr = WebConfigurationManager.ConnectionStrings["oracleConnStr"].ConnectionString;
            crypText = WebConfigurationManager.AppSettings["crypText"];
        }


        public bool getSupplierStatus(string taxNr)
        {
            bool status = false;
            using (var sqlConnection = new OracleConnection(oracleConnStr))
            {
                var query = $@" SELECT RAY_BT.KML_MUSTERI_KONTROL('{taxNr}') FROM DUAL  ";
                status = sqlConnection.Query<bool>(query).FirstOrDefault();
            }
            return status;
        }
        public int UpdatePassword(int Id, string password)
        {
            string pass = Crypter.Encrypt(password, crypText);
            SqlConnection sql = new SqlConnection(logoConStr);
            return sql.Execute(" update  Ntl_User set Password=@pass ,ResetPassword=0 where Id=@Id", new { pass, Id });
        }
        public int UpdateActiveName(int Id, string ActiveName)
        {

            SqlConnection sql = new SqlConnection(logoConStr);
            return sql.Execute(" update  Ntl_User set ActiveName=@ActiveName  where Id=@Id", new { ActiveName, Id });
        }
        public Ntl_User getUser(string UserName, string password)
        {
            string pass = Crypter.Encrypt(password, crypText);
            SqlConnection sql = new SqlConnection(logoConStr);

            string aa = Crypter.Decrypt("ZIlPTUXvHCJmc7b0uP9vgA==", crypText);

            return sql.Query<Ntl_User>(" select * from  Ntl_User where UserName=@UserName and Password=@pass", new { UserName, pass }).FirstOrDefault();
        }

        public Ntl_User getUserByActiveName(string ActiveName)
        {

            SqlConnection sql = new SqlConnection(logoConStr);
            return sql.Query<Ntl_User>(" select * from  Ntl_User where ActiveName=@ActiveName   ", new { ActiveName }).FirstOrDefault();
        }

        public Ntl_User getUserById(int UserId)
        {
            SqlConnection sql = new SqlConnection(logoConStr);
            return sql.Query<Ntl_User>(" select * from  Ntl_User where Id=@UserId", new { UserId }).FirstOrDefault();
        }
        public string getProjectUserMail(int ProjectId)
        {

            SqlConnection sql = new SqlConnection(logoConStr);
            return sql.ExecuteScalar<string>("select u.Email from  Ntl_User  u  left join Ntl_Offer o on o.UserId = u.Id where ProjectId = ", new { ProjectId });
        }

        public int UpdateUser(Ntl_User user)
        {
            user.Password = Crypter.Encrypt(user.Password, crypText);
            SqlConnection sql = new SqlConnection(logoConStr);
            string query = $@" UPDATE Ntl_User
                               SET UserName = @UserName
                                  ,Password = @Password
                                  ,UserType = @UserType
                                  ,Email = @Email
                                  ,FullName = @FullName ,Budget=@Budget ,RequirementDemand=@RequirementDemand ,AnlyCode=@AnlyCode
                             WHERE Id=@Id ";
            return sql.Execute(query, user);
        }

        public int SaveUser(Ntl_User user)
        {
            user.Password = Crypter.Encrypt(user.Password, crypText);
            SqlConnection sql = new SqlConnection(logoConStr);
            string query = $@" INSERT INTO Ntl_User
                               (UserName
                               ,Password
                               ,UserType
                               ,Email
                               ,FullName,Budget,RequirementDemand ,AnlyCode ,ResetPassword)
                         VALUES
                               (@UserName
                               ,@Password
                               ,@UserType
                               ,@Email
                               ,@FullName ,@Budget ,@RequirementDemand ,@AnlyCode,1) ";
            return sql.Execute(query, user);
        }
        public List<Ntl_Demand> getDemandList()
        {
            string query = $@"  SELECT 
                              DS.LOGICALREF SlipRef,
                              DT.LOGICALREF TransRef,
                              CONVERT(NVARCHAR(50),DS.SLIPNR)  + ' - ' + CONVERT(NVARCHAR(50),DT.LINENR) SlipNr,
                              CONVERT(NVARCHAR(50),DS.SLIPNR)   DemandNr ,
                              DS.SLIPDATE SlipDate,
                              CONVERT(NVARCHAR(50),DS.SLIPDATE,104 )  SlipDateStr,
                              AUX.DESCRIPTION ItemGrpCode,
                              ITM.CODE ItemCode,
                              ITM.DESCRIPTION ItemDesc
                              ,DT.QUANTITY Quantity
                              ,UNIT.CODE Unit
                             ,(select TOP 1  TalepEden from RAYSIGORTA_BUTCE..ButceDetay  where FisNo=ds.SLIPNR)   PersonName
                              ,ISNULL(SUB_ANLY.DESCRIPTION,'')  Department,
                              DT.PROCUREDATE ProcureDate ,
                              (select SUM(ONHAND) from V_{FirmNr}_01_MMITEMTOTALS WHERE ITEMREF=ITM.LOGICALREF) StockAmount,      
                              DT.LINEEXP LineExp,
                              '' Usage  ,DT.LINENET LineNet                    
                              from  U_{FirmNr}_01_DEMANDSLIPS DS
                              LEFT JOIN U_{FirmNr}_01_DEMANDTRANS DT ON DT.DEMANDSLIPREF = DS.LOGICALREF
                              left join U_{FirmNr}_01_DEMANDTRANSADS D_ANLY ON D_ANLY.TRANSREF=DT.LOGICALREF  
                              LEFT JOIN U_{FirmNr}_ANLYDIMENSIONS ANLY  ON ANLY.LOGICALREF=D_ANLY.ANLYDIMREF
							  LEFT JOIN U_{FirmNr}_ANLYDIMENSIONS SUB_ANLY  ON SUB_ANLY.CODE=SUBSTRING(ANLY.CODE,1,10)  
                              LEFT JOIN U_{FirmNr}_UNITS UNIT ON UNIT.LOGICALREF=DT.UOMREF                             
                              LEFT JOIN U_{FirmNr}_ITEMS ITM ON ITM.LOGICALREF =DT.ITEMREF
                              left JOIN U_{FirmNr}_AUXCODES AUX ON AUX.AUXCODE = ITM.GROUPCODE AND CODETYPE=4                              
                              WHERE DS.AUXCODE NOT IN ('X','G','CG') and DT.BOSTATUS IN (0,1)  and  DT.LOGICALREF NOT IN (SELECT Trasnsref FROM Ntl_OfferLineTrasns where isnull(Cancelled,0) = 0 )
                              order by DS.SLIPDATE ";

            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            List<Ntl_Demand> list = sqlConnection.Query<Ntl_Demand>(query).ToList();
            foreach (var item in list)
            {
                Ntl_PersonInfo info = getPersonInfoByDemands(item.DemandNr);
                if (info != null)
                {
                    item.PersonName = info.PersonName;
                    item.PersonEmail = info.PersonEmail;
                }
                else
                {
                    //  item.PersonName = "İsmail Özer";
                    //   item.PersonEmail = "ismail.ozer@netline.net.tr";

                }
            }

            return list;
        }



        public List<Ntl_Demand> getRequirementDemandList()
        {
            string query = $@"  SELECT 
                              DS.LOGICALREF SlipRef,
                              DT.LOGICALREF TransRef,
                              CONVERT(NVARCHAR(50),DS.SLIPNR)  + ' - ' + CONVERT(NVARCHAR(50),DT.LINENR) SlipNr,
                              CONVERT(NVARCHAR(50),DS.SLIPNR)   DemandNr ,
                              DS.SLIPDATE SlipDate,
                              CONVERT(NVARCHAR(50),DS.SLIPDATE,104 )  SlipDateStr,
                              AUX.DESCRIPTION ItemGrpCode,
                              ITM.CODE ItemCode,
                              ITM.DESCRIPTION ItemDesc
                              ,DT.QUANTITY Quantity
                              ,UNIT.CODE Unit
                              ,DOCTRACKINGNR PersonName
                              ,ISNULL(SUB_ANLY.DESCRIPTION,'')  Department,
                              DT.PROCUREDATE ProcureDate ,
                              (select SUM(ONHAND) from V_{FirmNr}_01_MMITEMTOTALS WHERE ITEMREF=ITM.LOGICALREF) StockAmount,      
                              DT.LINEEXP LineExp,
                              '' Usage                  
                              from  U_{FirmNr}_01_DEMANDSLIPS DS
                              LEFT JOIN U_{FirmNr}_01_DEMANDTRANS DT ON DT.DEMANDSLIPREF = DS.LOGICALREF
                              left join U_{FirmNr}_01_DEMANDTRANSADS D_ANLY ON D_ANLY.TRANSREF=DT.LOGICALREF  
                              LEFT JOIN U_{FirmNr}_ANLYDIMENSIONS ANLY  ON ANLY.LOGICALREF=D_ANLY.ANLYDIMREF
							  LEFT JOIN U_{FirmNr}_ANLYDIMENSIONS SUB_ANLY  ON SUB_ANLY.CODE=SUBSTRING(ANLY.CODE,1,10)  
                              LEFT JOIN U_{FirmNr}_UNITS UNIT ON UNIT.LOGICALREF=DT.UOMREF                             
                              LEFT JOIN U_{FirmNr}_ITEMS ITM ON ITM.LOGICALREF =DT.ITEMREF
                              left JOIN U_{FirmNr}_AUXCODES AUX ON AUX.AUXCODE = ITM.GROUPCODE AND CODETYPE=4                              
                              WHERE DS.AUXCODE='X' and DT.BOSTATUS IN (0,1)  and  DT.LOGICALREF NOT IN (SELECt Transref  FROM Ntl_RequirementDemand)
                              order by DS.SLIPDATE ";

            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            List<Ntl_Demand> list = sqlConnection.Query<Ntl_Demand>(query).ToList();


            return list;
        }
        public Ntl_Talep getCompletedRequirementDemand(string DemandNr)
        {
            Ntl_Talep talep = new Ntl_Talep();
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            string query = $@" select top 1 DemandNr ProjeKodu , Date_ from Ntl_RequirementDemand where DemandNr=@DemandNr ";
            talep = sqlConnection.Query<Ntl_Talep>(query, new { DemandNr }).FirstOrDefault();
            sqlConnection = new SqlConnection(logoConStr);
            talep.Lines = sqlConnection.Query<Ntl_Talep_Detay>($@"select distinct TalepMiktar, Miktar, Birim, BirimFiyat, StokMiktari, KritikStokSeviyesi, UrunAciklamasi, AnalizBoyutuAdi  from  Ntl_RequirementDemand where DemandNr=@DemandNr", new { DemandNr }).ToList();


            return talep;
        }
        public List<Ntl_Demand> getRequirementDemandList(Ntl_DemandFilter filter)
        {

            filter.Begdate = Convert.ToDateTime(filter.SlipBegDate);
            filter.Enddate = Convert.ToDateTime(filter.SlipEndDate);

            string query = $@"  SELECT 
                              DS.LOGICALREF SlipRef,
                              DT.LOGICALREF TransRef,
                              CONVERT(NVARCHAR(50),DS.SLIPNR)  + ' - ' + CONVERT(NVARCHAR(50),DT.LINENR) SlipNr,
                              CONVERT(NVARCHAR(50),DS.SLIPNR)   DemandNr ,
                              DS.SLIPDATE SlipDate,
                              CONVERT(NVARCHAR(50),DS.SLIPDATE,104 )  SlipDateStr,
                              AUX.DESCRIPTION ItemGrpCode,
                              ITM.CODE ItemCode,
                              ITM.DESCRIPTION ItemDesc
                              ,DT.QUANTITY Quantity
                              ,UNIT.CODE Unit
                              ,DS.CREATEDBYNAME Person
                              ,ISNULL(SUB_ANLY.DESCRIPTION,'')  Department,
                              DT.PROCUREDATE ProcureDate ,
                              (select SUM(ONHAND) from V_{FirmNr}_01_MMITEMTOTALS WHERE ITEMREF=ITM.LOGICALREF) StockAmount,      
                              DT.LINEEXP LineExp,
                              '' Usage                  
                              from  U_{FirmNr}_01_DEMANDSLIPS DS
                              LEFT JOIN U_{FirmNr}_01_DEMANDTRANS DT ON DT.DEMANDSLIPREF = DS.LOGICALREF
                              left join U_{FirmNr}_01_DEMANDTRANSADS D_ANLY ON D_ANLY.TRANSREF=DT.LOGICALREF  
                              LEFT JOIN U_{FirmNr}_ANLYDIMENSIONS ANLY  ON ANLY.LOGICALREF=D_ANLY.ANLYDIMREF
							  LEFT JOIN U_{FirmNr}_ANLYDIMENSIONS SUB_ANLY  ON SUB_ANLY.CODE=SUBSTRING(ANLY.CODE,1,10)  
                              LEFT JOIN U_{FirmNr}_UNITS UNIT ON UNIT.LOGICALREF=DT.UOMREF                             
                              LEFT JOIN U_{FirmNr}_ITEMS ITM ON ITM.LOGICALREF =DT.ITEMREF
                              left JOIN U_{FirmNr}_AUXCODES AUX ON AUX.AUXCODE = ITM.GROUPCODE AND CODETYPE=4                              
                              WHERE DS.AUXCODE='X' and  DT.BOSTATUS IN (0,1)  and  DT.LOGICALREF NOT IN (SELECT Trasnsref FROM Ntl_OfferLineTrasns where isnull(Cancelled,0) = 0) ";
            if (!string.IsNullOrEmpty(filter.SlipBegDate))
            {
                if (checkForSQLInjection(filter.SlipBegDate))
                {
                    return null;
                }
                query += $@" and DS.SLIPDATE >=   @Begdate ";
            }
            if (!string.IsNullOrEmpty(filter.SlipEndDate))
            {
                if (checkForSQLInjection(filter.SlipEndDate))
                {
                    return null;
                }

                query += $@" and DS.SLIPDATE <=  @Enddate";
            }
            if (!string.IsNullOrEmpty(filter.SlipNr))
            {
                if (checkForSQLInjection(filter.SlipNr))
                {
                    return null;
                }

                query += $@" and CONVERT(NVARCHAR(50),DS.SLIPNR)  + ' - ' + CONVERT(NVARCHAR(50),DT.LINENR) like '%'+@SlipNr+'%'  ";
            }


            if (!string.IsNullOrEmpty(filter.ItemCode))
            {
                if (checkForSQLInjection(filter.ItemCode))
                {
                    return null;
                }
                query += $@" and  ITM.CODE like  '%'+@ItemCode+'%'  ";
            }
            if (!string.IsNullOrEmpty(filter.ItemDesc))
            {
                if (checkForSQLInjection(filter.ItemDesc))
                {
                    return null;
                }

                query += $@" and ITM.DESCRIPTION like  '%'+@ItemDesc+'%'  ";
            }
            if (!string.IsNullOrEmpty(filter.Person))
            {
                if (checkForSQLInjection(filter.Person))
                {
                    return null;
                }

                query += $@" and DS.CREATEDBYNAME  like  '%'+@Person+'%' ";
            }
            if (!string.IsNullOrEmpty(filter.Department))
            {
                if (checkForSQLInjection(filter.Department))
                {
                    return null;
                }

                query += $@" and ISNULL(SUB_ANLY.DESCRIPTION,'') like   '%'+@Department+'%' ";
            }


            query += $@"  order by DS.SLIPDATE ";

            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            List<Ntl_Demand> list = sqlConnection.Query<Ntl_Demand>(query, filter).ToList();

            List<Ntl_Demand> demands = new List<Ntl_Demand>();

            if (filter.ItemGrpCode.Length == 1 && string.IsNullOrEmpty(filter.ItemGrpCode[0]))
            {
                demands = list;
            }
            else if (filter.ItemGrpCode.Length == 0)
            {
                demands = list;
            }
            else
            {

                foreach (var ItemGrp in filter.ItemGrpCode)
                {
                    if (checkForSQLInjection(ItemGrp))
                    {
                        return null;
                    }

                    var tempList = list.Where(x => x.ItemGrpCode == ItemGrp).ToList();
                    foreach (var temp in tempList)
                    {
                        demands.Add(temp);
                    }


                }

            }


            foreach (var item in demands)
            {
                Ntl_PersonInfo info = getPersonInfoByDemands(item.DemandNr);
                if (info != null)
                {
                    item.PersonName = info.PersonName;
                    item.PersonEmail = info.PersonEmail;
                }
                else
                {
                    item.PersonName = "";
                    item.PersonEmail = "";

                }
            }
            return demands;
        }




        public List<Ntl_BrwsrTalep> GetBrwsrTaleps()
        {

            List<Ntl_BrwsrTalep> brwsrTaleps = new List<Ntl_BrwsrTalep>();
            string query = $@" SELECT DemandNr ,COUNT(*) UrunSayisi, avg(Miktar) ToplamSatinAlmaMiktari,convert(date,Date_) Date_ ,sum(dt.QUANTITY) ToplamTalepMiktari
                                FROM Ntl_RequirementDemand nrd 
                                left  join U_001_01_DEMANDTRANS dt on dt.LOGICALREF=nrd.Transref
                                left join U_001_01_DEMANDTRANSADS D_ANLY ON D_ANLY.TRANSREF=DT.LOGICALREF  
                                LEFT JOIN U_001_ANLYDIMENSIONS ANLY  ON ANLY.LOGICALREF=D_ANLY.ANLYDIMREF
                                group by DemandNr,convert(date,Date_),ANLY.DESCRIPTION";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            brwsrTaleps = sqlConnection.Query<Ntl_BrwsrTalep>(query).ToList();

            foreach (var brwsrTalep in brwsrTaleps)
            {
                string DemandNr = brwsrTalep.DemandNr;

                sqlConnection = new SqlConnection(FlowConStr);
                brwsrTalep.CreatedBy = sqlConnection.ExecuteScalar<string>("select top 1 AnalizBoyutuAdi from  Net_Talep_Detay where ProjeKodu=@DemandNr ", new { DemandNr });
            }
            return brwsrTaleps;
        }


        public Ntl_PersonInfo getPersonInfoByDemands(string fisno)
        {
            string query = "SELECT TalepEden PersonName,Email PersonEmail  FROM ButceDetay where FisNo=@fisno";

            SqlConnection sqlConnection = new SqlConnection(BudgetControlConStr);
            return sqlConnection.Query<Ntl_PersonInfo>(query, new { fisno }).FirstOrDefault();


        }
        public Ntl_PersonInfo getPersonAdmin(string personEmail)
        {
            string query = $@"select  ADSOYAD PersonName,EMAIL PersonEmail  from  NET_HIYERARSI_BILGILERI_Purch where HIYERARSI_KODU = (
select SUBSTRING(HIYERARSI_KODU,1,LEN(HIYERARSI_KODU) - CHARINDEX('.', REVERSE(HIYERARSI_KODU))) 
  from NET_HIYERARSI_BILGILERI_Purch  where EMAIL = @personEmail)                  ";

            SqlConnection sqlConnection = new SqlConnection(FlowConStr);
            return sqlConnection.Query<Ntl_PersonInfo>(query, new { personEmail }).FirstOrDefault();


        }
        public List<Ntl_Demand> getDemandList(Ntl_DemandFilter filter)
        {

            filter.Begdate = Convert.ToDateTime(filter.SlipBegDate);
            filter.Enddate = Convert.ToDateTime(filter.SlipEndDate);

            string query = $@"  SELECT 
                              DS.LOGICALREF SlipRef,
                              DT.LOGICALREF TransRef,
                              CONVERT(NVARCHAR(50),DS.SLIPNR)  + ' - ' + CONVERT(NVARCHAR(50),DT.LINENR) SlipNr,
                              CONVERT(NVARCHAR(50),DS.SLIPNR)   DemandNr ,
                              DS.SLIPDATE SlipDate,
                              CONVERT(NVARCHAR(50),DS.SLIPDATE,104 )  SlipDateStr,
                              AUX.DESCRIPTION ItemGrpCode,
                              ITM.CODE ItemCode,
                              ITM.DESCRIPTION ItemDesc
                              ,DT.QUANTITY Quantity
                              ,UNIT.CODE Unit
                              ,(select TOP 1  TalepEden from RAYSIGORTA_BUTCE..ButceDetay  where FisNo=DS.SLIPNR)  PersonName
                              ,ISNULL(SUB_ANLY.DESCRIPTION,'')  Department,
                              DT.PROCUREDATE ProcureDate ,
                              (select SUM(ONHAND) from V_{FirmNr}_01_MMITEMTOTALS WHERE ITEMREF=ITM.LOGICALREF) StockAmount,      
                              DT.LINEEXP LineExp,
                              '' Usage  ,DT.LINENET LineNet                    
                              from  U_{FirmNr}_01_DEMANDSLIPS DS
                              LEFT JOIN U_{FirmNr}_01_DEMANDTRANS DT ON DT.DEMANDSLIPREF = DS.LOGICALREF
                              left join U_{FirmNr}_01_DEMANDTRANSADS D_ANLY ON D_ANLY.TRANSREF=DT.LOGICALREF  
                              LEFT JOIN U_{FirmNr}_ANLYDIMENSIONS ANLY  ON ANLY.LOGICALREF=D_ANLY.ANLYDIMREF
							  LEFT JOIN U_{FirmNr}_ANLYDIMENSIONS SUB_ANLY  ON SUB_ANLY.CODE=SUBSTRING(ANLY.CODE,1,10)  
                              LEFT JOIN U_{FirmNr}_UNITS UNIT ON UNIT.LOGICALREF=DT.UOMREF                             
                              LEFT JOIN U_{FirmNr}_ITEMS ITM ON ITM.LOGICALREF =DT.ITEMREF
                              left JOIN U_{FirmNr}_AUXCODES AUX ON AUX.AUXCODE = ITM.GROUPCODE AND CODETYPE=4                              
                              WHERE  DS.AUXCODE NOT IN ('X','G','CG') and  DT.BOSTATUS IN (0,1)  and  DT.LOGICALREF NOT IN (SELECT Trasnsref FROM Ntl_OfferLineTrasns where isnull(Cancelled,0) = 0) ";
            if (!string.IsNullOrEmpty(filter.SlipBegDate))
            {
                if (checkForSQLInjection(filter.SlipBegDate))
                {
                    return null;
                }

                query += $@" and DS.SLIPDATE >= @Begdate";
            }
            if (!string.IsNullOrEmpty(filter.SlipEndDate))
            {
                if (checkForSQLInjection(filter.SlipEndDate))
                {
                    return null;
                }
                query += $@" and DS.SLIPDATE <= @Enddate";
            }
            if (!string.IsNullOrEmpty(filter.SlipNr))
            {
                if (checkForSQLInjection(filter.SlipNr))
                {
                    return null;
                }

                query += $@" and CONVERT(NVARCHAR(50),DS.SLIPNR)  + ' - ' + CONVERT(NVARCHAR(50),DT.LINENR) like '%'+@SlipNr+'%'";
            }



            if (!string.IsNullOrEmpty(filter.ItemCode))
            {

                if (checkForSQLInjection(filter.ItemCode))
                {
                    return null;
                }
                query += $@" and  ITM.CODE like '%'+@ItemCode+'%'";
            }
            if (!string.IsNullOrEmpty(filter.ItemDesc))
            {
                if (checkForSQLInjection(filter.ItemDesc))
                {
                    return null;
                }

                query += $@" and ITM.DESCRIPTION like '%'+@ItemDesc+'%'";
            }

            if (!string.IsNullOrEmpty(filter.Department))
            {

                if (checkForSQLInjection(filter.Department))
                {
                    return null;
                }

                query += $@" and ISNULL(SUB_ANLY.DESCRIPTION,'')  like   '%'+@Department+'%'  ";
            }
            if (!string.IsNullOrEmpty(filter.Person))
            {
                if (checkForSQLInjection(filter.Person))
                {
                    return null;
                }

                query += $@" and   ISNULL(ANLY.DESCRIPTION,'')  like  '%'+@Person+'%'  ";
            }

            query += $@"  order by DS.SLIPDATE ";

            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            List<Ntl_Demand> list = sqlConnection.Query<Ntl_Demand>(query, filter).ToList();

            List<Ntl_Demand> demands = new List<Ntl_Demand>();

            if (filter.ItemGrpCode.Length == 1 && string.IsNullOrEmpty(filter.ItemGrpCode[0]))
            {
                demands = list;
            }
            else if (filter.ItemGrpCode.Length == 0)
            {
                demands = list;
            }
            else
            {

                foreach (var ItemGrp in filter.ItemGrpCode)
                {
                    if (checkForSQLInjection(ItemGrp))
                    {
                        return null;
                    }

                    var tempList = list.Where(x => x.ItemGrpCode == ItemGrp).ToList();
                    foreach (var temp in tempList)
                    {
                        demands.Add(temp);
                    }


                }

            }




            foreach (var item in demands)
            {



                Ntl_PersonInfo info = getPersonInfoByDemands(item.DemandNr);
                if (info != null)
                {
                    item.PersonName = info.PersonName;
                    item.PersonEmail = info.PersonEmail;
                }
                else
                {
                    // item.PersonName = "İsmail Özer";
                    // item.PersonEmail = "ismail.ozer@netline.net.tr";

                }
            }
            return demands;
        }

        public List<Ntl_OfferLine> getOfferLines(string transRefs)
        {
            string query = $@" SELECT ITM.LOGICALREF ItemRef ,DT.LOGICALREF  TransRef_ ,
                                AUX.DESCRIPTION ItemGrpCode
								,CONVERT(NVARCHAR(50),DS.SLIPNR)   SlipNr ,
                                 ITM.CODE ItemCode,
                                 ITM.DESCRIPTION ItemDesc
                                 ,SUM(DT.QUANTITY) Quantity
                                 ,UNIT.CODE Unit
                                 ,CASE WHEN (SELECT TOP 1 TRANSDATE FROM U_001_01_MMTRANS MMT WHERE SLIPTYPE=1 AND ITEMREF=ITM.LOGICALREF order by MMT.TRANSDATE DESC ) > PRC.BEGDATE 
                                 THEN (SELECT TOP 1 case when INCVAT =0 THEN PRICE ELSE  PRICE / (1 + (ITM.VATRATE/100.00)) END  FROM U_001_01_MMTRANS MMT WHERE SLIPTYPE=1 AND ITEMREF=ITM.LOGICALREF order by MMT.TRANSDATE DESC ) ELSE case when INCVAT =0 THEN PRC.PRICE ELSE  PRC.PRICE / (1 + (ITM.VATRATE/100.00)) END END LastPurchPrice
                                 ,ITM.VATRATE  VatRate
                                 ,CONVERT(BIT, ISNULL(DS.AUTHCODE,'0') ) isContract   
								 ,AUX.DESCRIPTION ItemGrpCode
								 ,(select TOP 1  TalepEden from RAYSIGORTA_BUTCE..ButceDetay  where FisNo=DS.SLIPNR)   PersonName
                                 from  U_001_01_DEMANDSLIPS DS
                                 LEFT JOIN U_001_01_DEMANDTRANS DT ON DT.DEMANDSLIPREF = DS.LOGICALREF
								 left join U_001_01_DEMANDTRANSADS D_ANLY ON D_ANLY.TRANSREF=DT.LOGICALREF  
							     LEFT JOIN U_001_ANLYDIMENSIONS ANLY  ON ANLY.LOGICALREF=D_ANLY.ANLYDIMREF
							     LEFT JOIN U_001_ANLYDIMENSIONS SUB_ANLY  ON SUB_ANLY.CODE=SUBSTRING(ANLY.CODE,1,10)  
                                 LEFT JOIN U_001_UNITS UNIT ON UNIT.LOGICALREF=DT.UOMREF 
                                 LEFT JOIN U_001_ITEMS ITM ON ITM.LOGICALREF =DT.ITEMREF
                                 left JOIN U_001_PRICES PRC ON PRC.ITEMREF=ITM.LOGICALREF AND PRICETYPE=1
                                 left JOIN U_001_AUXCODES AUX ON AUX.AUXCODE = ITM.GROUPCODE AND CODETYPE=4
                                 WHERE DT.LOGICALREF IN ({transRefs})
                                 GROUP BY  AUX.DESCRIPTION ,ITM.CODE,UNIT.CODE,ITM.DESCRIPTION,ITM.LOGICALREF,ITM.VATRATE  ,DS.AUTHCODE,AUX.DESCRIPTION,ANLY.DESCRIPTION,DS.SLIPNR ,PRC.BEGDATE,PRC.PRICE,PRC.INCVAT,DT.LOGICALREF  ";

            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            List<Ntl_OfferLine> list = sqlConnection.Query<Ntl_OfferLine>(query).ToList();


            foreach (var item in list)
            {
                item.TransRef = getOfferTransRefList(item.TransRef_, item.ItemRef);
            }

            return list;
        }


        public List<Ntl_Talep_Detay> getDemandLines(string transRefs)
        {
            string query = $@" SELECT  ITM.LOGICALREF Stockref ,
                                AUX.DESCRIPTION UrunGrupKodu,
                                 ITM.CODE UrunKodu,
                                 ITM.DESCRIPTION UrunAciklamasi
								 ,ITM.AUXCODE2 UrunTuru
                                 ,sum(DT.QUANTITY) TalepMiktar
								 ,0 Miktar
                                 ,UNIT.CODE Birim
                                 ,0 AnalizBoyutuRef
								 ,'' AnalizBoyutuKodu
								 ,'' AnalizBoyutuAdi
								 ,DT.AUXCODE Acente
								 ,US.LOGICALREF Unitsetref
								 ,UNIT.LOGICALREF Uomref
								 ,DT.UINFO1 Uinfo1
								 ,DT.UINFO2 Uinfo2
								 ,US.DESCRIPTION Uomsetaciklama
								 ,ITM.CARDTYPE KartTipi
								 ,ITM.DESCRIPTION4 MuhKod
                                 from  U_001_01_DEMANDSLIPS DS
                                 LEFT JOIN U_001_01_DEMANDTRANS DT ON DT.DEMANDSLIPREF = DS.LOGICALREF							
								 
                                 LEFT JOIN U_001_UNITS UNIT ON UNIT.LOGICALREF=DT.UOMREF 
								 LEFT JOIN U_001_UNITSETS US ON US.LOGICALREF=UNIT.UOMSETREF
                                 LEFT JOIN U_001_ITEMS ITM ON ITM.LOGICALREF =DT.ITEMREF
                                 left JOIN U_001_AUXCODES AUX ON AUX.AUXCODE = ITM.GROUPCODE AND CODETYPE=4
                                 WHERE DT.LOGICALREF IN ({transRefs})
								 GROUP BY ITM.LOGICALREF ,AUX.DESCRIPTION,ITM.DESCRIPTION,ITM.CODE,UNIT.CODE	
							     ,ITM.AUXCODE2,UNIT.LOGICALREF   
                                 ,DT.AUXCODE,US.LOGICALREF,US.LOGICALREF
								 ,DT.UINFO1,DT.UINFO2,DT.UINFO2,US.DESCRIPTION,ITM.CARDTYPE ,ITM.DESCRIPTION4	";

            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            List<Ntl_Talep_Detay> list = sqlConnection.Query<Ntl_Talep_Detay>(query).ToList();
            foreach (var item in list)
            {
                int stockref = item.Stockref;

                query = "SELECT  FIILI_STOK  StokMiktari ,TANIMLI_FIYAT BirimFiyat,KRITIK_STOK KritikStokSeviyesi FROM NET_FLOW_URUN_BILGILERI  where REF=@stockref";
                sqlConnection = new SqlConnection(logoConStr);
                Ntl_Talep_DetayStockInfo itemInfo = sqlConnection.Query<Ntl_Talep_DetayStockInfo>(query, new { stockref }).FirstOrDefault();
                item.StokMiktari = itemInfo.StokMiktari;
                item.BirimFiyat = item.BirimFiyat;
                item.KritikStokSeviyesi = item.KritikStokSeviyesi;
                item.Transrefs = getDemanTransRefList(transRefs, item.Stockref);


            }




            return list;
        }

        public Ntl_Talep_Detay getDemandLine(string transRef)
        {
            string query = $@" SELECT  ITM.LOGICALREF Stockref ,
                                AUX.DESCRIPTION UrunGrupKodu,
                                 ITM.CODE UrunKodu,
                                 ITM.DESCRIPTION UrunAciklamasi
								 ,ITM.AUXCODE2 UrunTuru
                                 ,sum(DT.QUANTITY) TalepMiktar
								 ,0 Miktar
                                 ,UNIT.CODE Birim
                                 ,SUB_ANLY.LOGICALREF AnalizBoyutuRef
								 ,SUB_ANLY.CODE AnalizBoyutuKodu
								 ,SUB_ANLY.DESCRIPTION AnalizBoyutuAdi
								 ,DT.AUXCODE Acente
								 ,US.LOGICALREF Unitsetref
								 ,UNIT.LOGICALREF Uomref
								 ,DT.UINFO1 Uinfo1
								 ,DT.UINFO2 Uinfo2
								 ,US.DESCRIPTION Uomsetaciklama
								 ,ITM.CARDTYPE KartTipi
								 ,ITM.DESCRIPTION4 MuhKod
                                 from  U_001_01_DEMANDSLIPS DS
                                 LEFT JOIN U_001_01_DEMANDTRANS DT ON DT.DEMANDSLIPREF = DS.LOGICALREF
								 left join U_001_01_DEMANDTRANSADS D_ANLY ON D_ANLY.TRANSREF=DT.LOGICALREF 
								 LEFT JOIN U_001_ANLYDIMENSIONS ANLY  ON ANLY.LOGICALREF=D_ANLY.ANLYDIMREF
							     LEFT JOIN U_001_ANLYDIMENSIONS SUB_ANLY  ON SUB_ANLY.CODE=SUBSTRING(ANLY.CODE,1,10)  
                                 LEFT JOIN U_001_UNITS UNIT ON UNIT.LOGICALREF=DT.UOMREF 
								 LEFT JOIN U_001_UNITSETS US ON US.LOGICALREF=UNIT.UOMSETREF
                                 LEFT JOIN U_001_ITEMS ITM ON ITM.LOGICALREF =DT.ITEMREF
                                 left JOIN U_001_AUXCODES AUX ON AUX.AUXCODE = ITM.GROUPCODE AND CODETYPE=4
                                  WHERE DT.LOGICALREF IN ({transRef})
								 GROUP BY ITM.LOGICALREF ,AUX.DESCRIPTION,ITM.DESCRIPTION,ITM.CODE,UNIT.CODE	
							     ,ITM.AUXCODE2,SUB_ANLY.LOGICALREF,UNIT.LOGICALREF   
                                 ,SUB_ANLY.CODE,SUB_ANLY.DESCRIPTION,DT.AUXCODE,US.LOGICALREF,US.LOGICALREF
								 ,DT.UINFO1,DT.UINFO2,DT.UINFO2,US.DESCRIPTION,ITM.CARDTYPE ,ITM.DESCRIPTION4	";

            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            Ntl_Talep_Detay item = sqlConnection.Query<Ntl_Talep_Detay>(query).FirstOrDefault();
            int stockref = item.Stockref;
            query = "SELECT  FIILI_STOK  StokMiktari ,TANIMLI_FIYAT BirimFiyat,KRITIK_STOK KritikStokSeviyesi FROM NET_FLOW_URUN_BILGILERI  where REF=@stockref";
            sqlConnection = new SqlConnection(logoConStr);
            Ntl_Talep_DetayStockInfo itemInfo = sqlConnection.Query<Ntl_Talep_DetayStockInfo>(query, new { stockref }).FirstOrDefault();
            item.StokMiktari = itemInfo.StokMiktari;
            item.BirimFiyat = item.BirimFiyat;
            item.KritikStokSeviyesi = item.KritikStokSeviyesi;
            item.Transrefs = getDemanTransRefList(transRef, item.Stockref);

            return item;
        }

        public int createPurchFlow(int projeId, string projeKodu)
        {
            string query = $@" INSERT INTO Net_Talep_Fis  (ProjeId ,ProjeKodu)
                    VALUES  ( @projeId , @projeKodu )";
            SqlConnection sqlConnection = new SqlConnection(FlowConStr);
            int created = sqlConnection.Execute(query, new { projeId, projeKodu });


            return created;
        }


        public int createPurchFlowLines(Ntl_Talep_Detay talep, int ProjeId)
        {
            string query = $@" ";



            query = $@" INSERT INTO Net_Talep_Detay  (ProjeId, ProjeKodu  ,UrunGrupKodu  ,UrunTuru  ,UrunKodu  ,UrunAciklamasi  
                    ,Miktar  ,Birim  ,BirimFiyat  ,Tutar  ,KullanimYeri  ,TalepEdenKisi  ,HediyePromosyonTipi  
                    ,AnalizBoyutuRef  ,AnalizBoyutuKodu  ,AnalizBoyutuAdi  ,Acente  ,Unitsetref  ,Uomref  ,Uinfo1  ,Uinfo2  
                    ,StokMiktari  ,KritikStokSeviyesi  ,Stockref  ,Uomsetaciklama  ,KartTipi  ,MuhKod)
                    VALUES  ( {ProjeId} ,@ProjeKodu  ,@UrunGrupKodu  ,@UrunTuru  ,@UrunKodu  ,@UrunAciklamasi  
                    ,@Miktar  ,@Birim  ,@BirimFiyat  ,@Tutar  ,@KullanimYeri  ,@TalepEdenKisi  ,@HediyePromosyonTipi  
                    ,@AnalizBoyutuRef  ,@AnalizBoyutuKodu  ,@AnalizBoyutuAdi  ,@Acente  ,@Unitsetref  ,@Uomref  ,@Uinfo1  ,@Uinfo2  
                    ,@StokMiktari  ,@KritikStokSeviyesi  ,@Stockref  ,@Uomsetaciklama  ,@KartTipi  ,@MuhKod)";
            SqlConnection sqlConnection = new SqlConnection(FlowConStr);
            int created = sqlConnection.Execute(query, talep);
            if (created > 0)
            {
                foreach (var transref in talep.Transrefs)
                {


                    query = $@" INSERT INTO Ntl_RequirementDemand
                                 (Transref
                                 ,DemandNr
                                 ,TalepMiktar
                                 ,Date_
                                 ,Miktar
                                 ,Birim
                                 ,BirimFiyat
                                 ,StokMiktari
                                 ,KritikStokSeviyesi
                                 ,UrunAciklamasi
                                 ,AnalizBoyutuAdi)
                         VALUES
                               ({transref}
                               ,'{talep.ProjeKodu}'
                               ,{talep.TalepMiktar.ToString().Replace(',', '.')} 
                               , GETDATE() 
                               ,{talep.Miktar.ToString().Replace(',', '.')}
                               ,'{talep.Birim}'
                               ,{talep.BirimFiyat.ToString().Replace(',', '.')}
                               ,{talep.StokMiktari.ToString().Replace(',', '.')}
                               ,{talep.KritikStokSeviyesi.ToString().Replace(',', '.')}
                               ,'{talep.UrunAciklamasi}'
                               ,'{talep.AnalizBoyutuAdi}' )";
                    sqlConnection = new SqlConnection(logoConStr);
                    created = sqlConnection.Execute(query, talep);
                }

            }


            return created;
        }


        public List<int> getDemanTransRefList(string transRefs, int itemRef)
        {
            string query = $@" select DT.LOGICALREF  from U_001_01_DEMANDTRANS DT 
								 
								 WHERE   DT.LOGICALREF IN ({transRefs}) AND  DT.ITEMREF ={itemRef} ";

            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            List<int> list = sqlConnection.Query<int>(query).ToList();
            return list;
        }

        public List<int> getOfferTransRefList(int transRefs, int itemRef)
        {
            string query = $@" SELECT LOGICALREF   from  U_{FirmNr}_01_DEMANDTRANS DT   WHERE DT.LOGICALREF IN ({transRefs}) AND  DT.ITEMREF ={itemRef} ";

            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            List<int> list = sqlConnection.Query<int>(query).ToList();
            return list;
        }
        public List<string> getGroupList()
        {
            string query = $@" SELECT DESCRIPTION   from  U_{FirmNr}_AUXCODES AUX where  CODETYPE=4 ";

            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            List<string> list = sqlConnection.Query<string>(query).ToList();
            return list;
        }

        public List<string> getDemandSlips(int ProjectId)
        {
            string query = $@" SELECT SLIPNR   FROM[RAYJAVA].[dbo].[Ntl_OfferLine] ofl
                             left join Ntl_OfferLineTrasns oft on oft.OfferLineId=ofl.Id
                             left join U_{FirmNr}_01_DEMANDTRANS DT on DT.LOGICALREF= oft.Trasnsref
                             left join U_{FirmNr}_01_DEMANDSLIPS DS on DS.LOGICALREF=DT.DEMANDSLIPREF
                             where ProjectId=@ProjectId  ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            List<string> list = sqlConnection.Query<string>(query, new { ProjectId }).ToList();
            return list;
        }



        public List<Ntl_OfferSupplier> getSuppliers(string SupplierCode, string SupplierDesc)
        {
            string query = $@"   select TOP 10 LOGICALREF SupplierRef ,CODE SupplierCode , DESCRIPTION   SupplierDesc ,TAXNR  TaxNr from U_{FirmNr}_ARPS where CODE NOT LIKE 'TRP%' AND  CARDTYPE=3 ";
            if (!string.IsNullOrEmpty(SupplierCode))
            {
                if (checkForSQLInjection(SupplierCode))
                {
                    return new List<Ntl_OfferSupplier>();
                }

                query += $@" AND CODE LIKE  '%'+@SupplierCode+'%'  ";
            }
            if (!string.IsNullOrEmpty(SupplierDesc))
            {
                if (checkForSQLInjection(SupplierDesc))
                {
                    return new List<Ntl_OfferSupplier>();
                }

                query += $@" AND DESCRIPTION LIKE   '%'+@SupplierDesc+'%' ";
            }
            query += $@" ORDER BY DESCRIPTION ";

            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            List<Ntl_OfferSupplier> list = sqlConnection.Query<Ntl_OfferSupplier>(query, new { SupplierDesc, SupplierCode }).ToList();
            return list;
        }

        public Ntl_OfferSupplier getSupplierByRef(int SupplierRef)
        {
            string query = $@"   select TOP 10 LOGICALREF SupplierRef ,CODE SupplierCode , DESCRIPTION SupplierDesc ,TAXNR  TaxNr    from U_{FirmNr}_ARPS where CARDTYPE=3 and LOGICALREF=@SupplierRef ";


            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            Ntl_OfferSupplier supplier = sqlConnection.Query<Ntl_OfferSupplier>(query, new { SupplierRef }).FirstOrDefault();
            return supplier;
        }

        public List<int> getSupplierRefListByOrderId(int projectId)
        {
            string query = $@" SELECT distinct SupplierRef FROM Ntl_OfferSupplier where ProjectId=@projectId ";


            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            List<int> supplier = sqlConnection.Query<int>(query, new { projectId }).ToList();
            return supplier;
        }

        public int getNextProjectNr()
        {

            SqlConnection sql = new SqlConnection(logoConStr);
            string query = $@"  SELECT  MAX(CONVERT(int, substring(ProjectNr,5,5))) FROM Ntl_Offer  where isnull(Cancelled,0)=0 ";
            return sql.ExecuteScalar<int>(query);
        }

        public int getItemCardType(int ItemRef)
        {

            SqlConnection sql = new SqlConnection(logoConStr);
            string query = $@"  select CARDTYPE FROM U_001_ITEMS WHERE LOGICALREF =@ItemRef";
            return sql.ExecuteScalar<int>(query, new { ItemRef });
        }

        public Ntl_OfferRequest getRequestGuid(int SupplierRef, int ProjectId)
        {

            SqlConnection sql = new SqlConnection(logoConStr);
            string query = $@" SELECT * FROM Ntl_OfferRequest where Responded=0 and  isnull(Closed,0)=0 and SupplierRef=@SupplierRef and ProjectId=@ProjectId ";
            return sql.Query<Ntl_OfferRequest>(query, new { SupplierRef, ProjectId }).FirstOrDefault();
        }


        public int getNextTalepProjectNr()
        {

            SqlConnection sql = new SqlConnection(FlowConStr);
            string query = $@"select  isnull(( SELECT  MAX(CONVERT(int, substring(ProjeKodu,5,15))) FROM Net_Talep_Fis),0) ";
            return sql.ExecuteScalar<int>(query);
        }

        public string getProjectExp(int projectId)
        {

            SqlConnection sql = new SqlConnection(logoConStr);
            string query = $@"  SELECT SuggestionExplanation  FROM Ntl_Offer where ProjectId=@projectId ";
            return sql.ExecuteScalar<string>(query, new { projectId });
        }
        public string getNextOrderNr()
        {

            SqlConnection sql = new SqlConnection(logoConStr);
            string query = $@"  SELECT ISNULL((SELECT  MAX(CONVERT(int, substring(SLIPNR,5,12))) FROM U_001_01_ORDERSLIPS WHERE SLIPTYPE=1 AND SLIPNR LIKE 'RAY-%'),0) ";
            int lastNr = sql.ExecuteScalar<int>(query);
            string LastSlipNr = "RAY-" + (lastNr + 1).ToString().PadLeft(12, '0');
            return LastSlipNr;
        }
        public int saveOffer(Ntl_Offer offer, int UserId)
        {
            string query = $@"  if exists (select * from Ntl_Offer  WHERE ProjectId = @ProjectId)
            begin 
            UPDATE dbo.Ntl_Offer
               SET ProjectNr = @ProjectNr
                  ,EndDate = @EndDate
                  ,DeliveryAddress = @DeliveryAddress
                  ,isDeliveryAddress = @isDeliveryAddress
                  ,Explanation = @Explanation   ,EndTime=@EndTime,OfferDate=@OfferDate
            WHERE ProjectId = @ProjectId
            end 
            else
            begin
            INSERT INTO dbo.Ntl_Offer
           (ProjectNr
           ,EndDate
           ,DeliveryAddress
           ,isDeliveryAddress
           ,Explanation ,EndTime ,OfferDate,UserId)
            VALUES
           (@ProjectNr
           ,@EndDate
           ,@DeliveryAddress
           ,@isDeliveryAddress
           ,@Explanation ,@EndTime ,@OfferDate,{UserId}) SELECT SCOPE_IDENTITY() as ProjectId  end";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            int prjId = sqlConnection.ExecuteScalar<int>(query, offer);
            if (offer.ProjectId > 0)
            {
                return offer.ProjectId;
            }
            else
            {
                return prjId;
            }


        }

        public int UpdateOfferStatus(int ProjectId, int orderSlipref)
        {
            string query = $@" update Ntl_Offer set IsSend=1 ,SendOn=GETDATE(), OrderSlipRef=@orderSlipref where ProjectId=@ProjectId ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Execute(query, new { orderSlipref, ProjectId });
        }

        public int insertInvoiceBudget(int orderSlipref)
        {
            string query = $@" INSERT INTO [dbo].[X_001_01_CT_BUDGET]
                                 ([LOGICALREF]
                                 ,[PARENTREF]
                                 ,[DEMANDNR]
                                 ,[BUDGET]
                                 ,[MONTH_]
                                 ,[AMOUNT]
                                 ,[BUDGETID]
                                 ,[INVOICEREF]
                                 ,[ORDERREF]   
                                 ,[TE_RECSTATUS]
                                 ,[TE_WPIID]
                                 ,[TE_WFIID]
                                 ,[TE_RIGHTS])
                                    SELECT NEXT VALUE FOR RAYJAVA.dbo.X_001_01_CT_BUDGETSEQ  , * FROM (
                                      select distinct                                       
                                    0 ZZ, DS.SLIPNR 
                                    , GL.DESCRIPTION 
                                    ,CASE MONTH(BD.Tarih)
                                     WHEN 1 THEN 'OCAK'
                                     WHEN 2 THEN 'ŞUBAT'
                                     WHEN 3 THEN 'MART'
                                     WHEN 4 THEN 'NİSAN'
                                     WHEN 5 THEN 'MAYIS'
                                     WHEN 6 THEN 'HAZİRAN'
                                     WHEN 7 THEN 'TEMMUZ'
                                     WHEN 8 THEN 'AĞUSTOS'
                                     WHEN 9 THEN 'EYLÜL'
                                     WHEN 10 THEN 'EKİM'
                                     WHEN 11 THEN 'KASIM'
                                     WHEN 12 THEN 'ARALIK' END MONTH_
                                     , BD.BlokeTutari ,bd.Id, 0 GG,ORD.LOGICALREF,  1 AA,1 BB,1 CC,NULL DD
                                    
                                    FROM  U_001_01_ORDERSLIPS ORD 
                                    LEFT JOIN U_001_01_ORDERTRANS ORDT ON ORD.LOGICALREF=ORDT.ORDSLIPREF 
                                    LEFT JOIN Ntl_Offer o on o.OrderSlipRef=ORD.LOGICALREF
                                    left JOIN Ntl_OfferLine ol on ol.ProjectId=o.ProjectId and ol.ItemRef=ORDT.ITEMREF
                                    left join Ntl_OfferLineTrasns olt on olt.OfferLineId=ol.Id
                                    LEFT join U_001_01_DEMANDTRANS DT ON DT.LOGICALREF=olt.Trasnsref
                                    left join U_001_01_DEMANDSLIPS ds on ds.LOGICALREF=DT.DEMANDSLIPREF
                                    LEFT JOIN RAYSIGORTA_BUTCE..ButceDetay bd on bd.FisNo=ds.SLIPNR
									INNER JOIN U_001_GLACCOUNTS GL ON REPLACE(GL.CODE,'.','')=bd.ButceKodu
                                    WHERE  DS.SLIPNR is not null  AND	ORD.LOGICALREF=@orderSlipref) AS TBL ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Execute(query, new { orderSlipref });
        }




        public int saveOfferLine(Ntl_OfferLine offerline, int ProjectId)
        {
            string transrefs = "";
            foreach (var TransRef in offerline.TransRef)
            {
                transrefs += "," + TransRef;
            }
            transrefs = transrefs.Substring(1);

            string query = $@"    INSERT INTO dbo.Ntl_OfferLine
           (ProjectId
           , ItemRef
           , ItemCode
           , ItemDesc
           , Quantity
           , Unit
           , LastPurchPrice
           , Total
           , VatRate
           , NetTotal, isContract)
     VALUES
           ( {ProjectId}
           , @ItemRef
           , @ItemCode
           , @ItemDesc
           , (select SUM(QUANTITY) from U_001_01_DEMANDTRANS WHERE LOGICALREF IN ( {transrefs}) )
           , @Unit
           , @LastPurchPrice
           , @Total
           , @VatRate
           , @NetTotal , @isContract) SELECT SCOPE_IDENTITY() as Id ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<int>(query, offerline);

        }


        public int saveLineTrans(int transRef, int lineId)
        {

            string query = $@"  INSERT INTO dbo.Ntl_OfferLineTrasns
           (OfferLineId
           , Trasnsref,Cancelled)
     VALUES
           (@lineId
           , @transRef ,0 ) SELECT SCOPE_IDENTITY() as LineId ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);

            return sqlConnection.ExecuteScalar<int>(query, new { transRef, lineId });

        }

        public bool transRefControl(int transref)
        {

            string query = $@" select count(*) from Ntl_OfferLineTrasns where Cancelled=0 and Trasnsref=@transref ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            if (sqlConnection.ExecuteScalar<int>(query, new { transref }) > 0)
                return true;
            else
                return false;
        }
        public int transRefProjectId(int transref)
        {

            string query = $@" select ProjectId from Ntl_OfferLine where Id in (
                                select OfferLineId from Ntl_OfferLineTrasns where Cancelled =0 and Trasnsref=@transref ) ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<int>(query, new { transref });

        }




        public int saveOfferSupplier(Ntl_OfferSupplier offerSupp, int ProjectId)
        {

            string query = $@" if exists (select * from Ntl_OfferSupplier  WHERE ProjectId = {ProjectId} and SupplierRef=@SupplierRef)
            begin 
            UPDATE dbo.Ntl_OfferSupplier
               SET SupplierRef = SupplierRef
                   ,SupplierCode = SupplierCode
                   ,SupplierDesc = SupplierDesc
           WHERE ProjectId = {ProjectId} and SupplierRef=@SupplierRef
            end
            else
            begin
            INSERT INTO dbo.Ntl_OfferSupplier
           (ProjectId
           ,SupplierRef
           ,SupplierCode
           ,SupplierDesc)
            VALUES
           ({ProjectId}
           ,@SupplierRef
           ,@SupplierCode
           ,@SupplierDesc) SELECT SCOPE_IDENTITY() as Id  end";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<int>(query, offerSupp);
        }
        public int saveOfferDoc(Ntl_OfferDocs offerDocs)
        {

            string query = $@" INSERT INTO dbo.Ntl_OfferDocs
           (ProjectId
           ,UploadedFileName
           ,UploadedFileContentTyp
           ,UploadedFile)
           VALUES
           (@ProjectId
           ,@UploadedFileName
           ,@UploadedFileContentTyp
           ,@UploadedFile) ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<int>(query, offerDocs);
        }
        public int UpdateOfferDoc(int Id, string DocumentName)
        {
            string query = $@" update Ntl_OfferDocs set DocumentName=@DocumentName where Id=@Id ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Execute(query, new { DocumentName, Id });
        }

        public int UpdateOrderDoc(int Id, string DocumentName)
        {
            string query = $@" update Ntl_OrderDocs set DocumentName=@DocumentName where Id=@Id ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Execute(query, new { DocumentName, Id });
        }
        public int DeleteOfferDoc(int Id)
        {
            string query = $@" delete from Ntl_OfferDocs  where Id=@Id ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Execute(query, new { Id });
        }
        public int DeleteOrderDoc(int Id)
        {
            string query = $@" delete from Ntl_OrderDocs  where Id=@Id ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Execute(query, new { Id });
        }
        public int saveSupplierOffer(int ProjectId, int LineId, int SupplierRef, double Price, string Explanation, string RequestGuid, int Trcurr, double VatRate, double TrRate)
        {
            string query = $@" INSERT INTO Ntl_SupplierOffer  (ProjectId , LineId , SupplierRef , Price , OfferTime ,Explanation ,RequestGuid ,TrCurr ,VatRate , TrRate)
                             VALUES  (@ProjectId , @LineId , @SupplierRef , @Price ,getdate() , @Explanation  ,@RequestGuid ,@TrCurr ,@VatRate ,@TrRate)";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<int>(query, new { ProjectId, LineId, SupplierRef, Price, Explanation, RequestGuid, Trcurr, VatRate, TrRate });
        }


        public int saveProjectDocs(int ProjectId, string UploadedFileName, string UploadedFileContentTyp, string UploadedFilePath)
        {
            string query = $@" if not exists (select * from Ntl_OfferDocs  WHERE ProjectId = @ProjectId and UploadedFileName=@UploadedFileName)
begin 
INSERT INTO Ntl_OfferDocs
           (ProjectId
           ,UploadedFileName
           ,UploadedFileContentTyp
           ,UploadedFilePath)
     VALUES
           (@ProjectId
           ,@UploadedFileName
           ,@UploadedFileContentTyp
           ,@UploadedFilePath)  end  ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Execute(query, new { ProjectId, UploadedFileName, UploadedFileContentTyp, UploadedFilePath });
        }
        public int saveOrderDocs(int ProjectId, string UploadedFileName, string UploadedFileContentTyp, string UploadedFilePath)
        {
            string query = $@" if not exists (select * from Ntl_OrderDocs  WHERE ProjectId = @ProjectId and UploadedFileName=@UploadedFileName)
begin 
INSERT INTO Ntl_OrderDocs
           (ProjectId
           ,UploadedFileName
           ,UploadedFileContentTyp
           ,UploadedFilePath)
     VALUES
           (@ProjectId
           ,@UploadedFileName
           ,@UploadedFileContentTyp
           ,@UploadedFilePath)  end  ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Execute(query, new { ProjectId, UploadedFileName, UploadedFileContentTyp, UploadedFilePath });
        }
        public int saveSupplierRequest(int ProjectId, int SupplierRef, Guid RequestGuid, int requestNr)
        {

            string query = $@"     INSERT INTO Ntl_OfferRequest
           (SupplierRef
           , RequestDate_
           , ProjectId
           , Responded
           , RequestGuid
           , RequestNr )
     VALUES
           (@SupplierRef
           ,getdate()
           , @ProjectId
           , 0
           , @RequestGuid
           ,@requestNr )";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            sqlConnection.ExecuteScalar<int>(query, new { ProjectId, SupplierRef, RequestGuid, requestNr });

            sqlConnection = new SqlConnection(logoConStr);
            query = "update Ntl_OfferRequest set Closed=1 where ProjectId=@ProjectId and RequestNr<@requestNr and SupplierRef=@SupplierRef";
            sqlConnection.Execute(query, new { ProjectId, requestNr, SupplierRef });

            return 1;
        }
        public int getNextRequestNr(int ProjectId)
        {

            string query = $@"  select isnull((select max(RequestNr) from Ntl_OfferRequest where  ProjectId=@ProjectId  ) ,0)+1 ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<int>(query, new { ProjectId });
        }
        public int getNextRequestNr(int ProjectId, int supplierRef)
        {

            string query = $@"  select isnull((select max(RequestNr) from Ntl_OfferRequest where  ProjectId=@ProjectId  and SupplierRef=@supplierRef) ,0)+1 ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<int>(query, new { ProjectId, supplierRef });
        }
        public int updateRequest(string requestId, string explain, int responded)
        {

            string query = $@"  update Ntl_OfferRequest set Responded=@responded,ResponseDate_=Getdate() ,Explain=@explain where RequestGuid=@requestId";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Execute(query, new { requestId, explain, responded });

        }
        public int updateProjectTrRate(int projectId, double eur, double usd)
        {
            SqlConnection sqlConnection = new SqlConnection(logoConStr);

            string query = $@"  select SUM(_count) from
                                 (
                                 select COUNT(*) _count from  Ntl_Comment where ProjectId=@projectId
                                 union all
                                 select COUNT(*) _count from  Ntl_Confirm where ProjectId=@projectId
                                 ) as Tbl";

            int CountConfirm = sqlConnection.ExecuteScalar<int>(query, new { projectId });

            if (CountConfirm == 0)
            {

                query = $@"  update Ntl_Offer set UsdCurr=@usd,EurCurr=@eur where ProjectId=@projectId";
                sqlConnection = new SqlConnection(logoConStr);
                return sqlConnection.Execute(query, new { projectId, eur, usd });
            }
            else
            {
                return 0;
            }

        }
        public List<Ntl_OfferLine> getProjectLines(int projectId)
        {


            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            string query = $@" select  ol.Id      ,ol.ProjectId      ,ol.ItemRef      ,ol.ItemCode      ,ol.ItemDesc      ,ol.Quantity
                                        ,ol.Unit      ,ol.LastPurchPrice      ,ol.LastPurchPrice*ol.Quantity Total      ,ol.VatRate
                                        ,ol.LastPurchPrice*Quantity*(1+(ol.VatRate/100))  NetTotal      ,ol.Cancelled      ,ol.isContract
                                        ,AUX.DESCRIPTION ItemGrpCode
                        ,CONVERT(NVARCHAR(50),DS.SLIPNR)   SlipNr 
                        ,AUX.DESCRIPTION ItemGrpCode
                        ,(select TOP 1  TalepEden from RAYSIGORTA_BUTCE..ButceDetay  where FisNo=DS.SLIPNR)   PersonName
                         from Ntl_OfferLine ol
                        left join Ntl_OfferLineTrasns olt on olt.OfferLineId=ol.Id
                        LEFT JOIN U_001_01_DEMANDTRANS DT ON DT.LOGICALREF = olt.Trasnsref
                        LEFT JOIN U_001_01_DEMANDSLIPS DS ON DT.DEMANDSLIPREF = DS.LOGICALREF
                        left join U_001_01_DEMANDTRANSADS D_ANLY ON D_ANLY.TRANSREF=DT.LOGICALREF  
                        LEFT JOIN U_001_ANLYDIMENSIONS ANLY  ON ANLY.LOGICALREF=D_ANLY.ANLYDIMREF
                        LEFT JOIN U_001_ANLYDIMENSIONS SUB_ANLY  ON SUB_ANLY.CODE=SUBSTRING(ANLY.CODE,1,10)  
                        LEFT JOIN U_001_UNITS UNIT ON UNIT.LOGICALREF=DT.UOMREF 
                        LEFT JOIN U_001_ITEMS ITM ON ITM.LOGICALREF =DT.ITEMREF
                        left JOIN U_001_AUXCODES AUX ON AUX.AUXCODE = ITM.GROUPCODE AND CODETYPE=4
                         where ProjectId=@projectId ";
            return sqlConnection.Query<Ntl_OfferLine>(query, new { projectId }).ToList();

        }
        public double getProjectYearAmount(int projectId)
        {
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            string query = $@" select Amount from Ntl_PaymentPlan where LineNr=1 and ProjectId=@projectId ";
            return sqlConnection.ExecuteScalar<double>(query, new { projectId });
        }
        public void CancelOffer(int projectId)
        {

            string query = " update Ntl_Offer set Cancelled=1 where ProjectId=@projectId";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            int CountConfirm = sqlConnection.ExecuteScalar<int>(query, new { projectId });


            query = " update Ntl_OfferLine set Cancelled=1 where ProjectId=@projectId";
            sqlConnection = new SqlConnection(logoConStr);
            CountConfirm = sqlConnection.ExecuteScalar<int>(query, new { projectId });


            query = " update Ntl_OfferLineTrasns set Cancelled=1 where OfferLineId in  (select Id from  Ntl_OfferLine where  ProjectId=@projectId )";
            sqlConnection = new SqlConnection(logoConStr);
            CountConfirm = sqlConnection.ExecuteScalar<int>(query, new { projectId });

        }


        public Ntl_Offer getWaitingOfferForNotify(int projectId)
        {

            string query = $@" select * from Ntl_Offer where ProjectId=@projectId";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            Ntl_Offer offer = sqlConnection.Query<Ntl_Offer>(query, new { projectId }).FirstOrDefault();
            sqlConnection = new SqlConnection(logoConStr);
            query = $@" select max(RequestNr) from Ntl_OfferRequest where ProjectId=@projectId";
            offer.OfferCount = sqlConnection.ExecuteScalar<int>(query, new { projectId });
            return offer;
        }

        private List<Ntl_OfferLine> getWaitingOfferLines(int projectId)
        {
            List<Ntl_OfferLine> lines = new List<Ntl_OfferLine>();
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            string query = $@" select  ol.Id      ,ol.ProjectId      ,ol.ItemRef      ,ol.ItemCode      ,ol.ItemDesc      ,ol.Quantity
                                ,ol.Unit      ,ol.LastPurchPrice      ,ol.LastPurchPrice*ol.Quantity Total      ,ol.VatRate
                                ,ol.LastPurchPrice*Quantity*(1+(ol.VatRate/100))  NetTotal      ,ol.Cancelled      ,ol.isContract
                                ,AUX.DESCRIPTION ItemGrpCode
                        ,CONVERT(NVARCHAR(50),DS.SLIPNR)   SlipNr 
                        ,AUX.DESCRIPTION ItemGrpCode
                        ,(select TOP 1  TalepEden from RAYSIGORTA_BUTCE..ButceDetay  where FisNo=DS.SLIPNR)   PersonName
                         from Ntl_OfferLine ol
                        left join Ntl_OfferLineTrasns olt on olt.OfferLineId=ol.Id
                        LEFT JOIN U_001_01_DEMANDTRANS DT ON DT.LOGICALREF = olt.Trasnsref
                        LEFT JOIN U_001_01_DEMANDSLIPS DS ON DT.DEMANDSLIPREF = DS.LOGICALREF
                        left join U_001_01_DEMANDTRANSADS D_ANLY ON D_ANLY.TRANSREF=DT.LOGICALREF  
                        LEFT JOIN U_001_ANLYDIMENSIONS ANLY  ON ANLY.LOGICALREF=D_ANLY.ANLYDIMREF
                        LEFT JOIN U_001_ANLYDIMENSIONS SUB_ANLY  ON SUB_ANLY.CODE=SUBSTRING(ANLY.CODE,1,10)  
                        LEFT JOIN U_001_UNITS UNIT ON UNIT.LOGICALREF=DT.UOMREF 
                        LEFT JOIN U_001_ITEMS ITM ON ITM.LOGICALREF =DT.ITEMREF
                        left JOIN U_001_AUXCODES AUX ON AUX.AUXCODE = ITM.GROUPCODE AND CODETYPE=4
                         where ProjectId=@projectId";
            lines = sqlConnection.Query<Ntl_OfferLine>(query, new { projectId }).ToList();


            foreach (var line in lines)
            {
                int itemRef = line.ItemRef;
                sqlConnection = new SqlConnection(logoConStr);
                query = $@" select AUX.DESCRIPTION from U_001_ITEMS ITM                
                    left JOIN U_001_AUXCODES AUX ON AUX.AUXCODE = ITM.GROUPCODE AND CODETYPE=4  where ITM.LOGICALREF=@itemRef";
                line.ItemGrpCode = sqlConnection.ExecuteScalar<string>(query, new { itemRef });
            }

            return lines;
        }
        public Ntl_Offer getWaitingOffer(int projectId)
        {
            string query = $@" select * from Ntl_Offer where ProjectId=@projectId ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            Ntl_Offer offer = sqlConnection.Query<Ntl_Offer>(query, new { projectId }).FirstOrDefault();

            string isdeliveryAddress = "0";

            if (offer.isDeliveryAddress)
                isdeliveryAddress = "1";


            sqlConnection = new SqlConnection(logoConStr);
            query = $@" select  ol.Id      ,ol.ProjectId      ,ol.ItemRef      ,ol.ItemCode      ,ol.ItemDesc      ,ol.Quantity
                                ,ol.Unit      ,ol.LastPurchPrice      ,ol.LastPurchPrice*ol.Quantity Total      ,ol.VatRate
                                ,ol.LastPurchPrice*Quantity*(1+(ol.VatRate/100))  NetTotal      ,ol.Cancelled      ,ol.isContract
                                ,AUX.DESCRIPTION ItemGrpCode
                         ,case when {isdeliveryAddress} =1 then '{offer.DeliveryAddress}' else ADDR.BIRIM_ADI end Address
                        ,CONVERT(NVARCHAR(50),DS.SLIPNR)   SlipNr 
                        ,AUX.DESCRIPTION ItemGrpCode
                        ,(select TOP 1  TalepEden from RAYSIGORTA_BUTCE..ButceDetay  where FisNo=ds.SLIPNR)    PersonName
                         from Ntl_OfferLine ol
                        left join Ntl_OfferLineTrasns olt on olt.OfferLineId=ol.Id
                        LEFT JOIN U_001_01_DEMANDTRANS DT ON DT.LOGICALREF = olt.Trasnsref
                        LEFT JOIN U_001_01_DEMANDSLIPS DS ON DT.DEMANDSLIPREF = DS.LOGICALREF
                        left join U_001_01_DEMANDTRANSADS D_ANLY ON D_ANLY.TRANSREF=DT.LOGICALREF  
                        LEFT JOIN U_001_ANLYDIMENSIONS ANLY  ON ANLY.LOGICALREF=D_ANLY.ANLYDIMREF
                        LEFT JOIN U_001_ANLYDIMENSIONS SUB_ANLY  ON SUB_ANLY.CODE=SUBSTRING(ANLY.CODE,1,10)  
                        LEFT JOIN NET_PERSONEL_ADRES_BILGILERI ADDR ON ADDR.SICIL_NUMARASI = SUBSTRING(ANLY.CODE,LEN(ANLY.CODE)-3,5)
                        LEFT JOIN U_001_UNITS UNIT ON UNIT.LOGICALREF=DT.UOMREF 
                        LEFT JOIN U_001_ITEMS ITM ON ITM.LOGICALREF =DT.ITEMREF
                        left JOIN U_001_AUXCODES AUX ON AUX.AUXCODE = ITM.GROUPCODE AND CODETYPE=4
                         where ProjectId=@projectId";
            offer.Lines = sqlConnection.Query<Ntl_OfferLine>(query, new { projectId }).ToList();



            sqlConnection = new SqlConnection(logoConStr);
            query = $@" select * from Ntl_OfferSupplier supp where ProjectId=@projectId";
            offer.Suppliers = sqlConnection.Query<Ntl_OfferSupplier>(query, new { projectId }).ToList();


            sqlConnection = new SqlConnection(logoConStr);
            query = $@" select Id,UploadedFileName ,DocumentName from Ntl_OfferDocs supp where ProjectId=@projectId";
            offer.DocList = sqlConnection.Query<Ntl_OfferDocs>(query, new { projectId }).ToList();
            int offerCount = 0;
            foreach (var line in offer.Lines)
            {
                line.TransRef = getTransref(line.Id);

                foreach (var item in offer.Suppliers)
                {
                    item.NetTotals = getSupplierPriceses(projectId, item.SupplierRef, line.Quantity);
                    if (item.NetTotals.Count > 0)
                    {
                        offerCount = item.NetTotals.Max(x => x.RequestNr);
                        if (offer.OfferCount < offerCount)
                        {
                            offer.OfferCount = offerCount;
                        }

                    }

                }
            }
            return offer;
        }


        public Ntl_ItemInfo GetItemInfo(int ItemRef)
        {
            string query = $@" select ITM.LOGICALREF ItemRef, ITM.CODE  ItemCode ,ITM.DESCRIPTION ItemDesc,U.CODE UnitCode, 
                            U.LOGICALREF UomRef , ITM.UOMSETREF UomSetRef , US.CODE UnitSetCode
                            from U_001_ITEMS ITM
                            LEFT JOIN U_001_UNITS U  ON U.UOMSETREF = ITM.UOMSETREF AND U.MAINUOM=1
                            LEFT JOIN U_001_UNITSETS US ON US.LOGICALREF=ITM.UOMSETREF 
                            WHERE ITM.LOGICALREF=@ItemRef ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Query<Ntl_ItemInfo>(query, new { ItemRef }).FirstOrDefault();

        }

        public Ntl_OfferForOrder getWaitingOfferForOrder(int projectId, int supplierRef)
        {
            string query = $@" select * from Ntl_Offer where ProjectId=@projectId";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            Ntl_OfferForOrder offer = sqlConnection.Query<Ntl_OfferForOrder>(query, new { projectId }).FirstOrDefault();

            sqlConnection = new SqlConnection(logoConStr);
            query = $@" select * from Ntl_OfferLine where ProjectId=@projectId";
            offer.Lines = sqlConnection.Query<Ntl_OfferLine>(query, new { projectId }).ToList();

            foreach (var line in offer.Lines)
            {
                line.Priceses.Add(getSupplierPrice(line.Id, supplierRef));
            }

            offer.Supplier = getSupplierByRef(supplierRef);
            return offer;
        }
        public Ntl_Offer getWaitingOffer(int projectId, int OfferNr)
        {
            string query = $@" select * from Ntl_Offer where ProjectId=@projectId";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            Ntl_Offer offer = sqlConnection.Query<Ntl_Offer>(query, new { projectId }).FirstOrDefault();



            sqlConnection = new SqlConnection(logoConStr);
            query = $@" select * from Ntl_OfferLine where ProjectId=@projectId";
            offer.Lines = sqlConnection.Query<Ntl_OfferLine>(query, new { projectId }).ToList();



            sqlConnection = new SqlConnection(logoConStr);
            query = $@" select * from Ntl_OfferSupplier supp where ProjectId=@projectId";
            offer.Suppliers = sqlConnection.Query<Ntl_OfferSupplier>(query, new { projectId }).ToList();


            sqlConnection = new SqlConnection(logoConStr);
            query = $@" select Id,UploadedFileName ,DocumentName from Ntl_OfferDocs supp where ProjectId=@projectId";
            offer.DocList = sqlConnection.Query<Ntl_OfferDocs>(query, new { projectId }).ToList();

            foreach (var line in offer.Lines)
            {
                line.TransRef = getTransref(line.Id);

                foreach (var item in offer.Suppliers)
                {
                    item.NetTotals = getSupplierPriceses(projectId, item.SupplierRef, line.Quantity);
                    int offerCount = item.NetTotals.Count;
                    if (offer.OfferCount < offerCount)
                    {
                        offer.OfferCount = offerCount;
                    }
                    line.Priceses.Add(getSupplierPrice(line.Id, item.SupplierRef, OfferNr, line.Quantity));
                }
            }
            return offer;
        }

        public Ntl_Offer getSupplierWaitingOffer(int projectId, int OfferNr, int supplierRef)
        {
            string query = $@" select * from Ntl_Offer where ProjectId=@projectId";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            Ntl_Offer offer = sqlConnection.Query<Ntl_Offer>(query, new { projectId }).FirstOrDefault();



            sqlConnection = new SqlConnection(logoConStr);
            query = $@" select * from Ntl_OfferLine where ProjectId=@projectId";
            offer.Lines = sqlConnection.Query<Ntl_OfferLine>(query, new { projectId }).ToList();



            sqlConnection = new SqlConnection(logoConStr);
            query = $@" select * from Ntl_OfferSupplier supp where ProjectId=@projectId  and SupplierRef=@supplierRef";
            offer.Suppliers = sqlConnection.Query<Ntl_OfferSupplier>(query, new { projectId, supplierRef }).ToList();


            sqlConnection = new SqlConnection(logoConStr);
            query = $@" select Id,UploadedFileName ,DocumentName from Ntl_OfferDocs supp where ProjectId=@projectId";
            offer.DocList = sqlConnection.Query<Ntl_OfferDocs>(query, new { projectId }).ToList();

            foreach (var line in offer.Lines)
            {
                line.TransRef = getTransref(line.Id);

                foreach (var item in offer.Suppliers)
                {
                    item.NetTotals = getSupplierPriceses(projectId, item.SupplierRef, line.Quantity);
                    int offerCount = item.NetTotals.Count;
                    if (offer.OfferCount < offerCount)
                    {
                        offer.OfferCount = offerCount;
                    }
                    line.Priceses.Add(getSupplierPrice(line.Id, item.SupplierRef, OfferNr, line.Quantity));
                }
            }
            return offer;
        }

        public Ntl_OfferDocs getDocument(int Id)
        {
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            string query = $@" select UploadedFilePath ,  UploadedFileContentTyp from Ntl_OfferDocs supp where Id=@Id ";
            return sqlConnection.Query<Ntl_OfferDocs>(query, new { Id }).FirstOrDefault();
        }
        public Ntl_OfferDocs getOrderDocument(int Id)
        {
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            string query = $@" select UploadedFilePath ,  UploadedFileContentTyp from Ntl_OrderDocs supp where Id=@Id ";
            return sqlConnection.Query<Ntl_OfferDocs>(query, new { Id }).FirstOrDefault();
        }
        public Ntl_SupplierOffer getItemsForOffer(int projectId, int SuplierRef)
        {
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            string query = $@" select * from Ntl_Offer where ProjectId=@projectId";
            Ntl_SupplierOffer supplierOffer = sqlConnection.Query<Ntl_SupplierOffer>(query, new { projectId }).FirstOrDefault();



            sqlConnection = new SqlConnection(logoConStr);
            query = $@" select ol.*,AUX.DESCRIPTION ItemGrpCode
                        ,CONVERT(NVARCHAR(50),DS.SLIPNR)   SlipNr 
                        ,AUX.DESCRIPTION ItemGrpCode
                        ,(select TOP 1  TalepEden from RAYSIGORTA_BUTCE..ButceDetay  where FisNo=DS.SLIPNR)   PersonName
                         from Ntl_OfferLine ol
                        left join Ntl_OfferLineTrasns olt on olt.OfferLineId=ol.Id
                        LEFT JOIN U_001_01_DEMANDTRANS DT ON DT.LOGICALREF = olt.Trasnsref
                        LEFT JOIN U_001_01_DEMANDSLIPS DS ON DT.DEMANDSLIPREF = DS.LOGICALREF
                        left join U_001_01_DEMANDTRANSADS D_ANLY ON D_ANLY.TRANSREF=DT.LOGICALREF  
                        LEFT JOIN U_001_ANLYDIMENSIONS ANLY  ON ANLY.LOGICALREF=D_ANLY.ANLYDIMREF
                        LEFT JOIN U_001_ANLYDIMENSIONS SUB_ANLY  ON SUB_ANLY.CODE=SUBSTRING(ANLY.CODE,1,10)  
                        LEFT JOIN U_001_UNITS UNIT ON UNIT.LOGICALREF=DT.UOMREF 
                        LEFT JOIN U_001_ITEMS ITM ON ITM.LOGICALREF =DT.ITEMREF
                        left JOIN U_001_AUXCODES AUX ON AUX.AUXCODE = ITM.GROUPCODE AND CODETYPE=4
                         where ProjectId=@projectId";
            supplierOffer.Lines = sqlConnection.Query<Ntl_OfferItems>(query, new { projectId }).ToList();


            foreach (var line in supplierOffer.Lines)
            {
                int id = line.Id;
                sqlConnection = new SqlConnection(logoConStr);
                query = $@" select * from Ntl_SupplierOffer where SupplierRef=@SuplierRef and  LineId =@id";
                line.OfferList = sqlConnection.Query<Ntl_SupplierOfferList>(query, new { SuplierRef, id }).ToList();
                line.TrCurr = 160;

            }

            sqlConnection = new SqlConnection(logoConStr);
            query = $@" select * from Ntl_OfferDocs supp where ProjectId=@projectId";
            supplierOffer.files = sqlConnection.Query<Ntl_OfferDocs>(query, new { projectId }).ToList();

            sqlConnection = new SqlConnection(logoConStr);
            query = $@"select Responded from Ntl_OfferRequest where ProjectId=@projectId and SupplierRef=@SuplierRef";
            supplierOffer.Responded = sqlConnection.ExecuteScalar<int>(query, new { projectId, SuplierRef });


            return supplierOffer;
        }

        public Ntl_PersonInfo getProjectRespnsible(int projectId)
        {
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            string query = $@" select distinct u.*  from Ntl_Offer o
                                left join Net_UserInfo u on u.UserId=o.UserId
					                where ProjectId=@projectId ";
            return sqlConnection.Query<Ntl_PersonInfo>(query, new { projectId }).FirstOrDefault();
        }

        public List<Ntl_OfferDetail> getOfferDetail(int projectId)
        {
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            string query = $@"  select Sicil, ItemDesc, SUM(Quantity)  Quantity ,Department , Unit,Address,Telephone,isDeliveryAddress from  (   
                                 select                                
                                  SICIL_NUMARASI Sicil     
                                ,   ol.ItemDesc
                                 ,DT.QUANTITY Quantity
                                 ,UNIT.CODE Unit
                                 ,case when offer.isDeliveryAddress =1 then DeliveryAddress else ADDR.BIRIM_ADI end Department,
                                 ADDR.ADRES  Address ,ADDR.TELEFON Telephone ,offer.isDeliveryAddress
                                from   Ntl_Offer offer 
                                left join Ntl_OfferLine ol on offer.ProjectId=ol.ProjectId
                                left join Ntl_OfferLineTrasns olt on olt.OfferLineId=ol.Id
                                left join U_001_01_DEMANDTRANS dt on dt.LOGICALREF= olt.Trasnsref
                                left join U_001_01_DEMANDTRANSADS DT_ANLY ON DT_ANLY.TRANSREF = dt.LOGICALREF
                                left join U_001_ANLYDIMENSIONS ANLY ON ANLY.LOGICALREF = DT_ANLY.ANLYDIMREF
                                LEFT JOIN NET_PERSONEL_ADRES_BILGILERI ADDR ON ADDR.SICIL_NUMARASI = SUBSTRING(ANLY.CODE,LEN(ANLY.CODE)-3,5)
                                LEFT JOIN U_001_UNITS UNIT ON UNIT.LOGICALREF= DT.UOMREF   where offer.ProjectId=@projectId    ) as tbl
                                group by ItemDesc ,Department , Unit  ,Address,Telephone ,Sicil ,isDeliveryAddress";
            List<Ntl_OfferDetail> details = sqlConnection.Query<Ntl_OfferDetail>(query, new { projectId }).ToList();

            return details;
        }

        public bool getProjectContractControl(int ProjectId)
        {

            SqlConnection sql = new SqlConnection(logoConStr);
            string query = $@"  SELECT  case when sum(convert(int,isContract))>0 then convert(bit,1) else  convert(bit,0) end FROM Ntl_OfferLine where ProjectId=@ProjectId ";
            return sql.ExecuteScalar<bool>(query, new { ProjectId });
        }

        public List<string> getItemType(int ProjectId)
        {

            SqlConnection sql = new SqlConnection(logoConStr);
            string query = $@"  select SUBSTRING(DESCRIPTION4,1,1) from U_001_ITEMS WHERE LOGICALREF in (SELECT ItemRef FROM Ntl_OfferLine where ProjectId=@ProjectId) ";
            return sql.Query<string>(query, new { ProjectId }).ToList();
        }


        public Ntl_Request getResponses(int projectId)
        {

            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            string query = $@" delete from Ntl_OfferRequest where Id in(select Max(Id)
							from Ntl_OfferRequest 
							where ProjectId=@projectId
							group by RequestGuid,ProjectId							
							having count(Id)>1 ) ";
            sqlConnection.Execute(query, new { projectId });



            query = $@" select distinct SupplierRef from  Ntl_OfferSupplier where ProjectId=@projectId ";
            sqlConnection = new SqlConnection(logoConStr);
            List<int> supplierList = sqlConnection.Query<int>(query, new { projectId }).ToList();


            query = $@" select distinct SuggestionSupplierRef from  Ntl_Offer where ProjectId=@projectId ";
            sqlConnection = new SqlConnection(logoConStr);
            int SuggestionSupplierRef = sqlConnection.ExecuteScalar<int>(query, new { projectId });


            List<Ntl_BudgetControl> budgetCtrl = getItemForBudget(projectId.ToString());

            bool isContract = getProjectContractControl(projectId);
            List<string> itemTypes = getItemType(projectId);

            foreach (string itemType in itemTypes)
            {
                if (itemType == "2")
                {
                    isContract = true;
                }

            }


            Ntl_Request request = new Ntl_Request() { ProjectId = projectId }; ;

            foreach (var supplier in supplierList)
            {
                query = $@"select sum(Price*case when so.TrCurr=1 then o.UsdCurr when so.TrCurr=20 then o.EurCurr  else 1 end*ol.Quantity*((so.VatRate/100.0)+1) ) Total
                            ,RequestNr ,DESCRIPTION4 BudgetCode from Ntl_SupplierOffer so 
                            left join Ntl_Offer o on o.ProjectId=so.ProjectId
                            left join Ntl_OfferLine ol on ol.Id=so.LineId 
                            left join U_001_ITEMS ITM on ITM.LOGICALREF=ol.ItemRef
                            left join Ntl_OfferRequest nof on nof.RequestGuid=so.RequestGuid
                            where so.ProjectId=@projectId and so.SupplierRef =@supplier 
                            and  RequestNr= (select max(RequestNr) from Ntl_OfferRequest where Responded=1 and SupplierRef =@supplier  and  ProjectId=@projectId)
                            group by RequestNr,so.ProjectId ,DESCRIPTION4 ";

                sqlConnection = new SqlConnection(logoConStr);
                List<Ntl_RequestSupplierLine> lines = sqlConnection.Query<Ntl_RequestSupplierLine>(query, new { projectId, supplier }).ToList();


                Ntl_RequestSupplier requestSupplier = new Ntl_RequestSupplier();
                requestSupplier.SupplierRef = supplier;
                requestSupplier.Supplier = getSupplierByRef(supplier);

                requestSupplier.BudgetControl = true;

                if (lines.Count > 0)
                {

                    foreach (var item in budgetCtrl)
                    {
                        Ntl_RequestSupplierLine supplierLine = lines.Where(x => x.BudgetCode == item.BudgetCode).FirstOrDefault();
                        requestSupplier.RequestNr = supplierLine.RequestNr;
                        double budgetNet = 0;





                        if (isContract)
                        {

                            budgetNet = item.Budget.BudgetYear - item.Budget.GerceklesenYear - item.Budget.YillikBlokeAmount;
                        }
                        else
                        {
                            budgetNet = item.Budget.Budget - item.Budget.Gerceklesen - item.Budget.BlokeAmount;
                        }
                        requestSupplier.NetTotal += supplierLine.Total;

                        if (supplierLine.Total > budgetNet)
                        {
                            requestSupplier.BudgetControl = false;

                            Ntl_Budget budget = new Ntl_Budget()
                            {
                                BlokeAmount = item.Budget.BlokeAmount,
                                Budget = item.Budget.Budget,
                                BudgetCode = item.BudgetCode,
                                BudgetName = item.Budget.BudgetName,
                                BudgetYear = item.Budget.BudgetYear,
                                GerceklesenYear = item.Budget.GerceklesenYear,
                                Gerceklesen = item.Budget.Gerceklesen,
                                YillikBlokeAmount = item.Budget.YillikBlokeAmount,
                                BudgetMonth = item.Budget.BudgetMonth,
                                BudgetYillik = isContract,
                                BudgetOk = false



                            };

                            if (SuggestionSupplierRef == supplier)
                            {
                                request.Budgets.Add(budget);
                            }
                        }
                        else
                        {

                            Ntl_Budget budget = new Ntl_Budget()
                            {
                                BlokeAmount = item.Budget.BlokeAmount,
                                Budget = item.Budget.Budget,
                                BudgetCode = item.BudgetCode,
                                BudgetName = item.Budget.BudgetName,
                                BudgetYear = item.Budget.BudgetYear,
                                GerceklesenYear = item.Budget.GerceklesenYear,
                                Gerceklesen = item.Budget.Gerceklesen,
                                YillikBlokeAmount = item.Budget.YillikBlokeAmount,
                                BudgetYillik = isContract,
                                BudgetOk = true

                            };
                            if (SuggestionSupplierRef == supplier)
                            {
                                request.Budgets.Add(budget);
                            }
                        }
                        requestSupplier.supplierLines.Add(supplierLine);
                    }

                    requestSupplier.RespondMessage = getRespondMessage(projectId, supplier);
                    requestSupplier.Responded = getRespondedInfo(projectId, supplier, requestSupplier.RequestNr);

                    request.RequestSuppliers.Add(requestSupplier);



                }
                else
                {
                    requestSupplier.RespondMessage = getRespondMessage(projectId, supplier);
                    requestSupplier.Responded = getRespondedInfo(projectId, supplier);
                    request.RequestSuppliers.Add(requestSupplier);

                }
            }


            return request;
        }

        private string getRespondMessage(int projectId, int supplierId)
        {

            string query = $@" SELECT top 1  Explain
                            FROM Ntl_OfferRequest where ProjectId=@projectId and SupplierRef=@supplierId order by RequestNr desc ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);

            return sqlConnection.Query<string>(query, new { supplierId, projectId }).FirstOrDefault();

        }
        private int getRespondedInfo(int projectId, int supplierId, int requestNr)
        {

            string query = $@" SELECT top 1   case when (select COUNT(*) from Ntl_SupplierOffer where RequestGuid=ofr.RequestGuid )
                                >(select COUNT(*) from Ntl_SupplierOffer where RequestGuid=ofr.RequestGuid and Price>0 ) then 3 else Responded end  Responded
                            FROM Ntl_OfferRequest ofr where ProjectId=@projectId and SupplierRef=@supplierId and RequestNr=@requestNr   order by RequestNr desc ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);

            return sqlConnection.Query<int>(query, new { supplierId, projectId, requestNr }).FirstOrDefault();

        }
        private int getRespondedInfo(int projectId, int supplierId)
        {

            string query = $@" SELECT top 1  Responded
                            FROM Ntl_OfferRequest where ProjectId=@projectId and SupplierRef=@supplierId 
                            and Responded<>(select case when sum(Responded)>0 then 0 else 2 end from Ntl_OfferRequest where ProjectId=@projectId and SupplierRef=@supplierId  )
                                order by RequestNr desc ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);

            return sqlConnection.Query<int>(query, new { supplierId, projectId }).FirstOrDefault();

        }

        public Ntl_Request getResponsesForComment(int projectId)
        {

            string query = $@" select SuggestionSupplierRef from  Ntl_Offer where ProjectId=@projectId";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            int supplier = sqlConnection.ExecuteScalar<int>(query, new { projectId });
            List<Ntl_BudgetControl> budgetCtrl = getItemForBudget(projectId.ToString());


            Ntl_Request request = new Ntl_Request()
            { ProjectId = projectId };

            query = $@"select   sum(Price*case when so.TrCurr=1 then o.UsdCurr when so.TrCurr=20 then o.EurCurr  else 1 end*ol.Quantity*((so.VatRate/100.0)+1) ) Total
                            ,RequestNr ,DESCRIPTION4 BudgetCode from Ntl_SupplierOffer so 
                            left join Ntl_Offer o on o.ProjectId=so.ProjectId
                            left join Ntl_OfferLine ol on ol.Id=so.LineId 
                            left join U_001_ITEMS ITM on ITM.LOGICALREF=ol.ItemRef
                            left join Ntl_OfferRequest nof on nof.RequestGuid=so.RequestGuid
                            where so.ProjectId=@projectId and so.SupplierRef =@supplier  group by RequestNr,so.ProjectId ,DESCRIPTION4 order by RequestNr  desc ";

            sqlConnection = new SqlConnection(logoConStr);
            List<Ntl_RequestSupplierLine> lines = sqlConnection.Query<Ntl_RequestSupplierLine>(query, new { projectId, supplier }).ToList();


            Ntl_RequestSupplier requestSupplier = new Ntl_RequestSupplier();
            requestSupplier.SupplierRef = supplier;
            requestSupplier.Supplier = getSupplierByRef(supplier);


            bool isContract = getProjectContractControl(projectId);

            requestSupplier.BudgetControl = true;
            foreach (var item in budgetCtrl)
            {
                Ntl_RequestSupplierLine supplierLine = lines.Where(x => x.BudgetCode == item.BudgetCode).FirstOrDefault();
                requestSupplier.RequestNr = supplierLine.RequestNr;
                double budgetNet = 0;
                if (isContract)
                {

                    budgetNet = item.Budget.BudgetYear - item.Budget.Gerceklesen - item.Budget.YillikBlokeAmount;
                }
                else
                {
                    budgetNet = item.Budget.Budget - item.Budget.Gerceklesen - item.Budget.BlokeAmount;
                }


                if (supplierLine.Total > budgetNet)
                {

                    requestSupplier.BudgetControl = false;
                    Ntl_Budget budget = new Ntl_Budget()
                    {
                        BlokeAmount = item.Budget.BlokeAmount,
                        Budget = item.Budget.Budget,
                        BudgetCode = item.BudgetCode,
                        BudgetName = item.Budget.BudgetName,
                        BudgetYear = item.Budget.BudgetYear,
                        GerceklesenYear = item.Budget.GerceklesenYear,
                        Gerceklesen = item.Budget.Gerceklesen,
                        YillikBlokeAmount = item.Budget.YillikBlokeAmount,
                        BudgetMonth = item.Budget.BudgetMonth,
                        BudgetYillik = isContract,
                        BudgetOk = false

                    };

                    request.Budgets.Add(budget);
                }
                else
                {

                    Ntl_Budget budget = new Ntl_Budget()
                    {
                        BlokeAmount = item.Budget.BlokeAmount,
                        Budget = item.Budget.Budget,
                        BudgetCode = item.BudgetCode,
                        BudgetName = item.Budget.BudgetName,
                        BudgetYear = item.Budget.BudgetYear,
                        GerceklesenYear = item.Budget.GerceklesenYear,
                        Gerceklesen = item.Budget.Gerceklesen,
                        YillikBlokeAmount = item.Budget.YillikBlokeAmount,
                        BudgetYillik = isContract,
                        BudgetOk = true
                    };
                    request.Budgets.Add(budget);
                }
                requestSupplier.supplierLines.Add(supplierLine);
            }




            return request;
        }


        public Ntl_OfferRequest getRequest(string requestId)
        {
            string query = $@" SELECT *   FROM Ntl_OfferRequest where  RequestGuid =@requestId ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Query<Ntl_OfferRequest>(query, new { requestId }).FirstOrDefault();

        }

        public List<int> getTransref(int lineId)
        {
            string query = $@" select Trasnsref from Ntl_OfferLineTrasns where OfferLineId=@lineId";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Query<int>(query, new { lineId }).ToList();

        }
        public string getItemGroups(int projectId)
        {
            string query = $@" select DISTINCT AUX.DESCRIPTION from  Ntl_OfferLine ol
                                left join Ntl_OfferLineTrasns olt on olt.OfferLineId=ol.Id
                                left join U_001_01_DEMANDTRANS dt on dt.LOGICALREF= olt.Trasnsref
                                left join U_001_ITEMS itm on itm.LOGICALREF= dt.ITEMREF
                                left JOIN U_001_AUXCODES AUX ON AUX.AUXCODE = itm.GROUPCODE AND CODETYPE = 4
                                WHERE ProjectId =@projectId";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);

            List<string> list = sqlConnection.Query<string>(query, new { projectId }).ToList();
            string ItemGroups = "";
            foreach (var item in list)
            {
                if (item != null)
                {
                    ItemGroups += "," + item.ToLower();
                }
            }
            if (ItemGroups.Length > 0)
            {
                ItemGroups = ItemGroups.Substring(1);
            }
            return ItemGroups;

        }



        public List<int> getOfferTransref(int transRef)
        {
            string query = $@" select LOGICALREF from U_001_OFFERTRANS where OFFERALTREF=@transRef ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Query<int>(query, new { transRef }).ToList();

        }

        public List<Ntl_BrowserOffer> getProjectsOnOffer(int pageNr)
        {

            int begNr = (pageNr * 15) - 14;
            int endNr = (pageNr * 15);

            string query = $@"select * from( select ROW_NUMBER() OVER(ORDER BY  ProjectNr desc) AS SiraNo,  ProjectId,ProjectNr , EndDate,EndTime ,SuggestionSupplierRef from Ntl_Offer   where ProjectId not in (select ProjectId from Ntl_Comment union all
                select ProjectId from Ntl_Confirm)   and  isnull(Cancelled,0) = 0) as tbl  where SiraNo between @begNr and @endNr ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Query<Ntl_BrowserOffer>(query, new { begNr, endNr }).ToList();

        }

        public int getCountProjectsOnOffer()
        {



            string query = $@"select count(*) from Ntl_Offer   where ProjectId not in (select ProjectId from Ntl_Comment union all
                select ProjectId from Ntl_Confirm)   and  isnull(Cancelled,0) = 0 ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<int>(query);

        }

        public List<Ntl_BrowserOffer> getProjectsWaitingOrder(int pageNr, Dictionary<string, string> keys)
        {
            List<Ntl_BrowserOffer> list = new List<Ntl_BrowserOffer>();

            try
            {

                int begNr = (pageNr * 15) - 14;
                int endNr = (pageNr * 15);
                string query = $@" select * from ( select ROW_NUMBER() OVER(ORDER BY  ProjectNr desc) AS SiraNo, * from (    select distinct 
                    ProjectId,ProjectNr , EndDate,EndTime,SuggestionSupplierRef ,OrderSlipRef ,				
					case when 	 isnull((select TOP 1  PersonName from Ntl_Comment where ISNULL(Status_,0)=0 and ProjectId=o.ProjectId ),'' ) <>'' then  1 	
					when isnull((select top 1 PersonName from Ntl_Confirm where ISNULL(ConfirmStatus,0)=0 and ProjectId=o.ProjectId  order by Id desc),'') <> '' then 2                                 
                    when           isnull((select top 1 PersonName from Ntl_Confirm where ISNULL(ConfirmStatus,0)=2 and ProjectId=o.ProjectId  order by Id desc),'')<>'' then 3 
                    when          (select COUNT(*) from Ntl_Confirm where ProjectId=o.ProjectId) >0 then 4 else 0 end ProjectStatus
					from Ntl_Offer o  where  ProjectId  in (select ProjectId from Ntl_Comment union all
					select ProjectId from Ntl_Confirm)    and ISNULL(IsSend,0)=0   and  isnull(Cancelled,0) = 0 ) as tbl  where 1=1   ";
                if (!string.IsNullOrEmpty(keys["PrjNo"]))
                {
                    query += $@" and ProjectNr like '%{keys["PrjNo"]}%' ";
                }

                if (!string.IsNullOrEmpty(keys["ProjectStatus"]) && keys["ProjectStatus"] != "0")
                {
                    query += $@" and tbl.ProjectStatus ={keys["ProjectStatus"]} ";
                }
                query += $@"  ) as tbl2 where SiraNo between  @begNr and @endNr ";

                SqlConnection sqlConnection = new SqlConnection(logoConStr);
                list = sqlConnection.Query<Ntl_BrowserOffer>(query, new { begNr, endNr }).ToList();

            }
            catch (Exception ex)
            {

                throw ex;
            }

            return list;
        }
        public int getCountProjectsWaitingOrder(Dictionary<string, string> keys)
        {

            string query = $@" select count(*) from (    select distinct 
                    ProjectId,ProjectNr , EndDate,EndTime,SuggestionSupplierRef ,OrderSlipRef ,				
					case when 	 isnull((select TOP 1  PersonName from Ntl_Comment where ISNULL(Status_,0)=0 and ProjectId=o.ProjectId ),'' ) <>'' then  1 	
					when isnull((select top 1 PersonName from Ntl_Confirm where ISNULL(ConfirmStatus,0)=0 and ProjectId=o.ProjectId  order by Id desc),'') <> '' then 2                                 
                    when           isnull((select top 1 PersonName from Ntl_Confirm where ISNULL(ConfirmStatus,0)=2 and ProjectId=o.ProjectId  order by Id desc),'')<>'' then 3 
                    when          (select COUNT(*) from Ntl_Confirm where ProjectId=o.ProjectId) >0 then 4 else 0 end ProjectStatus
					from Ntl_Offer o  where  ProjectId  in (select ProjectId from Ntl_Comment union all
					select ProjectId from Ntl_Confirm)    and ISNULL(IsSend,0)=0   and  isnull(Cancelled,0) = 0 ) as tbl  where 1=1   ";
            if (!string.IsNullOrEmpty(keys["PrjNo"]))
            {
                query += $@" and ProjectNr like '%{keys["PrjNo"]}%' ";
            }

            if (!string.IsNullOrEmpty(keys["ProjectStatus"]) && keys["ProjectStatus"] != "0")
            {
                query += $@" and tbl.ProjectStatus ={keys["ProjectStatus"]} ";
            }
            query += $@"    ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<int>(query);
        }
        public List<Ntl_BrowserOffer> getProjectsOnOrder(int pageNr, Dictionary<string, string> keys)
        {

            DateTime enddate = DateTime.Today;
            if (!string.IsNullOrEmpty(keys["EndDate"]))
            {
                enddate = Convert.ToDateTime(keys["EndDate"]);

            }

            int begNr = (pageNr * 15) - 14;
            int endNr = (pageNr * 15);
            string query = $@" select * from( 
                    select ROW_NUMBER() OVER(ORDER BY  ProjectNr desc) AS SiraNo, 
                    ProjectId,ProjectNr , EndDate,EndTime,SuggestionSupplierRef ,OrderSlipRef 
                    from Ntl_Offer   o
                    left join U_001_ARPS a on a.LOGICALREF=o.SuggestionSupplierRef
                    left join U_001_01_ORDERSLIPS orf on orf.LOGICALREF=o.OrderSlipRef
                    where    ISNULL(IsSend,0)=1  and OrderSlipRef NOT in   (select ORDSLIPREF from U_001_01_MMTRANS )   and  isnull(Cancelled,0) = 0 ";
            if (!string.IsNullOrEmpty(keys["PrjNo"]))
            {
                query += $@" and ProjectNr like '%{keys["PrjNo"]}%' ";
            }

            if (!string.IsNullOrEmpty(keys["OrderNr"]))
            {
                query += $@" and orf.SLIPNR like '%{keys["OrderNr"]}%' ";
            }
            if (!string.IsNullOrEmpty(keys["Supplier"]))
            {
                query += $@" and a.DESCRIPTION like '%{keys["Supplier"]}%' ";
            }
            if (!string.IsNullOrEmpty(keys["EndDate"]))
            {
                query += $@" and EndDate=@enddate ";
            }




            query += $@" ) as tbl  where SiraNo between  @begNr and @endNr ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            List<Ntl_BrowserOffer> offers = sqlConnection.Query<Ntl_BrowserOffer>(query, new { enddate, begNr, endNr }).ToList();
            foreach (var offer in offers)
            {
                offer.Supplier = getSupplierByRef(offer.SuggestionSupplierRef);
                offer.OrderInfo = getOrderInfo(offer.OrderSlipRef);
            }
            return offers;
        }

        public int getCountProjectsOnOrder(Dictionary<string, string> keys)
        {
            DateTime enddate = DateTime.Today;
            if (!string.IsNullOrEmpty(keys["EndDate"]))
            {
                enddate = Convert.ToDateTime(keys["EndDate"]);

            }

            string query = $@" select count(*)
                    from Ntl_Offer   o
                    left join U_001_ARPS a on a.LOGICALREF=o.SuggestionSupplierRef
                    left join U_001_01_ORDERSLIPS orf on orf.LOGICALREF=o.OrderSlipRef
                    where    ISNULL(IsSend,0)=1  and OrderSlipRef NOT in   (select ORDSLIPREF from U_001_01_MMTRANS )   and  isnull(Cancelled,0) = 0 ";
            if (!string.IsNullOrEmpty(keys["PrjNo"]))
            {
                query += $@" and ProjectNr like '%{keys["PrjNo"]}%' ";
            }

            if (!string.IsNullOrEmpty(keys["OrderNr"]))
            {
                query += $@" and orf.SLIPNR like '%{keys["OrderNr"]}%' ";
            }
            if (!string.IsNullOrEmpty(keys["Supplier"]))
            {
                query += $@" and a.DESCRIPTION like '%{keys["Supplier"]}%' ";
            }
            if (!string.IsNullOrEmpty(keys["EndDate"]))
            {
                query += $@" and EndDate=@enddate ";
            }

            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<int>(query, new { enddate });
        }

        public Ntl_OrderInfo getOrderInfo(int orderRef)
        {
            string query = $@" select LOGICALREF OrderSlipRef, SLIPNR OrderNr ,NETTOTAL  OrderNetTotal from U_001_01_ORDERSLIPS WHERE  LOGICALREF=@orderRef ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);

            return sqlConnection.Query<Ntl_OrderInfo>(query, new { orderRef }).FirstOrDefault(); ;
        }
        public Ntl_InvoiceInfo getInvoiceInfo(int orderRef)
        {
            string query = $@" select distinct inv.SLIPNR InvoiceNr,inv.NETTOTAL InvoiceNetTotal,inv.LOGICALREF InvoiceSlipRef ,inv.BOSTATUS InvoiceStatus from U_001_01_MMTRANS mmt
                                left join U_001_01_INVOICES inv on inv.LOGICALREF=mmt.INVOICEREF
                                left join U_001_01_ORDERSLIPS ord on ord.LOGICALREF=mmt.ORDSLIPREF
                                where ord.LOGICALREF=@orderRef ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Query<Ntl_InvoiceInfo>(query, new { orderRef }).FirstOrDefault(); ;
        }
        public Ntl_InvoiceInfo getInvoiceInfoByInvoicRef(int orderRef)
        {
            string query = $@" select distinct inv.SLIPNR InvoiceNr,inv.NETTOTAL InvoiceNetTotal,inv.LOGICALREF InvoiceSlipRef ,inv.BOSTATUS InvoiceStatus from U_001_01_MMTRANS mmt
                                left join U_001_01_INVOICES inv on inv.LOGICALREF=mmt.INVOICEREF
                                left join U_001_01_ORDERSLIPS ord on ord.LOGICALREF=mmt.ORDSLIPREF
                                where inv.LOGICALREF=@orderRef ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Query<Ntl_InvoiceInfo>(query, new { orderRef }).FirstOrDefault(); ;
        }
        public void UpdateInvoice(int InvoiceRef)
        {

        }

        public bool setInvoiceStatus(int ProjectId)
        {
            string query = $@" update Ntl_Order set   InvoiceConfirmed=1  where ProjectId=@ProjectId";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<bool>(query, new { ProjectId });
        }

        public List<Ntl_BrowserOffer> getProjectsCompleted(int pageNr, Dictionary<string, string> keys)
        {
            DateTime enddate = DateTime.Today;
            if (!string.IsNullOrEmpty(keys["EndDate"]))
            {
                enddate = Convert.ToDateTime(keys["EndDate"]);

            }
            int begNr = (pageNr * 15) - 14;
            int endNr = (pageNr * 15);
            string query = $@" select * from (
select ROW_NUMBER() OVER(ORDER BY  ProjectNr desc) AS SiraNo,  ProjectId,DueDate,	ProjectNr,	EndDate,	EndTime,	SuggestionSupplierRef,	OrderSlipRef ,InvoiceSlipRef from( 
                    select  
                distinct    ProjectId,ProjectNr , EndDate,EndTime,SuggestionSupplierRef ,OrderSlipRef ,INVOICEREF InvoiceSlipRef,
            (select MAX(DUEDATE) from U_001_01_ORDERTRANS WHERE ORDSLIPREF=orf.LOGICALREF) DueDate
                    from Ntl_Offer   o
                    left join U_001_ARPS a on a.LOGICALREF=o.SuggestionSupplierRef
                    left join U_001_01_ORDERSLIPS orf on orf.LOGICALREF=o.OrderSlipRef
                    left join U_001_01_MMTRANS mmt on mmt.ORDSLIPREF =orf.LOGICALREF
                    left join U_001_01_INVOICES inv on inv.LOGICALREF=mmt.INVOICEREF 
                    where    ISNULL(IsSend,0)=1  and OrderSlipRef  in   (select ORDSLIPREF from U_001_01_MMTRANS )   and  isnull(Cancelled,0) = 0  ";
            if (!string.IsNullOrEmpty(keys["PrjNo"]))
            {
                query += $@" and ProjectNr like '%{keys["PrjNo"]}%' ";
            }

            if (!string.IsNullOrEmpty(keys["OrderNr"]))
            {
                query += $@" and orf.SLIPNR like '%{keys["OrderNr"]}%' ";
            }
            if (!string.IsNullOrEmpty(keys["InvoiceNr"]))
            {
                query += $@" and inv.SLIPNR like '%{keys["InvoiceNr"]}%' ";
            }

            if (!string.IsNullOrEmpty(keys["Supplier"]))
            {
                query += $@" and a.DESCRIPTION like '%{keys["Supplier"]}%' ";
            }
            if (!string.IsNullOrEmpty(keys["EndDate"]))
            {
                query += $@" and EndDate=@enddate ";
            }
            query += $@" ) as tbl ) as tbl2 where SiraNo between  @begNr and @endNr ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            List<Ntl_BrowserOffer> offers = sqlConnection.Query<Ntl_BrowserOffer>(query, new { enddate, begNr, endNr }).ToList();
            foreach (var offer in offers)
            {
                offer.Supplier = getSupplierByRef(offer.SuggestionSupplierRef);
                offer.OrderInfo = getOrderInfo(offer.OrderSlipRef);
                offer.InvInfo = getInvoiceInfoByInvoicRef(offer.InvoiceSlipRef);
                offer.SendedConfirm = getInvoiceConfirmed(offer.ProjectId);
                offer.InvoiceConfirmed = getInvoiceStatus(offer.ProjectId);

            }
            return offers;
        }

        public int getCountProjectsCompleted(Dictionary<string, string> keys)
        {
            DateTime enddate = DateTime.Today;
            if (!string.IsNullOrEmpty(keys["EndDate"]))
            {
                enddate = Convert.ToDateTime(keys["EndDate"]);
            }
            string query = $@" select count(*) from ( select distinct  ProjectId,	ProjectNr,	EndDate,	EndTime,	SuggestionSupplierRef,	OrderSlipRef ,mmt.INVOICEREF  InvoiceSlipRef
                    from Ntl_Offer   o
                    left join U_001_ARPS a on a.LOGICALREF=o.SuggestionSupplierRef
                    left join U_001_01_ORDERSLIPS orf on orf.LOGICALREF=o.OrderSlipRef
                    left join U_001_01_MMTRANS mmt on mmt.ORDSLIPREF =orf.LOGICALREF
                    left join U_001_01_INVOICES inv on inv.LOGICALREF=mmt.INVOICEREF 
                    where ISNULL(IsSend,0)=1  and OrderSlipRef in   (select ORDSLIPREF from U_001_01_MMTRANS )
               
";
            if (!string.IsNullOrEmpty(keys["PrjNo"]))
            {
                query += $@" and ProjectNr like '%{keys["PrjNo"]}%' ";
            }

            if (!string.IsNullOrEmpty(keys["OrderNr"]))
            {
                query += $@" and orf.SLIPNR like '%{keys["OrderNr"]}%' ";
            }
            if (!string.IsNullOrEmpty(keys["InvoiceNr"]))
            {
                query += $@" and inv.SLIPNR like '%{keys["InvoiceNr"]}%' ";
            }

            if (!string.IsNullOrEmpty(keys["Supplier"]))
            {
                query += $@" and a.DESCRIPTION like '%{keys["Supplier"]}%' ";
            }
            if (!string.IsNullOrEmpty(keys["EndDate"]))
            {
                query += $@" and EndDate=@enddate ";
            }


            query += $@"  and isnull(Cancelled,0) = 0 ) as tbl where 1 = 1";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<int>(query, new { enddate });
        }

        public bool getInvoiceStatus(int ProjectId)
        {
            string query = $@" select isnull(InvoiceConfirmed,0) from Ntl_Order where ProjectId=@ProjectId ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<bool>(query, new { ProjectId });
        }

        public int getInvoiceConfirmed(int ProjectId)
        {
            string query = $@" select isnull(( SELECT   ConfirmStatus   FROM Ntl_InvoiceConfirm where ProjectId=@ProjectId ),1) ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<int>(query, new { ProjectId });
        }
        public string getInvoiceConfirmedExplanation(int ProjectId)
        {
            string query = $@"  SELECT   Explanation    FROM Ntl_InvoiceConfirm where ProjectId=@ProjectId ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<string>(query, new { ProjectId });
        }
        public Ntl_BrowserOffer getProjectCompleted(int ProjectId)
        {
            string query = $@" select ProjectId,ProjectNr , EndDate,EndTime,SuggestionSupplierRef ,OrderSlipRef from Ntl_Offer   where    ISNULL(IsSend,0)=1  and  isnull(Cancelled,0) = 0  and ProjectId=@ProjectId ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            Ntl_BrowserOffer offer = sqlConnection.Query<Ntl_BrowserOffer>(query, new { ProjectId }).FirstOrDefault();


            offer.Supplier = getSupplierByRef(offer.SuggestionSupplierRef);
            offer.OrderInfo = getOrderInfo(offer.OrderSlipRef);
            offer.InvInfo = getInvoiceInfo(offer.OrderSlipRef);
            offer.Explanation = getInvoiceConfirmedExplanation(offer.ProjectId);

            return offer;
        }

        public List<Ntl_OfferDocs> getOfferDocs(int projectId)
        {
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            string query = $@" select *  from Ntl_OfferDocs supp where ProjectId=@projectId";
            return sqlConnection.Query<Ntl_OfferDocs>(query, new { projectId }).ToList();
        }

        public List<Ntl_OrderDocs> getOrderDocs(int projectId)
        {
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            string query = $@" select Id,UploadedFileName ,DocumentName, UploadedFilePath from Ntl_OrderDocs supp where ProjectId=@projectId";
            return sqlConnection.Query<Ntl_OrderDocs>(query, new { projectId }).ToList();
        }
        public Ntl_SupplierOfferPrice getSupplierPrice(int lineId, int supplierRef)
        {
            string query = $@"  select Price *(case when TrCurr =1 then o.UsdCurr  when TrCurr = 20 then EurCurr else 1 end )  Price,so.Explanation 
                                from Ntl_SupplierOffer so  left join Ntl_Offer o on o.ProjectId=so.ProjectId
                                where LineId=@lineId and SupplierRef =@supplierRef order by OfferTime desc  ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Query<Ntl_SupplierOfferPrice>(query, new { supplierRef, lineId }).FirstOrDefault();
        }
        public Ntl_SupplierOfferPrice getSupplierPrice(int lineId, int supplierRef, int OfferNr, double Quantity)
        {

            DataSet dsDovizKur = new DataSet();
            dsDovizKur.ReadXml(@"https://www.tcmb.gov.tr/kurlar/today.xml");
            DataRow drUsd = dsDovizKur.Tables[1].Rows[0];
            DataRow drEur = dsDovizKur.Tables[1].Rows[3];

            double Eur = Convert.ToDouble(drEur[4].ToString().Replace('.', ','));
            double Usd = Convert.ToDouble(drUsd[4].ToString().Replace('.', ','));
            string query = $@"select Price,Explanation,TrCurr ,TrRate,VatRate from Ntl_SupplierOffer so
                        left join Ntl_OfferRequest nof on nof.RequestGuid=so.RequestGuid 
                        where LineId=@lineId and so.SupplierRef =@supplierRef and RequestNr=@OfferNr ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);

            Ntl_SupplierOfferPrice offerPrice = sqlConnection.Query<Ntl_SupplierOfferPrice>(query, new { supplierRef, OfferNr, lineId }).FirstOrDefault();

            if (offerPrice == null)
            {
                offerPrice = new Ntl_SupplierOfferPrice();
            }
            else
            {
                if (offerPrice.TrCurr == 1)
                {
                    double vatRate = (offerPrice.VatRate / 100.0) + 1;
                    offerPrice.Price = offerPrice.Price;
                    offerPrice.TrNet = offerPrice.Price * Quantity;
                    offerPrice.TrRate = Usd;
                }
                else if (offerPrice.TrCurr == 20)
                {
                    double vatRate = (offerPrice.VatRate / 100.0) + 1;
                    offerPrice.Price = offerPrice.Price;
                    offerPrice.TrNet = offerPrice.Price * Quantity;
                    offerPrice.TrRate = Eur;
                }
                else
                {
                    double vatRate = (offerPrice.VatRate / 100.0) + 1;
                    offerPrice.Price = offerPrice.Price;
                    offerPrice.TrNet = offerPrice.Price * Quantity;
                    offerPrice.TrRate = 1;
                }
            }

            return offerPrice;
        }
        public string getProjeNo(int prjId)
        {
            string query = $@" select ProjectNr from Ntl_Offer where ProjectId=@prjId   ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<string>(query, new { prjId });
        }


        public string getProjectUser(int prjId)
        {
            string query = $@" select FullName from Ntl_Offer o
                               left join Ntl_User u on u.Id=o.UserId where ProjectId=@prjId   ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<string>(query, new { prjId });
        }



        public List<Ntl_ConfirmHistory> getHistory(int projectId)
        {
            string query = $@" SELECT  PersonName Person
                                ,case  ConfirmStatus when   -10  then 'Onaya Gönderdi'  when 1  then 'Onayladı' when 2 then  'Onaylamadan Geri Gönderdi' when 3 then  'Onaylamadan Geri Gönderdi'  end ConfirmType
                                ,CommentTime Time_
                                ,isnull((select top 1 CommentTime from Ntl_Confirm    where ProjectId=cf.ProjectId and  ConfirmStatus in (-10,1,2,3) and CommentTime<cf.CommentTime order by CommentTime desc),'1900-01-01') BeforeTime_    
                                FROM Ntl_Confirm  cf
                                where ProjectId=@projectId and  ConfirmStatus in (-10,1,2,3)
                                order by CommentTime desc  ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Query<Ntl_ConfirmHistory>(query, new { projectId }).ToList();
        }

        public int createComment(Ntl_Comment comment)
        {
            string query = $@" INSERT INTO Ntl_Comment
           (ProjectId
           ,CommentGuid
           ,Comment
           ,PersonName
           ,PersonEmail
           ,CommentTime,Status_)
     VALUES
           (@ProjectId
           ,@CommentGuid
           ,@Comment
           ,@PersonName
           ,@PersonEmail
           ,@CommentTime,@Status_)";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<int>(query, comment);

        }


        public List<Ntl_NetTotal> getSupplierPriceses(int ProjectId, int supplierRef, double Quantity)
        {
            DataSet dsDovizKur = new DataSet();
            dsDovizKur.ReadXml(@"https://www.tcmb.gov.tr/kurlar/today.xml");
            DataRow drUsd = dsDovizKur.Tables[1].Rows[0];
            DataRow drEur = dsDovizKur.Tables[1].Rows[3];

            double Eur = Convert.ToDouble(drEur[4].ToString().Replace('.', ','));
            double Usd = Convert.ToDouble(drUsd[4].ToString().Replace('.', ','));

            string query = $@" select distinct RequestNr from   Ntl_OfferRequest where ProjectId=@ProjectId  ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);

            List<int> Requestlist = sqlConnection.Query<int>(query, new { ProjectId }).ToList();
            List<Ntl_NetTotal> list = new List<Ntl_NetTotal>();
            foreach (var RequestNr in Requestlist)
            {

                query = $@" select count(*) from  Ntl_OfferRequest where ProjectId={ProjectId} and SupplierRef =@supplierRef  and RequestNr=@RequestNr ";
                bool Requestsend = sqlConnection.ExecuteScalar<bool>(query, new { supplierRef, RequestNr });



                query = $@"  select isnull(Sum(NetTotal),0) NetTotal,RequestNr , isnull(VatRate,0) VatRate,RequestGuid, isnull(TrCurr,0) TrCurr ,Closed ,Responded from (   
                 select distinct so.Id, Price*ol.Quantity  NetTotal,RequestNr ,so.VatRate ,so.TrCurr ,nof.RequestGuid ,isnull(Closed,0) Closed ,
                  case when (select COUNT(*) from Ntl_SupplierOffer where RequestGuid=so.RequestGuid )
 >(select COUNT(*) from Ntl_SupplierOffer where RequestGuid=so.RequestGuid and Price>0 ) then 3 else isnull(Responded,0) end  Responded   

  from Ntl_OfferRequest nof 
                                        left join  Ntl_SupplierOffer so   on nof.RequestGuid=so.RequestGuid
                                        left join Ntl_OfferLine ol on ol.Id=so.LineId 
                                        where nof.ProjectId=@ProjectId  and nof.SupplierRef =@supplierRef  and RequestNr=@RequestNr  ) as tbl	
                                        group by RequestNr ,VatRate ,TrCurr,RequestGuid,Closed,Responded ";
                sqlConnection = new SqlConnection(logoConStr);

                List<Ntl_NetTotal> itemList = sqlConnection.Query<Ntl_NetTotal>(query, new { ProjectId, supplierRef, RequestNr }).ToList();
                if (itemList == null || itemList.Count == 0)
                {

                    list.Add(new Ntl_NetTotal() { RequestNr = RequestNr, VatRate = 0, NetTotal = 0, TrCurr = 0, RequestSend = Requestsend, OfferSend = false, Closed = false });
                }
                else if (itemList.Count == 1 && itemList[0].NetTotal == 0)
                {
                    bool offerSend = false;
                    if (itemList[0].Responded > 0)
                    {
                        offerSend = true;
                    }


                    list.Add(new Ntl_NetTotal() { RequestNr = RequestNr, VatRate = 0, NetTotal = 0, TrCurr = 0, RequestSend = Requestsend, OfferSend = offerSend, RequestGuid = itemList[0].RequestGuid, Closed = itemList[0].Closed, Responded = itemList[0].Responded });
                }
                else
                {

                    foreach (var item in itemList)
                    {

                        if (item.TrCurr == 1)
                        {
                            item.NetTotalWithoutTax = item.NetTotal * Usd;
                            item.NetTotal = (item.NetTotal * ((item.VatRate / 100.0) + 1)) * Usd;
                        }
                        else if (item.TrCurr == 20)
                        {
                            item.NetTotalWithoutTax = item.NetTotal * Eur;
                            item.NetTotal = (item.NetTotal * ((item.VatRate / 100.0) + 1)) * Eur;
                        }
                        else
                        {
                            item.NetTotalWithoutTax = item.NetTotal;
                            item.NetTotal = (item.NetTotal * ((item.VatRate / 100.0) + 1));
                        }



                        list.Add(item);
                    }
                }



            }
            List<Ntl_NetTotal> newlist = new List<Ntl_NetTotal>();

            List<int> requestList = list.Select(x => x.RequestNr).Distinct().ToList();

            foreach (var request in requestList)
            {
                Ntl_NetTotal netTotal = new Ntl_NetTotal()
                {
                    NetTotal = list.Where(x => x.RequestNr == request).Select(x => x.NetTotal).Sum(),
                    NetTotalWithoutTax = list.Where(x => x.RequestNr == request).Select(x => x.NetTotalWithoutTax).Sum(),
                    OfferSend = list.Where(x => x.RequestNr == request).Select(x => x.OfferSend).First(),
                    RequestSend = list.Where(x => x.RequestNr == request).Select(x => x.RequestSend).First(),
                    RequestGuid = list.Where(x => x.RequestNr == request).Select(x => x.RequestGuid).First(),
                    Closed = list.Where(x => x.RequestNr == request).Select(x => x.Closed).First(),
                    Responded = list.Where(x => x.RequestNr == request).Select(x => x.Responded).First(),
                    RequestNr = request

                };
                newlist.Add(netTotal);
            }


            return newlist;
        }

        public string getSupplierMail(int supplierRef)
        {
            string query = $@"  select EMAIL from U_{FirmNr}_ARPS WHERE LOGICALREF =@supplierRef ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<string>(query, new { supplierRef });
        }

        public string getOrderSupplierMail(int ProjectId)
        {
            string query = $@"  select case when SupplierEmail='bos' then '' else SupplierEmail end  from  Ntl_Order  where ProjectId=@ProjectId ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<string>(query, new { ProjectId });
        }
        public string getOrderCCMails(int ProjectId)
        {
            string query = $@"  select CCEmail1 from  Ntl_Order where ProjectId =@ProjectId ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<string>(query, new { ProjectId });
        }
        public int getCommentProjectId(string CommentGuid)
        {
            string query = $@"  SELECT   ProjectId    FROM Ntl_Comment where CommentGuid=@CommentGuid ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<int>(query, new { CommentGuid });
        }
        public int getConfirmProjectId(string ConfirmGuid)
        {
            string query = $@"  SELECT   ProjectId    FROM Ntl_Confirm where ConfirmGuid=@ConfirmGuid ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<int>(query, new { ConfirmGuid });
        }
        public int getInvoiceConfirmProjectId(string ConfirmGuid)
        {
            string query = $@"  SELECT   ProjectId    FROM Ntl_InvoiceConfirm where ConfirmGuid=@ConfirmGuid ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<int>(query, new { ConfirmGuid });
        }

        public int updateInvoiceConfirm(string ConfirmGuid)
        {
            string query = $@"  update     Ntl_InvoiceConfirm set ConfirmStatus=1 ,ConfirmTime=getdate()  where ConfirmGuid=@ConfirmGuid ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<int>(query, new { ConfirmGuid });
        }

        public int getRequestProjectId(string requestGuid)
        {
            string query = $@"  SELECT   ProjectId    FROM Ntl_OfferRequest where RequestGuid=@requestGuid ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<int>(query, new { requestGuid });
        }

        public string getOfferExplainByRequestGuid(string requestGuid)
        {
            string query = $@" SELECT Explain FROM Ntl_OfferRequest where RequestGuid=@requestGuid ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<string>(query, new { requestGuid });
        }

        public Ntl_Comment getCommentByGuid(string CommentGuid)
        {
            string query = $@"  SELECT   *    FROM Ntl_Comment  where CommentGuid=@CommentGuid ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);

            Ntl_Comment comment = sqlConnection.Query<Ntl_Comment>(query, new { CommentGuid }).FirstOrDefault();
            comment.CommentStr = Encoding.GetEncoding(1254).GetString(comment.Comment, 0, comment.Comment.Length);
            return comment;
        }

        public Ntl_Confirm getConfirmByGuid(string ConfirmGuid)
        {
            string query = $@"  SELECT   *    FROM Ntl_Confirm where ConfirmGuid=@ConfirmGuid ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);

            Ntl_Confirm comment = sqlConnection.Query<Ntl_Confirm>(query, new { ConfirmGuid }).FirstOrDefault();
            comment.CommentStr = Encoding.GetEncoding(1254).GetString(comment.Comment, 0, comment.Comment.Length);
            return comment;
        }
        public List<Ntl_Comment> getCommentsByProjectId(int ProjectId)
        {

            List<Ntl_Comment> list = new List<Ntl_Comment>();
            string query = $@" select * from ( SELECT   ProjectId,Comment,PersonName,PersonEmail,CommentTime   FROM Ntl_Comment where ProjectId=@ProjectId
                                    union all
                               SELECT  ProjectId,Comment,PersonName,PersonEmail,CommentTime    FROM Ntl_Confirm where ProjectId=@ProjectId ) as tbl order by CommentTime ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);

            List<Ntl_Comment> comments = sqlConnection.Query<Ntl_Comment>(query, new { ProjectId }).ToList();
            foreach (var comment in comments)
            {
                comment.CommentStr = Encoding.GetEncoding(1254).GetString(comment.Comment, 0, comment.Comment.Length);

                if (!string.IsNullOrEmpty(comment.CommentStr))
                {

                    list.Add(comment);
                }
            }


            return list;
        }
        public int updateComment(string CommentGuid, byte[] Comment)
        {
            string query = $@" update  Ntl_Comment set Comment=@Comment,CommentTime=getdate(),Status_=1 where CommentGuid=@CommentGuid ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Execute(query, new { Comment, CommentGuid });
        }




        public int updateCommentMailTime(string CommentGuid, string Exception_)
        {
            string query = $@" update  Ntl_Comment set MailSendTime=getdate(), Exception_=@Exception_ where CommentGuid=@CommentGuid ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Execute(query, new { CommentGuid, Exception_ });
        }
        public int updateConfirm(string ConfirmGuid, byte[] Comment, int ConfirmStatus, int projectId)
        {
            string query = $@" update  Ntl_Confirm set Comment=@Comment,CommentTime=getdate(),ConfirmStatus=@ConfirmStatus where ConfirmGuid=@ConfirmGuid ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            sqlConnection.Execute(query, new { Comment, ConfirmGuid, ConfirmStatus });

            if (ConfirmStatus == 2)
            {
                query = $@" update  Ntl_Confirm set ConfirmStatus=-2 where ConfirmStatus =-1 and ProjectId=@projectId ";
                sqlConnection = new SqlConnection(logoConStr);
                sqlConnection.Execute(query, new { projectId });
            }





            query = $@"    delete from Ntl_Confirm where ProjectId = @projectId
							 and PersonEmail = ( select PersonEmail from Ntl_Confirm where ConfirmGuid = @ConfirmGuid) 
							 and ConfirmStatus=0";
            sqlConnection = new SqlConnection(logoConStr);
            sqlConnection.Execute(query, new { projectId, ConfirmGuid });
            return 1;
        }

        public int updateProjectExp(string ProjectId, string exp)
        {
            string query = $@" update  Ntl_Offer set SuggestionExplanation=@exp where ProjectId=@ProjectId ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Execute(query, new { ProjectId, exp });
        }
        public int updateProjectOfferExp(string ProjectId, string exp)
        {
            string query = $@" update  Ntl_Offer set Explanation=@exp where ProjectId=@ProjectId ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Execute(query, new { ProjectId, exp });
        }

        public int updateConfirmStatus(int ProjectId, bool status)
        {
            string query = $@" update  Ntl_Offer set WaitingConfirm=@status where ProjectId=@ProjectId ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Execute(query, new { ProjectId, status });
        }
        public int updateRejectConfirmStatus(int ProjectId)
        {
            string query = $@" update  Ntl_Confirm set ConfirmStatus=3 where ProjectId=@ProjectId and ConfirmStatus=2 ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Execute(query, new { ProjectId });
        }

        public int updateProjectSupplier(int SuggestionSupplierRef, int ProjectId)
        {
            string query = $@" update  Ntl_Offer set SuggestionSupplierRef=@SuggestionSupplierRef where ProjectId=@ProjectId ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Execute(query, new { ProjectId, SuggestionSupplierRef });
        }

        public int getSuggestionSupplier(int ProjectId)
        {
            string query = $@"select isnull((select   SuggestionSupplierRef    from Ntl_Offer where ProjectId=@ProjectId),0) ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<int>(query, new { ProjectId });
        }
        public string getCommentPersonMail(int ProjectId)
        {
            string query = $@"select isnull((select   CommentPersonMail    from Ntl_Offer where ProjectId=@ProjectId),'') ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<string>(query, new { ProjectId });
        }
        public string getCommentPerson(int ProjectId)
        {
            string query = $@"select isnull((select   CommentPerson   from Ntl_Offer where ProjectId=@ProjectId),'') ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<string>(query, new { ProjectId });
        }
        public bool getConfirmStatus(int ProjectId)
        {
            string query = $@"  select   WaitingConfirm    from Ntl_Offer where ProjectId=@ProjectId ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<bool>(query, new { ProjectId });
        }
        public bool getSendStatus(int ProjectId)
        {
            string query = $@"  select   IsSend    from Ntl_Offer where ProjectId=@ProjectId ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<bool>(query, new { ProjectId });
        }


        public List<Ntl_ConfirmList> getConfirmList(double limit)
        {
            List<Ntl_ConfirmList> list = new List<Ntl_ConfirmList>();
            try
            {

                string query = $@"  Select * from  Ntl_ConfirmList  where ConfirmLimit<=@limit  Order by  ConfirmNr";
                SqlConnection sqlConnection = new SqlConnection(logoConStr);
                list = sqlConnection.Query<Ntl_ConfirmList>(query, new { limit }).ToList();

                foreach (var item in list)
                {
                    if (item.Code == "01.03.02")
                    {
                        item.Email = "butcekontrol@raysigorta.com.tr";
                        item.FullName = "Bütçe Kontrol";
                    }
                    else
                    {
                        string itemcode = item.Code;

                        query = $@" SELECT  ADSOYAD FullName ,EMAIL Email, HIYERARSI_KODU Code   from NET_HIYERARSI_BILGILERI_Purch where HIYERARSI_KODU=@itemcode";
                        sqlConnection = new SqlConnection(FlowConStr);
                        Ntl_ConfirmList ntl_Confirm = sqlConnection.Query<Ntl_ConfirmList>(query, new { itemcode }).FirstOrDefault();
                        item.Email = ntl_Confirm.Email;
                        item.FullName = ntl_Confirm.FullName;
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }





            return list;
        }


        public List<Ntl_ConfirmList> getConfirmList()
        {
            List<Ntl_ConfirmList> list = new List<Ntl_ConfirmList>();
            try
            {

                string query = $@"  Select * from  Ntl_ConfirmList    Order by  ConfirmNr";
                SqlConnection sqlConnection = new SqlConnection(logoConStr);
                list = sqlConnection.Query<Ntl_ConfirmList>(query).ToList();

                foreach (var item in list)
                {
                    if (item.Code == "01.03.02")
                    {
                        item.Email = "butcekontrol@raysigorta.com.tr";
                        item.FullName = "Bütçe Kontrol";
                    }
                    else
                    {
                        string itemcode = item.Code;

                        query = $@" SELECT  ADSOYAD FullName ,EMAIL Email, HIYERARSI_KODU Code   from NET_HIYERARSI_BILGILERI_Purch where HIYERARSI_KODU=@itemcode";
                        sqlConnection = new SqlConnection(FlowConStr);
                        Ntl_ConfirmList ntl_Confirm = sqlConnection.Query<Ntl_ConfirmList>(query, new { itemcode }).FirstOrDefault();
                        item.Email = ntl_Confirm.Email;
                        item.FullName = ntl_Confirm.FullName;
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }





            return list;
        }

        public Ntl_Confirm getConfirmPerson_(int ProjectId)
        {
            Ntl_Confirm confirm = new Ntl_Confirm();
            string query = $@" Select top 1 * from  Ntl_Confirm where ProjectId=@ProjectId  and isnull(ConfirmStatus,-1)= 0  Order by  ConfirmNr ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            confirm = sqlConnection.Query<Ntl_Confirm>(query, new { ProjectId }).FirstOrDefault();


            return confirm;
        }
        public Ntl_Confirm getConfirmPerson(int ProjectId, string confirmId)
        {
            Ntl_Confirm confirm = new Ntl_Confirm();
            string query = $@" Select top 1 * from  Ntl_Confirm where ProjectId=@ProjectId  and isnull(ConfirmStatus,-1)=-1  and ConfirmNr=isnull( (select ConfirmNr  from Ntl_Confirm where ConfirmGuid=@confirmId),0)+1 Order by  ConfirmNr ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            confirm = sqlConnection.Query<Ntl_Confirm>(query, new { ProjectId, confirmId }).FirstOrDefault();

            if (confirm != null)
            {
                int Id = confirm.Id;

                sqlConnection = new SqlConnection(logoConStr);
                sqlConnection.Execute("update Ntl_Confirm set ConfirmStatus=0 where Id=@Id", new { Id });
            }
            return confirm;
        }

        public List<Ntl_SelectValue> getDepartments(int projectId)
        {
            string query = $@" select distinct SUB_ANLY.CODE value, SUB_ANLY.DESCRIPTION  name from Ntl_OfferLineTrasns olt
                                left join U_001_01_DEMANDTRANS dt on dt.LOGICALREF= olt.Trasnsref
                                left join U_001_01_DEMANDTRANSADS D_ANLY ON D_ANLY.TRANSREF= DT.LOGICALREF
                                LEFT JOIN U_001_ANLYDIMENSIONS ANLY  ON ANLY.LOGICALREF= D_ANLY.ANLYDIMREF
                                LEFT JOIN U_001_ANLYDIMENSIONS SUB_ANLY  ON SUB_ANLY.CODE= SUBSTRING(ANLY.CODE, 1, 10)
                                where olt.OfferLineId in (select Id from Ntl_OfferLine where ProjectId=@projectId )";

            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Query<Ntl_SelectValue>(query, new { projectId }).ToList();

        }

        public Ntl_ProjectStatus getProjectStatus(int projectId)
        {

            string query = $@" select  
                                isnull((select TOP 1  PersonName from Ntl_Comment where ISNULL(Status_,0)=0 and ProjectId=o.ProjectId ),'' ) CommentPerson, 
                                 isnull((select top 1 PersonName from Ntl_Confirm where ISNULL(ConfirmStatus,0)=0 and ProjectId=o.ProjectId  order by Id desc),'') ConfirmPerson,
                                 (select COUNT(*) from Ntl_Confirm where ProjectId=o.ProjectId) ConfirmedPersonel,
                                 isnull(SuggestionSupplierRef,0)  SuggestionSupplierRef , isnull(WaitingConfirm,0) WaitingConfirm,
                                 isnull((select top 1 PersonName from Ntl_Confirm where ISNULL(ConfirmStatus,0)=2 and ProjectId=o.ProjectId  order by Id desc),'') RejectPerson,
                                  isnull(( (select  COUNT(*) from Ntl_Comment where ISNULL(Status_,0)=1 and ProjectId=o.ProjectId )+
								( select  count(*) from Ntl_Confirm where  isnull(cast(cast (Comment as varbinary) as varchar(max)),'')<>'' and ProjectId=o.ProjectId )
								 ),'') CommentCount ,
                                 case when	(select sum(case when isContract=0 then 5 else 0 end ) from Ntl_OfferLine where  ProjectId= o.ProjectId)>0 
							     then 5 else isnull((select TOP 1 Amount from Ntl_PaymentPlan where LineNr=1 and  ProjectId=o.ProjectId),0) end  PaymentPlan 
                                 from Ntl_Offer o where ProjectId=@projectId ";

            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Query<Ntl_ProjectStatus>(query, new { projectId }).FirstOrDefault();

        }
        public Ntl_ProjectStatus getBudgetStatus(int transferLineId)
        {

            string query = $@"   select '' CommentPerson ,isnull((select PersonEmail from  Ntl_BudgetConfirm where ConfirmStatus=0 and   TransferLineId=@transferLineId),'') ConfirmPerson,
                                (select COUNT(*) from  Ntl_BudgetConfirm where ConfirmStatus=1 and   TransferLineId=@transferLineId) ConfirmedPersonel,0 SuggestionSupplierRef ,
                                (select COUNT(*) from  Ntl_BudgetConfirm where ConfirmStatus=0 and  TransferLineId=@transferLineId) WaitingConfirm,
								(select top 1 PersonEmail from  Ntl_BudgetConfirm where ConfirmStatus=2 and  TransferLineId=@transferLineId order by Id desc ) RejectPerson
                                , 0 CommentCount, 0 PaymentPlan                                
                                  from Ntl_BudgetConfirm where 	TransferLineId=@transferLineId ";

            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Query<Ntl_ProjectStatus>(query, new { transferLineId }).FirstOrDefault();

        }

        public List<Ntl_Payplan> getPayplans(int projectId)
        {

            string query = $@"  select * from Ntl_PaymentPlan where ProjectId=@projectId";

            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Query<Ntl_Payplan>(query, new { projectId }).ToList();

        }

        public List<Ntl_Payplan> savePayplans(int projectId, int lineNr, double amount)
        {

            string query = $@"  if exists (select * from Ntl_PaymentPlan  WHERE ProjectId = @projectId and LineNr=@lineNr)
                                begin
                                update Ntl_PaymentPlan set Amount=@amount WHERE ProjectId = @projectId and LineNr=@lineNr
                                end
                                else
                                begin
                                INSERT INTO Ntl_PaymentPlan
                                ( ProjectId
                                , LineNr
                                , Amount )
                                  VALUES
                                ( @projectId
                                , @lineNr
                                , @amount ) end  ";

            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Query<Ntl_Payplan>(query, new { projectId, lineNr, amount }).ToList();

        }




        public void removeOffers(int requestNr, int projectId)
        {
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            string query = $@" delete from Ntl_SupplierOffer where RequestGuid in   (select  RequestGuid from Ntl_OfferRequest where ProjectId=@projectId and RequestNr=@requestNr) ";

            sqlConnection.Execute(query, new { requestNr, projectId });

            sqlConnection = new SqlConnection(logoConStr);
            query = $@" delete from Ntl_OfferRequest where ProjectId=@projectId and RequestNr=@requestNr ";
            sqlConnection.Execute(query, new { requestNr, projectId });


            sqlConnection = new SqlConnection(logoConStr);
            query = $@" update Ntl_OfferRequest set RequestNr=RequestNr-1 where ProjectId=@projectId and RequestNr>@requestNr ";
            sqlConnection.Execute(query, new { requestNr, projectId });

        }
        public void removeOffer(int requestNr, int supplierId, int projectId)
        {
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            string query = $@" delete from Ntl_SupplierOffer where RequestGuid in   (select  RequestGuid from Ntl_OfferRequest where ProjectId=@projectId and RequestNr=@requestNr and SupplierRef=@supplierId) ";

            sqlConnection.Execute(query, new { requestNr, projectId, supplierId });

            sqlConnection = new SqlConnection(logoConStr);
            query = $@" delete from Ntl_OfferRequest where ProjectId=@projectId and RequestNr=@requestNr and SupplierRef=@supplierId";
            sqlConnection.Execute(query, new { requestNr, projectId, supplierId });


            sqlConnection = new SqlConnection(logoConStr);
            query = $@" update Ntl_OfferRequest set RequestNr=RequestNr-1 where ProjectId=@projectId and RequestNr>@requestNr and SupplierRef=@supplierId";
            sqlConnection.Execute(query, new { requestNr, projectId, supplierId });

        }

        public string getProjectStatusForOrder(int projectId)
        {
            string query = $@" select TAXNR from U_001_ARPS ARP
                                inner join Ntl_Offer o on o.SuggestionSupplierRef=ARP.LOGICALREF  where ProjectId=@projectId ";

            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<string>(query, new { projectId });

        }


        public List<Ntl_SelectValue> getDepartmentPersons(string anly_Code)
        {

            if (!string.IsNullOrEmpty(anly_Code))
            {
                string query = $@"  select PER.NAME + ' '+ PER.SURNAME name , LOWER(MAIL.EXP1) value from  H_001_PERSONS PER
                            LEFT JOIN H_001_CONTACTS MAIL ON MAIL.CARDREF= PER.LOGICALREF AND MAIL.TYP= 6
                            LEFT JOIN U_001_ANLYDIMENSIONS ANLY ON PER.ANLYDIMREF= ANLY.LOGICALREF AND ANLY.LEVELNR= 3
                            WHERE ANLY.CODE LIKE @anly_Code+'%' ORDER BY ANLY.CODE ";

                SqlConnection sqlConnection = new SqlConnection(logoConStr);
                List<Ntl_SelectValue> list = sqlConnection.Query<Ntl_SelectValue>(query, new { anly_Code }).ToList();



                return list;
            }
            else
            {
                return new List<Ntl_SelectValue>();
            }

        }
        public Ntl_Comment getWatingComment(int projectId)
        {
            string query = $@" select Id, ProjectId, CommentGuid, Comment, MAIL.EXP1 PersonName, PersonEmail, CommentTime, Status_, MailSendTime, Exception_ 
                        from Ntl_Comment cmn 
                        LEFT JOIN H_001_CONTACTS MAIL ON MAIL.EXP1= cmn.PersonEmail AND MAIL.TYP= 6
                        left join H_001_PERSONS per on per.LOGICALREF=MAIL.CARDREF
                        WHERE isnull(Status_,0) =0 and ProjectId=@projectId";

            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Query<Ntl_Comment>(query, new { projectId }).FirstOrDefault();

        }




        public void UpdateCommentPerson(string personMail, string person, int ProjectId)
        {

            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            sqlConnection.Execute("update Ntl_Offer set CommentPersonMail=@personMail ,CommentPerson=@person where ProjectId=@ProjectId", new { personMail, ProjectId, person });


        }


        public Ntl_Confirm getConfirm(int projectId, string @personMail, string supplier)
        {
            string query = $@"  select top 1 * from (Select nc.[Id], nc.[ProjectId],isnull(o.OfferDate,o.EndDate) Date_ ,  [ConfirmGuid], [Comment], [PersonName], [PersonEmail], [ConfirmStatus], CommentTime ConfirmTime,
                                [ConfirmTime] CommentTime, o.ProjectNr ProjectNo , 1  ConfirmType ,SUBSTRING(isnull(arps.DESCRIPTION,' '),1,25)  Supplier ,arps.DESCRIPTION tedarikci
								from  Ntl_Confirm nc 
                                left join Ntl_Offer o on o.ProjectId=nc.ProjectId 
								left join  U_001_ARPS arps on arps.LOGICALREF=o.SuggestionSupplierRef
								where  isnull(ConfirmStatus,-1) in (0,1,2,3) and  PersonEmail=@personMail     and isnull(cancelled,0)=0 
                                union all 
                                Select   nc.[Id],nc.[ProjectId],isnull(o.OfferDate,o.EndDate) Date_ ,  [ConfirmGuid], [Comment], [PersonName], [PersonEmail], [ConfirmStatus],  ConfirmTime,
                                [ConfirmTime] CommentTime ,o.ProjectNr ProjectNo  , 2 ConfirmType	,SUBSTRING(isnull(arps.DESCRIPTION,' '),1,25)  Supplier ,arps.DESCRIPTION tedarikci
								from  Ntl_InvoiceConfirm nc 
                                left join Ntl_Offer o on o.ProjectId=nc.ProjectId 
								left join  U_001_ARPS arps on arps.LOGICALREF=o.SuggestionSupplierRef
								where  isnull(ConfirmStatus,-1) in (0,1,2) and  PersonEmail=@personMail    and isnull(cancelled,0)=0   
                                union all
                                Select   nc.[Id],nc.[ProjectId],isnull(o.OfferDate,o.EndDate) Date_ ,  CommentGuid, [Comment], [PersonName], [PersonEmail], Status_,CommentTime ConfirmTime,
                                CommentTime ,o.ProjectNr ProjectNo , 3 ConfirmType ,SUBSTRING(isnull(arps.DESCRIPTION,' '),1,25)  Supplier ,arps.DESCRIPTION tedarikci
								from  Ntl_Comment nc 
                                left join Ntl_Offer o on o.ProjectId=nc.ProjectId 
								left join  U_001_ARPS arps on arps.LOGICALREF=o.SuggestionSupplierRef
								where  isnull(Status_,-1) in (0) and  PersonEmail=@personMail   and isnull(cancelled,0)=0  )  as tbl  
								where ProjectId ={projectId}	";

            if (!string.IsNullOrEmpty(supplier))
            {
                query += " and  tedarikci like '%'+@supplier+'%' ";
            }

            query += " order by Id desc  ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Query<Ntl_Confirm>(query, new { personMail, supplier }).FirstOrDefault();
        }

        public Ntl_Confirm getBudgetConfirm(int budgetId, string @personMail, string supplier)
        {
            string query = $@" select top 1 * from (
                        	select bc.Id , btl.Id ProjectId , bt.TransferDate Date_ ,ConfirmGuid,u.FullName [PersonName] ,PersonEmail , ConfirmStatus ,ConfirmTime , 
                        	bt.TransferNo ProjectNo , 4 ConfirmType , 'Bütçe Transferi ' Supplier ,'Bütçe Transferi ' tedarikci
                        	  from 
                        	Ntl_BudgetConfirm bc 
                        	left join  Ntl_BudgetTransferLine  btl on btl.Id=bc.TransferLineId
                        	left join Ntl_BudgetTransfer bt on bt.Id=btl.TransferId
                        	left join Ntl_User u on u.Email=bc.PersonEmail ) as tbl 
								where ProjectId ={budgetId}	";

            if (!string.IsNullOrEmpty(supplier))
            {
                query += " and  tedarikci like '%'+@supplier+'%' ";
            }

            query += " order by Id desc  ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Query<Ntl_Confirm>(query, new { personMail, supplier }).FirstOrDefault();
        }
        public List<Ntl_Confirm> getConfirmList(string mail, Ntl_Filter filter)
        {

            DateTime begdate = filter.BegDate;
            DateTime enddate = filter.EndDate;

            List<Ntl_Confirm> confirms = new List<Ntl_Confirm>();
            List<int> projectList = new List<int>();
            string query = $@"  select distinct * from (
								select ProjectId from Ntl_Confirm where PersonEmail=@mail and (ProjectId in (select ProjectId from Ntl_Offer where  isnull(OfferDate,EndDate)  between @begdate and @enddate  ) or ConfirmStatus=0)
								union all
								select ProjectId from Ntl_InvoiceConfirm where PersonEmail=@mail and ProjectId in (select ProjectId from Ntl_Offer where  isnull(OfferDate,EndDate)  between @begdate and @enddate  )
								union all
								select ProjectId from Ntl_Comment where PersonEmail=@mail and ProjectId in (select ProjectId from Ntl_Offer where  isnull(OfferDate,EndDate)  between @begdate and @enddate  ) ) as tbl  ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            projectList = sqlConnection.Query<int>(query, new { mail, begdate, enddate }).ToList();


            foreach (var project in projectList)
            {
                var projectStatus = getProjectStatus(project);
                if (!string.IsNullOrEmpty(projectStatus.CommentPerson))
                {
                    projectStatus.ProjectStatus = projectStatus.CommentPerson + " adlı personelin yorumu bekleniyor.";
                }
                else if (!string.IsNullOrEmpty(projectStatus.ConfirmPerson))
                {
                    if (projectStatus.ConfirmPerson == "Bütçe Kontrol")
                    {
                        projectStatus.ProjectStatus = "Kontrol ve Planlama departmanından bütçe onayı bekleniyor.";
                    }
                    else
                    {
                        projectStatus.ProjectStatus = projectStatus.ConfirmPerson + " adlı personelin onayi bekleniyor.";
                    }
                }
                else if (projectStatus.RejectPerson != "")
                {

                    projectStatus.ProjectStatus = projectStatus.RejectPerson + " adlı personel onaylamadan geri gönderdi.";
                }
                else
                {
                    if (projectStatus.ConfirmedPersonel > 0)
                    {
                        projectStatus.ProjectStatus = "Proje Onaylandi.";

                    }
                }
                string ProjectStatus = projectStatus.ProjectStatus;

                Ntl_Confirm confirm = getConfirm(project, mail, filter.Supplier);


                if (confirm == null)
                {
                    confirm = new Ntl_Confirm() { ConfirmStatus = -5 };
                }

                confirm.ProjectStatus = ProjectStatus;

                confirms.Add(confirm);
            }

            query = $@" select TransferLineId	from Ntl_BudgetConfirm bc
                                left join Ntl_BudgetTransfer bt on bt.Id=bc.TransferId
                                where PersonEmail=@mail  and bt.TransferDate between  @begdate and @enddate  and isnull(Completed,0)=0 ";
            sqlConnection = new SqlConnection(logoConStr);
            projectList = sqlConnection.Query<int>(query, new { mail, begdate, enddate }).ToList();

            foreach (var item in projectList)
            {

                var budgetStatus = getBudgetStatus(item);


                if (!string.IsNullOrEmpty(budgetStatus.CommentPerson))
                {
                    budgetStatus.ProjectStatus = budgetStatus.CommentPerson + " adlı personelin yorumu bekleniyor.";
                }
                else if (!string.IsNullOrEmpty(budgetStatus.ConfirmPerson))
                {
                    if (budgetStatus.ConfirmPerson == "Bütçe Kontrol")
                    {
                        budgetStatus.ProjectStatus = "Kontrol ve Planlama departmanından bütçe onayı bekleniyor.";
                    }
                    else
                    {
                        budgetStatus.ProjectStatus = budgetStatus.ConfirmPerson + " adlı personelin onayi bekleniyor.";
                    }
                }
                else if (budgetStatus.RejectPerson != "")
                {

                    budgetStatus.ProjectStatus = budgetStatus.RejectPerson + " adlı personel onaylamadan geri gönderdi.";
                }
                else
                {
                    if (budgetStatus.ConfirmedPersonel > 0)
                    {
                        budgetStatus.ProjectStatus = "Proje Onaylandi.";

                    }
                }
                string ProjectStatus = budgetStatus.ProjectStatus;


                Ntl_Confirm budgetConfirm = getBudgetConfirm(item, mail, filter.Supplier);
                budgetConfirm.ProjectStatus = ProjectStatus;
                confirms.Add(budgetConfirm);
            }


            return confirms;
        }

        public int createConfirm(Ntl_Confirm confirm)
        {
            string query = "";
            if (confirm.ConfirmStatus == -10)
            { query = $@" INSERT INTO Ntl_Confirm
           (ProjectId
           ,ConfirmGuid
           ,Comment
           ,PersonName
           ,PersonEmail
           ,ConfirmStatus
           ,ConfirmNr ,CommentTime    )
     VALUES
           (@ProjectId
           ,@ConfirmGuid
           ,@Comment
           ,@PersonName
           ,@PersonEmail
           ,@ConfirmStatus
           ,@ConfirmNr ,getdate()  )"; }
            else
            { query = $@"  if not exists (select * from Ntl_Confirm  WHERE ConfirmStatus = @ConfirmStatus and PersonEmail=@PersonEmail and ProjectId=@ProjectId)
           begin  
            INSERT INTO Ntl_Confirm
           (ProjectId
           ,ConfirmGuid
           ,Comment
           ,PersonName
           ,PersonEmail
           ,ConfirmStatus
           ,ConfirmNr    )
     VALUES
           (@ProjectId
           ,@ConfirmGuid
           ,@Comment
           ,@PersonName
           ,@PersonEmail
           ,@ConfirmStatus
           ,@ConfirmNr ) end "; }

            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<int>(query, confirm);

        }

        public int createInvoiceConfirm(Ntl_InvoiceConfirm confirm)
        {
            string query = $@" INSERT INTO Ntl_InvoiceConfirm
           (ProjectId
           ,ConfirmGuid
           ,Comment
           ,PersonName
           ,PersonEmail
           ,ConfirmStatus,Explanation
            )
     VALUES
           (@ProjectId
           ,@ConfirmGuid
           ,@Comment
           ,@PersonName
           ,@PersonEmail
           ,@ConfirmStatus,@Explanation
            )";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<int>(query, confirm);

        }
        public void InsertErrorLogs(string method, string message, string stacktrace)
        {
            string query = $@" Insert into Ntl_ErrorLogs  (Method,Message,StackTrace) values (@method,@message,@stacktrace)";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            //sqlConnection.Execute(query, new { method, message, stacktrace });

        }

        #region BudgetControl


        public List<Ntl_BudgetControl> getItemForBudget(string ProjectId)
        {
            List<Ntl_BudgetControl> list = new List<Ntl_BudgetControl>();

            string query = $@"   select year(OfferDate) from  Ntl_Offer where ProjectId=@ProjectId   ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            int budgetYear = sqlConnection.Query<int>(query, new { ProjectId }).FirstOrDefault();


            query = $@" select distinct DESCRIPTION4 BudgetCode ,(select month(OfferDate)  from Ntl_Offer where ProjectId=@ProjectId) BudgetMonth  from Ntl_OfferLine ol
                                left join U_001_ITEMS ITM on ITM.LOGICALREF=ol.ItemRef 
                                where ProjectId=@ProjectId";
            sqlConnection = new SqlConnection(logoConStr);
            list = sqlConnection.Query<Ntl_BudgetControl>(query, new { ProjectId }).ToList();


            try
            {
                foreach (var item in list)
                {
                    string budgetCode = item.BudgetCode;
                    query = $@"     select SLIPNR from U_001_01_DEMANDSLIPS WHERE LOGICALREF IN(
                select DEMANDSLIPREF from U_001_01_DEMANDTRANS where LOGICALREF  IN 
                (SELECT Trasnsref FROM  Ntl_OfferLineTrasns where OfferLineId in ( select  Ol.Id BudgetCode from Ntl_OfferLine ol
                                left join U_001_ITEMS ITM on ITM.LOGICALREF=ol.ItemRef 
                                where ProjectId=@ProjectId  and  DESCRIPTION4 =@budgetCode ))) ";
                    sqlConnection = new SqlConnection(logoConStr);
                    List<string> SlipNrList = sqlConnection.Query<string>(query, new { ProjectId, budgetCode }).ToList();
                    string slipList = "";
                    foreach (var SlipNr in SlipNrList)
                    {
                        slipList += ",'" + SlipNr + "'";
                    }
                    if (slipList.Length > 0)
                    {
                        slipList = slipList.Substring(1);
                    }
                    else
                    { slipList = "'Test'"; }
                    query = $@"  select isnull(( select top 1  month(Tarih) from  ButceDetay where FisNo  in ({slipList}) and ButceKodu=@budgetCode order by Tarih ),MONTH(getdate()))    ";
                    sqlConnection = new SqlConnection(BudgetControlConStr);
                    int budgetMonth = sqlConnection.Query<int>(query, new { budgetCode }).FirstOrDefault();




                    query = $@"  select ButceKodu BudgetCode,ButceAdi BudgetName, PLANLANAN Budget, GERCEKLESEN  Gerceklesen  from vw_BudgetControl    
                            WHERE ButceKodu=@budgetCode AND  AY=@budgetMonth and YIL=@budgetYear ";

                    sqlConnection = new SqlConnection(BudgetConStr);
                    item.Budget = sqlConnection.Query<Ntl_Budget>(query, new { budgetCode, budgetMonth , budgetYear }).FirstOrDefault();
                    if (item.Budget == null)
                        item.Budget = new Ntl_Budget();

                    query = $@"  select sum( PLANLANAN) Budget from vw_BudgetControl    
                            WHERE ButceKodu=@budgetCode AND  YIL=@budgetYear";
                    sqlConnection = new SqlConnection(BudgetConStr);
                    item.Budget.BudgetYear = sqlConnection.ExecuteScalar<double>(query, new { budgetCode, budgetYear });

                    query = $@"  select sum(GERCEKLESEN) Budget from vw_BudgetControl    
                            WHERE ButceKodu=@budgetCode AND  YIL=@budgetYear";
                    sqlConnection = new SqlConnection(BudgetConStr);
                    item.Budget.GerceklesenYear = sqlConnection.ExecuteScalar<double>(query, new { budgetCode , budgetYear });




                    query = $@" select isnull(( SELECT  SUM( BlokeTutari)     
                            FROM ButceDetay
                            where FisNo not in ({slipList}) and ButceKodu=@budgetCode
                             and month(Tarih) = {budgetMonth} ),0)   ";

                    sqlConnection = new SqlConnection(BudgetControlConStr);
                    double BlockAmount = sqlConnection.Query<double>(query, new { budgetCode }).FirstOrDefault();


                    item.Budget.BlokeAmount = BlockAmount;

                    query = $@" select isnull(( SELECT  SUM( BlokeTutari)     
                            FROM ButceDetay
                            where FisNo not in ({slipList}) and ButceKodu=@budgetCode ),0)   ";

                    sqlConnection = new SqlConnection(BudgetControlConStr);
                    BlockAmount = sqlConnection.Query<double>(query, new { budgetCode }).FirstOrDefault();
                    item.Budget.Budget = item.Budget.Budget;

                    item.Budget.YillikBlokeAmount = BlockAmount;

                    //item.LineId = 0;
                    item.TransRef = new int[] { };


                }
            }
            catch (Exception ex)
            {
                throw ex;

                //foreach (var item in list)
                //{
                //    item.Budget = new Ntl_Budget();
                //}
            }




            return list.Distinct().ToList();
        }


        #endregion



        #region Budget
        public List<Ntl_SelectValue> getBudgetAccounts()
        {
            string query = $@"  select code value,name  from DimensionBase where type=1  and GCRecord is null order by name";
            SqlConnection sqlConnection = new SqlConnection(BudgetConStr);

            List<Ntl_SelectValue> list = sqlConnection.Query<Ntl_SelectValue>(query).ToList();


            return list;
        }
        public List<Ntl_SelectValue> getBudgetCosts()
        {
            string query = $@"  select code value,name  from DimensionBase where type=3   and GCRecord is null order by name";
            SqlConnection sqlConnection = new SqlConnection(BudgetConStr);

            List<Ntl_SelectValue> list = sqlConnection.Query<Ntl_SelectValue>(query).ToList();


            return list;
        }

        public double getBudgetValue(string Code, string TransferType, string TransferMonth)
        {


            string query = $@" select sum((value*sign)-realizedAmount)  from ValueInfo WHERE Id in (
                select valueInfos from ValueDimensions where dimensions=(select Id from DimensionBase where  GCRecord is null and  code='{Code}')) and value>0 and isCalculated=0";

            if (TransferType == "1")
                query += " and year(Valuedate)=" + (DateTime.Now.Year-1) + " and  Month(Valuedate)>=" + TransferMonth;
            else
                query += " and year(Valuedate)=" + (DateTime.Now.Year - 1) + " and   Month(Valuedate)=" + TransferMonth;

            SqlConnection sqlConnection = new SqlConnection(BudgetConStr);
            double list = sqlConnection.ExecuteScalar<double>(query);
            return list;
        }



        public void updateAmountFromBudget(string Code, string TransferMonth, double Amount)
        {
            int year = (DateTime.Now.Year - 1);
            string query = $@"  update  ValueInfo  set value =value-@Amount where  Id in (
                select valueInfos from ValueDimensions where dimensions=(select Id from DimensionBase where code=@Code  and GCRecord is null)) 
                and   Month(Valuedate)=@TransferMonth and year(Valuedate)=@year and isCalculated=0 and value>=@Amount";
            SqlConnection sqlConnection = new SqlConnection(BudgetConStr);
            sqlConnection.Execute(query, new { Code, TransferMonth, Amount, year });

        }
        public void updateAmountToBudget(string Code, string TransferMonth, double Amount)
        {
            int year = (DateTime.Now.Year - 1);
            string query = $@"  update  ValueInfo  set value =value+@Amount where  Id in (
                select valueInfos from ValueDimensions where dimensions=(select Id from DimensionBase where code=@Code  and GCRecord is null)) 
                and   Month(Valuedate)=@TransferMonth and year(Valuedate)=@year and  isCalculated=0 ";
            SqlConnection sqlConnection = new SqlConnection(BudgetConStr);
            sqlConnection.Execute(query, new { Code, TransferMonth, Amount, year });

        }
        public string getBudgetTransferNo()
        {
            string query = $@" SELECT MAX(substring(TransferNo,9,10) )    
                                FROM Ntl_BudgetTransfer where  TransferNo like 'Btc-' + CONVERT(varchar(4),YEAR(GETDATE()))+'%'";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            string str_Nr = sqlConnection.ExecuteScalar<string>(query);
            int Nr = 0;
            int.TryParse(str_Nr, out Nr);
            Nr++;
            str_Nr = "Btc-" + (DateTime.Now.Year - 1) + Nr.ToString().PadLeft(8, '0');
            return str_Nr;
        }
        public void updateBudgetTransfered(int Id, int transferId)
        {
            string query = $@"update   Ntl_BudgetTransferLine set Transfered=1 where Id=@Id ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            sqlConnection.Execute(query, new { Id });



            query = $@"update   Ntl_BudgetTransfer set Transfered=1 where Id=@transferId ";
            sqlConnection = new SqlConnection(logoConStr);
            sqlConnection.Execute(query, new { transferId });

        }
        public void updateTransferMessage(int Id, string errorMessage)
        {
            string query = $@"update   Ntl_Offer set ErrorMessage=@errorMessage where ProjectId=@Id ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            sqlConnection.Execute(query, new { Id, errorMessage });

        }

        public void updateBudgetBlokeAmount(Ntl_BudgetReturn budgetReturn)
        {
            string query = $@"    update ButceDetay set BlokeTutari=@NetTotal/ (select COUNT(*) from ButceDetay where FisNo=@DemandNr)  where FisNo=@DemandNr and ButceKodu=@BudgetCode ";
            SqlConnection sqlConnection = new SqlConnection(BudgetControlConStr);
            sqlConnection.Execute(query, budgetReturn);
        }
        public void updateBudgetBlokeAmount(int projectId)
        {
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            string query = $@"   select ds.SLIPNR DemandNr, itm.DESCRIPTION4  BudgetCode from Ntl_OfferLineTrasns olt 
                                  left join U_001_01_DEMANDTRANS dt on dt.LOGICALREF=olt.Trasnsref
                                  left join U_001_01_DEMANDSLIPS ds on ds.LOGICALREF=dt.DEMANDSLIPREF
                                  left join U_001_ITEMS itm on itm.LOGICALREF=dt.ITEMREF  
                                  where OfferLineId in (select Id FROM Ntl_OfferLine where ProjectId=@projectId)  ";

            List<Ntl_BudgetReturn> returns = sqlConnection.Query<Ntl_BudgetReturn>(query, new { projectId }).ToList();

            foreach (var budgetReturn in returns)
            {
                query = $@"    update ButceDetay set BlokeTutari=0  where FisNo=@DemandNr and ButceKodu=@BudgetCode ";
                sqlConnection = new SqlConnection(BudgetControlConStr);
                sqlConnection.Execute(query, budgetReturn);
            }


        }



        public List<Ntl_BudgetTransfer> getBudgetTransfers()
        {
            string query = $@" SELECT * 
,case when (select COUNT(*) from Ntl_BudgetConfirm where ConfirmStatus=2 and TransferId=bt.Id and isnull(Completed,0)=0 )>0 then 1 else 0 end HasRejected
,case when (select COUNT(*) from Ntl_BudgetConfirm where ConfirmStatus=1 and TransferId=bt.Id )>0 then 1 else 0 end HasConfirmed
from Ntl_BudgetTransfer bt order by TransferDate desc ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Query<Ntl_BudgetTransfer>(query).ToList();
        }


        public Ntl_BudgetTransfer getBudgetTransfer(int Id)
        {
            Ntl_BudgetTransfer budgetTransfer = new Ntl_BudgetTransfer();


            string query = $@" SELECT *,case when (select COUNT(*) from Ntl_BudgetConfirm where ConfirmStatus=2 and TransferId=bt.Id and isnull(Completed,0)=0 )>0 then 1 else 0 end HasRejected
        from Ntl_BudgetTransfer bt  where Id=@Id";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            budgetTransfer = sqlConnection.Query<Ntl_BudgetTransfer>(query, new { Id }).FirstOrDefault();


            query = $@" SELECT * ,
                        case when (select COUNT(*) from Ntl_BudgetConfirm where ConfirmStatus=2  and isnull(Completed,0)=0 and TransferLineId=btl.Id)>0 then 1 else 0 end  Rejected
                        from Ntl_BudgetTransferLine btl  where TransferId=@Id ";

            sqlConnection = new SqlConnection(logoConStr);
            budgetTransfer.Lines = sqlConnection.Query<Ntl_BudgetTransferLine>(query, new { Id }).ToList();


            return budgetTransfer;
        }
        public int getBudgetTransferType(int Id)
        {

            string query = $@" SELECT TransferType        from Ntl_BudgetTransfer bt  where Id=@Id";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<int>(query, new { Id });

        }
        public int getBudgetTransferMonth(int Id)
        {

            string query = $@" SELECT TransferMonth        from Ntl_BudgetTransfer bt  where Id=@Id";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<int>(query, new { Id });

        }
        public Ntl_BudgetTransferLine getBudgetDetail(int Id)
        {

            string query = $@" SELECT * from Ntl_BudgetTransferLine where Id=@Id ";

            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            var BudgetTransferLine = sqlConnection.Query<Ntl_BudgetTransferLine>(query, new { Id }).FirstOrDefault();

            query = $@" select *   from [RAYJAVA].[dbo].[Ntl_BudgetConfirm]  where TransferLineId=Id  ";

            sqlConnection = new SqlConnection(logoConStr);
            var ExpList = sqlConnection.Query<Ntl_BudgetConfirm>(query, new { Id }).ToList();
            BudgetTransferLine.BudgetExpList.Add(BudgetTransferLine.BudgetExp);
            foreach (var item in ExpList)
            {
                BudgetTransferLine.BudgetExpList.Add(item.Comment);
                BudgetTransferLine.BudgetExpList.Add(item.CommentResponse);
            }

            return BudgetTransferLine;
        }

        public Ntl_BudgetConfirm getRejectDetail(int Id)
        {

            string query = $@"   SELECT bc.Id,u.FullName PersonName , bc.PersonEmail ,bc.Comment, bc.ConfirmTime from Ntl_BudgetConfirm bc
  left join Ntl_User u on u.Email=bc.PersonEmail where TransferLineId=@Id";

            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Query<Ntl_BudgetConfirm>(query, new { Id }).FirstOrDefault();



        }

        public int saveBudgetTransfer(Ntl_BudgetTransfer budgetTransfer)
        {

            string query = $@"  if exists (select * from Ntl_BudgetTransfer  WHERE Id = @Id)
            begin 
            UPDATE Ntl_BudgetTransfer
               SET TransferNo = @TransferNo
                  ,TransferDate = @TransferDate
                  ,TotalAmountTo = @TotalAmountTo
                  ,TotalAmountFrom = @TotalAmountFrom
                  ,TransferType = @TransferType
                  ,TransferMonth = @TransferMonth
                  ,Transfered = @Transfered
                  ,Locked = @Locked
                  ,Calculeted=1
           WHERE Id=@Id
            end 
            else
            begin
           INSERT INTO Ntl_BudgetTransfer
           (TransferNo
           ,TransferDate
           ,TotalAmountTo
           ,TotalAmountFrom
           ,TransferType
           ,TransferMonth
           ,Transfered
           ,Locked
           ,Calculeted)
     VALUES
           (@TransferNo
           ,@TransferDate
           ,@TotalAmountTo
           ,@TotalAmountFrom
           ,@TransferType
           ,@TransferMonth
           ,@Transfered
           ,@Locked
           ,1) SELECT SCOPE_IDENTITY() as Id  end";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            int id = sqlConnection.ExecuteScalar<int>(query, budgetTransfer);
            if (budgetTransfer.Id > 0)
            {
                id = budgetTransfer.Id;
            }

            foreach (var line in budgetTransfer.Lines)
            {
                if (line.AmountFrom > 0)
                {

                    if (line.Id > 0)
                    {
                        line.TransferId = id;
                        query = $@" UPDATE dbo.Ntl_BudgetTransferLine
                               SET LineNr = @LineNr
                                  ,TransferId = @TransferId
                                  ,AccountFrom = @AccountFrom
                                  ,AccountTo = @AccountTo
                                  ,AccountNameFrom = @AccountNameFrom
                                  ,AccountNameTo = @AccountNameTo
                                  ,BranchFrom = @BranchFrom
                                  ,BranchTo = @BranchTo
                                  ,CostCenterFrom = @CostCenterFrom
                                  ,CostCenterTo = @CostCenterTo
                                  ,DepartmentFrom = @DepartmentFrom
                                  ,DepartmentTo = @DepartmentTo
                                  ,AmountFrom = @AmountFrom
                                  ,AmountTo = @AmountTo
                                  ,BudgetFrom = @BudgetFrom
                                  ,BudgetTo = @BudgetTo
                                  ,TransferMonthTo = @TransferMonthTo
                                  ,TransferMonthFrom = @TransferMonthFrom
                                  ,BudgetExp = @BudgetExp
                                  ,Confirmed = @Confirmed
                                  ,DemandPerson=@DemandPerson
                             WHERE Id=@Id";
                    }

                    else
                    {
                        line.TransferId = id;


                        query = $@" INSERT INTO Ntl_BudgetTransferLine
           (LineNr
           ,TransferId
           ,AccountFrom
           ,AccountTo
           ,AccountNameFrom
           ,AccountNameTo
           ,BranchFrom
           ,BranchTo
           ,CostCenterFrom
           ,CostCenterTo
           ,DepartmentFrom
           ,DepartmentTo
           ,AmountFrom
           ,AmountTo
           ,BudgetFrom
           ,BudgetTo
           ,TransferMonthTo
           ,TransferMonthFrom
           ,BudgetExp 
           ,DemandPerson 
)
     VALUES
           (@LineNr
           ,@TransferId
           ,@AccountFrom
           ,@AccountTo
           ,@AccountNameFrom
           ,@AccountNameTo
           ,@BranchFrom
           ,@BranchTo
           ,@CostCenterFrom
           ,@CostCenterTo
           ,@DepartmentFrom
           ,@DepartmentTo
           ,@AmountFrom
           ,@AmountTo
           ,@BudgetFrom
           ,@BudgetTo 
           ,@TransferMonthTo
           ,@TransferMonthFrom
           ,@BudgetExp 
           ,@DemandPerson )";


                    }
                    sqlConnection = new SqlConnection(logoConStr);
                    sqlConnection.Execute(query, line);
                }
            }




            return id;

        }


        public void saveBudgetConfirm(int BudgetId, double limit)
        {
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            string query = $@" INSERT INTO dbo.Ntl_BudgetConfirm
           (TransferId
           ,TransferLineId
           ,ConfirmGuid
           ,Comment
           ,PersonEmail
           ,ConfirmStatus

           )
select TransferId , Id,NEWID(),'',(select top 1 PersonMail from Ntl_BudgetConfirmList order by ConfirmLimit ),0 from Ntl_BudgetTransferLine where TransferId=@BudgetId and AmountTo>@limit ";

            sqlConnection.Execute(query, new { BudgetId, limit });

            query = $@" update Ntl_BudgetTransfer set SendedConfirm=1 where Id=@BudgetId  ";
            sqlConnection.Execute(query, new { BudgetId });

        }
        public void saveBudgetDetailConfirm(int budgetLineId, string mail)
        {
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            string query = $@"   select TransferId , Id,NEWID(),'',(select top 1 PersonMail from Ntl_BudgetConfirmList 
  where PersonMail<>@mail
    order by ConfirmLimit ),0 
  from Ntl_BudgetTransferLine where Id=@budgetLineId and AmountTo>  
  (select ConfirmLimit from Ntl_BudgetConfirmList where PersonMail=@mail ) ";
            sqlConnection.Execute(query, new { budgetLineId, mail });



        }

        public void updateBudgetConfirm(int budgetLineId, int status, string exp, string mail)
        {
            bool completed = false;
            if (status == 1)
            {
                completed = true;
            }

            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            string query = $@" update  dbo.Ntl_BudgetConfirm set ConfirmStatus=@status ,Comment=@exp ,ConfirmTime=getdate() ,  Completed=@completed where TransferLineId=@budgetLineId and  PersonEmail=@mail ";
            sqlConnection.Execute(query, new { budgetLineId, status, exp, mail, completed });
        }
        public void updateBudgetConfirmResponse(int budgetLineId, string exp)
        {
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            string query = $@" update  dbo.Ntl_BudgetConfirm set CommentResponse=@exp ,Completed=1  where TransferLineId=@budgetLineId and  isnull(Completed,0)=0 ";
            sqlConnection.Execute(query, new { budgetLineId, exp });
        }


        public void saveBudgetDetailConfirm(int Id, double limit)
        {
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            string query = $@" INSERT INTO dbo.Ntl_BudgetConfirm
           (TransferId
           ,TransferLineId
           ,ConfirmGuid
           ,Comment
           ,PersonEmail
           ,ConfirmStatus

           )
select TransferId , Id,NEWID(),'',(select top 1 PersonMail from Ntl_BudgetConfirmList order by ConfirmLimit ),0 from Ntl_BudgetTransferLine where Id=@Id and AmountTo>@limit ";

            sqlConnection.Execute(query, new { Id, limit });



        }
        public List<Ntl_BudgetReturn> GetBudgetReturns(int projectId)
        {
            string query = $@" select 
                             DESCRIPTION4 BudgetCode,ds.SLIPNR DemandNr , 
                             sum(  (so.Price*ol.Quantity*case when so.TrCurr=1 then UsdCurr when so.TrCurr=20 then EurCurr else 1 end ) * ((so.VatRate/100)+1)) NetTotal ,ol.isContract
                             from Ntl_OfferLine ol
                             left join Ntl_Offer o on o.ProjectId=ol.ProjectId
                             left join U_001_ITEMS itm on itm.LOGICALREF=ol.ItemRef
                             left join Ntl_OfferLineTrasns olt on olt.OfferLineId=ol.Id
                             left join U_001_01_DEMANDTRANS dt on dt.LOGICALREF=olt.Trasnsref
                             left join U_001_01_DEMANDSLIPS ds on ds.LOGICALREF=dt.DEMANDSLIPREF
                             left join Ntl_SupplierOffer so on so.ProjectId=ol.ProjectId and so.SupplierRef= o.SuggestionSupplierRef and ol.Id=so.LineId
                             left join Ntl_OfferRequest ofr on ofr.RequestGuid=so.RequestGuid
                             where o.ProjectId=@projectId
                             and  ofr.RequestNr =(SELECT MAX(RequestNr)  FROM Ntl_OfferRequest where ProjectId=o.ProjectId and SupplierRef=o.SuggestionSupplierRef )
                             group by DESCRIPTION4 ,ds.SLIPNR,ol.isContract";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Query<Ntl_BudgetReturn>(query, new { projectId }).ToList();
        }


        #endregion


        #region Order

        public string getOrderExplanation(int projectId, int supplierRef)
        {
            string query = $@"  select Explanation from Ntl_Order WHERE ProjectId=@projectId  and SupplierRef=@supplierRef ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<string>(query, new { projectId, supplierRef });
        }

        public DateTime getOrderDueDate(int projectId, int supplierRef)
        {
            string query = $@"  select DueDate from Ntl_Order WHERE ProjectId=@projectId  and SupplierRef=@supplierRef ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.ExecuteScalar<DateTime>(query, new { projectId, supplierRef });
        }

        public Ntl_SupplierOrder getOrder(int projectId)
        {
            Ntl_SupplierOrder supplierOrder = new Ntl_SupplierOrder();
            string query = $@" select  o.ProjectId,ProjectNr,os.SupplierRef ,SupplierCode,SupplierDesc,ord.DueDate ,ord.Explanation,SendedSupplier ,o.IsSend
                                ,isnull(SupplierEmail,'bos') Supplier_EMail ,CCEmail1 CC_EMail1, CCEmail2 CC_EMail2,CCEmail3 CC_EMail3
                                from Ntl_Offer o
                                left join Ntl_OfferSupplier os on os.SupplierRef=o.SuggestionSupplierRef  and os.ProjectId=o.ProjectId
                                left join Ntl_Order ord on ord.ProjectId=o.ProjectId and  ord.SupplierRef=o.SuggestionSupplierRef
                                where o.ProjectId=@projectId ";

            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            supplierOrder = sqlConnection.Query<Ntl_SupplierOrder>(query, new { projectId }).FirstOrDefault();

            supplierOrder.Lines = getOrderLines(projectId);


            List<Ntl_OfferDocs> orfferDocs = getOfferDocs(projectId);

            foreach (var offerDoc in orfferDocs)
            {
                int OfferDocId = offerDoc.Id;

                query = $@" if not exists (select * from Ntl_OrderDocs  where OfferDocId=@OfferDocId )
			                    begin
			                    insert into Ntl_OrderDocs ( [ProjectId], [UploadedFileName], [UploadedFileContentTyp], [UploadedFilePath], [DocumentName], [OrderId], [OfferDocId])
			                    select ProjectId , [UploadedFileName], [UploadedFileContentTyp], [UploadedFilePath], [DocumentName], ProjectId*-1 , Id from Ntl_OfferDocs where Id=@OfferDocId
			                    end";
                sqlConnection = new SqlConnection(logoConStr);
                sqlConnection.Execute(query, new { OfferDocId });

            }
            sqlConnection = new SqlConnection(logoConStr);
            query = $@" select Id,UploadedFileName ,DocumentName from Ntl_OrderDocs supp where ProjectId=@projectId ";

            supplierOrder.RequestGuid = supplierOrder.Lines.Select(x => x.RequestGuid).First();

            supplierOrder.DocList = sqlConnection.Query<Ntl_OrderDocs>(query, new { projectId }).ToList();

            if (supplierOrder.Supplier_EMail != "bos")
            {
                int supplier = supplierOrder.SupplierRef;
                sqlConnection = new SqlConnection(logoConStr);
                supplierOrder.Supplier_EMail = sqlConnection.ExecuteScalar<string>("select EMAIL from U_001_ARPS WHERE LOGICALREF=@supplier", new { supplier });
            }
            else
            {
                supplierOrder.Supplier_EMail = "";
            }
            return supplierOrder;
        }

        public int updateOrderDocs(int projectId)
        {


            string query = $@"   update [Ntl_OrderDocs] set OrderId=(select Id from Ntl_Order  WHERE  ProjectId = @projectId) where OrderId=@projectId*-1 ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Execute(query, new { projectId });
        }
        public int saveSupplierOrder(Ntl_SupplierOrder order)
        {
            string query = $@"  if exists (select * from Ntl_Order  WHERE SupplierRef=@SupplierRef and ProjectId = @ProjectId )
                                begin
                                UPDATE Ntl_Order
                             SET      
                                  Explanation = @Explanation
                                  ,DueDate = @DueDate
                                  ,RequestGuid = @RequestGuid
                                  ,SendedSupplier = @SendedSupplier
                                  ,SupplierEmail=  isnull(@Supplier_EMail,'')
                                  ,CCEmail1=  @CC_EMail1
                                  ,CCEmail2=  @CC_EMail2
                                  ,CCEmail3=  @CC_EMail3
                                   WHERE SupplierRef=@SupplierRef and ProjectId = @ProjectId
                                    end
                                    else
                                    begin
                                    INSERT INTO dbo.Ntl_Order
                                               (ProjectId
                                               ,SupplierRef
                                               ,Explanation
                                               ,DueDate
                                               ,RequestGuid
                                               ,SendedSupplier 
                                               ,SupplierEmail
                                               ,CCEmail1
                                               ,CCEmail2 
                                               ,CCEmail3 )
                                         VALUES
                                               (@ProjectId
                                               ,@SupplierRef
                                               ,@Explanation
                                               ,@DueDate
                                               ,@RequestGuid
                                               ,@SendedSupplier
                                               ,@Supplier_EMail 
                                               ,@CC_EMail1 
                                               ,@CC_EMail2 
                                               ,@CC_EMail3 )
                                    end ";


            SqlConnection sqlConnection = new SqlConnection(logoConStr);

            return sqlConnection.Execute(query, order);
        }
        public int updateSupplierOrder(int SupplierRef, int ProjectId)
        {
            string query = $@"  update Ntl_Order set SendedSupplier=1 WHERE SupplierRef=@SupplierRef and ProjectId = @ProjectId ";

            SqlConnection sqlConnection = new SqlConnection(logoConStr);

            return sqlConnection.Execute(query, new { ProjectId, SupplierRef });
        }

        public List<Ntl_SupplierOrderLine> getOrderLines(int projectId)
        {
            List<Ntl_SupplierOrderLine> lines = new List<Ntl_SupplierOrderLine>();
            string query = $@"select ofr.RequestGuid,
                    ol.ItemRef,ItemDesc, Price, case when TrCurr=1 then UsdCurr when TrCurr=20 then EurCurr else 1 end trrate 
                    ,Case when TrCurr=1 then 'USD' when TrCurr=20 then 'EUR' ELSE 'TL'  END TrCurr,
                    ol.Quantity ,ol.VatRate,
                    case when o.isDeliveryAddress =1 then DeliveryAddress else ADDR.ADRES end Address,
                    ADDR.TELEFON Telephone ,
                    UNIT.CODE Unit,
                    ISNULL(SUB_ANLY.DESCRIPTION,'')  Department    
                    from Ntl_Offer o
                    left join Ntl_OfferLine ol on o.ProjectId=ol.ProjectId
                    left join Ntl_SupplierOffer so on so.ProjectId = o.ProjectId  and SupplierRef = o.SuggestionSupplierRef and ol.Id= so.LineId
                    left join Ntl_OfferSupplier os on os.SupplierRef= o.SuggestionSupplierRef and o.ProjectId=os.ProjectId
                    left join Ntl_OfferRequest ofr on ofr.RequestGuid= so.RequestGuid
                    left join Ntl_OfferLineTrasns olt on olt.OfferLineId=ol.Id
                    left join U_001_01_DEMANDTRANS dt on dt.LOGICALREF= olt.Trasnsref
                    left join U_001_01_DEMANDTRANSADS DT_ANLY ON DT_ANLY.TRANSREF = dt.LOGICALREF
                    left join U_001_ANLYDIMENSIONS ANLY ON ANLY.LOGICALREF = DT_ANLY.ANLYDIMREF
                    LEFT JOIN U_001_ANLYDIMENSIONS SUB_ANLY  ON SUB_ANLY.CODE= SUBSTRING(ANLY.CODE, 1, 6)
                    LEFT JOIN NET_PERSONEL_ADRES_BILGILERI ADDR ON ADDR.SICIL_NUMARASI = SUBSTRING(ANLY.CODE,LEN(ANLY.CODE)-3,5)
                    LEFT JOIN U_001_UNITS UNIT ON UNIT.LOGICALREF= DT.UOMREF
                    where RequestNr = (select MAX(RequestNr) from Ntl_OfferRequest where ProjectId=@projectId and SupplierRef = SuggestionSupplierRef and Responded=1 )
                    and o.ProjectId=@projectId  ";

            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            lines = sqlConnection.Query<Ntl_SupplierOrderLine>(query, new { projectId }).ToList();



            return lines;
        }



        public string createSupplierExcel(int projectId)
        {

            List<Ntl_SupplierOrderLine> orderLines = getOrderLines(projectId);
            string projectNr = getProjeNo(projectId);



            string path = AppDomain.CurrentDomain.BaseDirectory + "ExcelFolder\\";
            string fileName = projectNr + "_" + DateTime.Now.ToString("ddMMyyyy_HHmmss") + ".xlsx";

            DataTable dt = new DataTable();

            dt.Columns.Add("Malzeme Adı");
            dt.Columns.Add("Miktar");
            dt.Columns.Add("Birim");
            dt.Columns.Add("Birim Fiyat");
            dt.Columns.Add("Döviz Türü");
            dt.Columns.Add("KDV(%)");
            dt.Columns.Add("Net Toplam(TL)");
            dt.Columns.Add("Departman");
            dt.Columns.Add("Adres");
            dt.Columns.Add("Telefon");


            DataRow tempRow = dt.NewRow();

            foreach (var line in orderLines)
            {
                line.NetTotal = (line.Price * line.TrRate * line.Quantity) * ((line.VatRate / 100) + 1);
                tempRow = dt.NewRow();
                tempRow[0] = line.ItemDesc;
                tempRow[1] = line.Quantity.ToString("n2");
                tempRow[2] = line.Unit;
                tempRow[3] = line.Price.ToString("n2");
                tempRow[4] = line.TrCurr;
                tempRow[5] = line.VatRate.ToString("n2");
                tempRow[6] = line.NetTotal.ToString("n2");
                tempRow[7] = line.Department;
                tempRow[8] = line.Address;
                tempRow[9] = line.Telephone;
                dt.Rows.Add(tempRow);
            }
            WriteExcelFile(path + fileName, dt);
            return path + fileName;
        }
        public string createOfferExcel(int projectId)
        {

            Ntl_Offer offer = getWaitingOffer(projectId);
            string projectNr = getProjeNo(projectId);



            string path = AppDomain.CurrentDomain.BaseDirectory + "ExcelFolder\\";
            string fileName = projectNr + "_" + DateTime.Now.ToString("ddMMyyyy_HHmmss") + ".xlsx";

            DataTable dt = new DataTable();
            dt.Columns.Add("Talep No");
            dt.Columns.Add("Talep Eden");
            dt.Columns.Add("Grup Kodu");
            dt.Columns.Add("Malzeme Açıklaması");
            dt.Columns.Add("Miktar");
            dt.Columns.Add("Birim");
            dt.Columns.Add("Kdv Hariç Birim Fiyat");
            dt.Columns.Add("KDV Hariç Toplam Tutar");
            dt.Columns.Add("Kdv Oranı %");
            dt.Columns.Add("KDV Dahil Toplam Tutar");
            dt.Columns.Add("Açıklama");
            dt.Columns.Add("Teslimat Adresi");
            DataRow tempRow = dt.NewRow();

            foreach (var line in offer.Lines) //this will also include your header row...
            {
                tempRow = dt.NewRow();
                tempRow[0] = line.SlipNr;
                tempRow[1] = line.PersonName;
                tempRow[2] = line.ItemGrpCode;
                tempRow[3] = line.ItemDesc;
                tempRow[4] = line.Quantity.ToString("n2");
                tempRow[5] = line.Unit;
                tempRow[6] = line.LastPurchPrice.ToString("n2");
                tempRow[7] = line.Total.ToString("n2");
                tempRow[8] = line.VatRate.ToString("n2");
                tempRow[9] = line.NetTotal.ToString("n2");
                tempRow[10] = offer.Explanation;
                tempRow[11] = offer.DeliveryAddress;
                dt.Rows.Add(tempRow);
            }
            WriteExcelFile(path + fileName, dt);
            return path + fileName;
        }



        private void WriteExcelFile(string outputPath, DataTable table)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(outputPath, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                worksheetPart.Worksheet = new Worksheet(sheetData);

                Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Sheet1" };

                sheets.Append(sheet);

                Row headerRow = new Row();

                List<string> columns = new List<string>();
                foreach (System.Data.DataColumn column in table.Columns)
                {
                    columns.Add(column.ColumnName);

                    Cell cell = new Cell();
                    cell.DataType = CellValues.String;
                    cell.CellValue = new CellValue(column.ColumnName);
                    headerRow.AppendChild(cell);
                }

                sheetData.AppendChild(headerRow);

                foreach (DataRow dsrow in table.Rows)
                {
                    Row newRow = new Row();
                    foreach (string col in columns)
                    {
                        Cell cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(dsrow[col].ToString());
                        newRow.AppendChild(cell);
                    }

                    sheetData.AppendChild(newRow);
                }

                workbookPart.Workbook.Save();
            }
        }

        public Ntl_Anly GetAnly(int lineId)
        {
            string query = $@" SELECT isnull(anly.LOGICALREF,57) Logicalref ,isnull(anly.CODE,'100.01.001') Code
                             FROM Ntl_OfferLineTrasns olt
                             left join Ntl_OfferLine ol on ol.Id = olt.OfferLineId
                             left join U_001_01_DEMANDTRANSADS d_anly on d_anly.TRANSREF = olt.Trasnsref
                             left join U_001_ANLYDIMENSIONS anly on anly.LOGICALREF = d_anly.ANLYDIMREF
                             where ol.Id = @lineId ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Query<Ntl_Anly>(query, new { lineId }).FirstOrDefault();

        }

        public Ntl_Anly GetAnlyInfo(string code)
        {
            string query = $@" SELECT isnull(LOGICALREF,57) Logicalref ,isnull(CODE,'100.01.001') Code ,isnull(DESCRIPTION,'100.01.001') Description
                             FROM  U_001_ANLYDIMENSIONS 
                             where CODE=@code";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            return sqlConnection.Query<Ntl_Anly>(query, new { code }).FirstOrDefault();

        }


        #endregion



        #region Email
        //IDARIISLERSATINALMA@raysigorta.com.tr
        public string getMailBodyForOffer(Ntl_Offer offer, string itemGroups, string logoId, string offerId, string link, bool revize)
        {
            string body = string.Empty;
            try
            {


                StreamReader reader;
                string path = System.AppDomain.CurrentDomain.BaseDirectory;
                if (revize)
                    reader = new StreamReader(path + "/Templates/SupplierOfferRevizeTemplate.html");
                else
                    reader = new StreamReader(path + "/Templates/SupplierOfferTemplate.html");

                body = reader.ReadToEnd();


                body = body.Replace("{ProjeNo}", offer.ProjectNr);
                body = body.Replace("{OfferDate}", offer.OfferDate.ToString("dd.MM.yyyy"));
                body = body.Replace("{ButceKalemi}", itemGroups);
                body = body.Replace("{EndDate}", offer.EndDate.ToString("dd.MM.yyyy"));
                string time = offer.EndTime.Hours + ":" + offer.EndTime.Minutes;


                body = body.Replace("{EndTime}", time);
                body = body.Replace("{offerLink}", link);
                body = body.Replace("{RayLogoId}", logoId);
                body = body.Replace("{offerId}", offerId);


                return body;

            }
            catch (Exception ex)
            {
                InsertErrorLogs("sendReminderEmailService", ex.Message, ex.StackTrace);

                return body;
            }
        }

        public string getMailBodyForOrder(int ProjectId)
        {
            string body = string.Empty;
            try
            {
                Ntl_SupplierOrder order = getOrder(ProjectId);


                List<Ntl_SupplierOrderLine> orderLines = getOrderLines(order.ProjectId);
                Ntl_OfferSupplier supplier = getSupplierByRef(order.SupplierRef);
                StreamReader reader;
                string path = System.AppDomain.CurrentDomain.BaseDirectory;
                reader = new StreamReader(path + "/Templates/SupplierOrderTemplate.html");

                body = reader.ReadToEnd();


                body = body.Replace("{ProjeNo}", order.ProjectNr);
                body = body.Replace("{DueDate}", order.DueDate.ToString("dd.MM.yyyy"));
                body = body.Replace("{SupplierCode}", supplier.SupplierCode);
                body = body.Replace("{SupplierName}", supplier.SupplierDesc);
                body = body.Replace("{Explantion}", order.Explanation);

                string items = "";
                double netTotal = 0;
                foreach (var item in orderLines)
                {
                    items += "<tr style=\"height: 30px\">";
                    items += "<td width=\"30%\" style=\"text-align:left;border-bottom:1px solid lightgray\">" + item.ItemDesc + " </td>";
                    items += "<td width=\"10%\" style=\"text-align:center;border-bottom:1px solid lightgray\">" + item.Quantity.ToString("n2") + "</td>";
                    items += "<td width=\"10%\" style=\"text-align:center;border-bottom:1px solid lightgray\">" + item.Unit + "</td>";
                    items += "<td width=\"10%\" style=\"text-align:center;border-bottom:1px solid lightgray\">" + item.Price.ToString("n2") + "</td>";
                    items += "<td width=\"10%\" style=\"text-align:center;border-bottom:1px solid lightgray\">" + item.TrCurr + "</td>";
                    items += "<td width=\"10%\" style=\"text-align:center;border-bottom:1px solid lightgray\">" + item.VatRate + "</td>";
                    items += "<td width=\"10%\" style=\"text-align:center;border-bottom:1px solid lightgray\">" + ((item.Price * item.Quantity * item.TrRate) * ((item.VatRate / 100) + 1)).ToString("n2") + "</td>";
                    items += "<td width=\"10%\" style=\"text-align: center;border-bottom:1px solid lightgray \">" + item.Address + "</td>";
                    items += "</tr>";
                    netTotal += ((item.Price * item.Quantity * item.TrRate) * ((item.VatRate / 100) + 1));
                }
                body = body.Replace("{NetTotal}", netTotal.ToString("n2"));

                body = body.Replace("{ItemList}", items);

                return body;

            }
            catch (Exception ex)
            {
                InsertErrorLogs("sendReminderEmailService", ex.Message, ex.StackTrace);

                return body;
            }
        }

        public string getMailBodyForOfferNotify(int ProjectId)
        {
            string body = string.Empty;
            try
            {
                Ntl_Offer offer = getWaitingOffer(ProjectId);



                StreamReader reader;
                string path = System.AppDomain.CurrentDomain.BaseDirectory;
                reader = new StreamReader(path + "/Templates/OfferNotifyTemplate.html");
                Ntl_PersonInfo personInfo = getProjectRespnsible(ProjectId);
                body = reader.ReadToEnd();




                string items = "";

                foreach (var item in offer.Lines)
                {
                    items += "            <tr>";
                    items += "       <td>" + item.ItemDesc;
                    items += "       </td>";
                    items += "       <td align=\"right\">" + item.Quantity.ToString("n2");
                    items += "       </td>";
                    items += "       <td>" + item.Unit;
                    items += "       </td>";
                    items += "       <td align=\"right\">";
                    items += "       </td>";
                    items += "       <td align=\"right\">";
                    items += "    </td>";
                    items += "    <td align=\"right\">";
                    items += "    </td>";
                    if (!offer.isDeliveryAddress)
                    {
                        items += "    <td>" + item.Address;
                        items += "    </td>";

                        body = body.Replace("{teslimatAdresiLine}", "<td style=\"max-width:320px; min - width:320px\">Teslimat Adresi</td>");
                    }
                    else
                    {
                        body = body.Replace("{teslimatAdresiLine}", "");


                    }
                    items += "</tr>";

                }
                body = body.Replace("{projectno}", offer.ProjectNr);
                body = body.Replace("{enddate}", offer.EndDate.ToString("dd.MM.yyyy"));
                body = body.Replace("{person}", personInfo.PersonName);
                body = body.Replace("{tel}", personInfo.Phone1);
                body = body.Replace("{mail}", personInfo.PersonEmail);
                body = body.Replace("{itemlist}", items);
                body = body.Replace("{aciklama}", offer.Explanation);
                if (!offer.isDeliveryAddress)
                {
                    body = body.Replace("{teslimatAdresi}", "Teslimat Adresi : " + offer.DeliveryAddress);

                }
                else
                {

                    body = body.Replace("{teslimatAdresi}", "");
                }
                return body;

            }
            catch (Exception ex)
            {
                InsertErrorLogs("sendReminderEmailService", ex.Message, ex.StackTrace);

                return body;
            }
        }

        public bool sendEmailService(string email, Guid newRequestGuid, string baseUrl)
        {
            try
            {
                Ntl_OfferRequest request = getRequest(newRequestGuid.ToString());
                Ntl_Offer offer = getWaitingOffer(request.ProjectId);
                string itemGroups = getItemGroups(request.ProjectId);
                baseUrl = "https://teklifver.raysigorta.com.tr/";
                // baseUrl = "http://89.0.0.212:8090/";
                MailMessage Email = new MailMessage();
                SmtpClient mailClient = new SmtpClient();


                if (System.Text.RegularExpressions.Regex.IsMatch(email, "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*") == true)
                {
                    Email.To.Add(email);
                }
                Email.IsBodyHtml = true;

                string link = baseUrl + "Offer/OfferGet?RequestId=" + newRequestGuid.ToString();
                if (offer.OfferCount == 1)
                {
                    string logoId = Guid.NewGuid().ToString();
                    string offerId = Guid.NewGuid().ToString();
                    AlternateView htmlView = AlternateView.CreateAlternateViewFromString(getMailBodyForOffer(offer, itemGroups, logoId, offerId, link, false), null, "text/html");
                    string path = System.AppDomain.CurrentDomain.BaseDirectory;
                    LinkedResource theEmailImage = new LinkedResource(path + "Templates\\button_teklif_ver.png");
                    theEmailImage.ContentId = offerId;
                    htmlView.LinkedResources.Add(theEmailImage);

                    theEmailImage = new LinkedResource(path + "Templates\\RaySigorta.png");
                    theEmailImage.ContentId = logoId;
                    htmlView.LinkedResources.Add(theEmailImage);
                    Email.AlternateViews.Add(htmlView);
                    Email.Subject = $@"[{offer.ProjectNr}] Nolu ihale için teklif talebi hk.";

                }
                else
                {
                    Email.Subject = $@"[{offer.ProjectNr}] Nolu İhale Revize Teklif Talebi ";
                    string logoId = Guid.NewGuid().ToString();
                    string offerId = Guid.NewGuid().ToString();
                    AlternateView htmlView = AlternateView.CreateAlternateViewFromString(getMailBodyForOffer(offer, itemGroups, logoId, offerId, link, true), null, "text/html");
                    string path = System.AppDomain.CurrentDomain.BaseDirectory;
                    LinkedResource theEmailImage = new LinkedResource(path + "Templates\\button_teklif_ver.png");
                    theEmailImage.ContentId = offerId;
                    htmlView.LinkedResources.Add(theEmailImage);

                    theEmailImage = new LinkedResource(path + "Templates\\RaySigorta.png");
                    theEmailImage.ContentId = logoId;
                    htmlView.LinkedResources.Add(theEmailImage);
                    Email.AlternateViews.Add(htmlView);

                }

                mailClient.Host = "89.0.0.72";
                mailClient.Port = 25;
                Email.From = new MailAddress("teklif@raysigorta.com.tr");
                mailClient.Send(Email);



            }

            catch (Exception ex)
            {

                InsertErrorLogs("sendReminderEmailService", ex.Message, ex.StackTrace);
            }

            return true;
        }

        public bool sendReminderEmailService(string email, Guid newRequestGuid, string baseUrl)
        {
            try
            {
                Ntl_OfferRequest request = getRequest(newRequestGuid.ToString());
                Ntl_Offer offer = getWaitingOffer(request.ProjectId);
                string itemGroups = getItemGroups(request.ProjectId);
                baseUrl = "https://teklifver.raysigorta.com.tr/";
                // baseUrl = "http://89.0.0.212:8090/";
                MailMessage Email = new MailMessage();
                SmtpClient mailClient = new SmtpClient();


                if (System.Text.RegularExpressions.Regex.IsMatch(email, "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*") == true)
                {
                    Email.To.Add(email);
                }
                Email.IsBodyHtml = true;
                string link = baseUrl + "Offer/OfferGet?RequestId=" + newRequestGuid.ToString();
                Email.Subject = $@"[{offer.ProjectNr}] Nolu ihale için teklif talebi hk.";
                Email.Body = $@"       <p> Ray Sigorta A.Ş. nin  {DateTime.Today.ToString("dd.MM.yyyy")} tarihinde başlatmış olduğu  {offer.ProjectNr} Nolu {itemGroups} Grup(ları) için ürün alımı ile ilgili teklif girişi için hatırlatmadır. </p> 
                    <br/>   <p>    Aşağıdaki link  üzerinden  {offer.EndDate.ToString("dd.MM.yyyy")} tarihine kadar teklifinizi girebilirsiniz. </p>  <br/> 
                    <br/>  <br/>  <p> Teşekkürler </p> 
 <br/>  <br/>  <br/>  " + link;
                mailClient.Host = "89.0.0.72";
                mailClient.Port = 25;
                Email.From = new MailAddress("teklif@raysigorta.com.tr");
                mailClient.Send(Email);

            }

            catch (Exception ex)
            {

                InsertErrorLogs("sendReminderEmailService", ex.Message, ex.StackTrace);
            }

            return true;
        }




        public bool sendCommentEmail(string email, Guid newRequestGuid, string baseUrl)
        {
            try
            {
                baseUrl = "https://teklifver.raysigorta.com.tr/";

                MailMessage Email = new MailMessage();
                SmtpClient mailClient = new SmtpClient();



                if (System.Text.RegularExpressions.Regex.IsMatch(email, "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*") == true)
                {
                    Email.To.Add(email);
                }

                Email.IsBodyHtml = true;
                string link = baseUrl + "Comment/Comment?CommentId=" + newRequestGuid.ToString();
                Email.Body = "Oluşturduğunuz talep(ler) için yorumunuz isteniyor;" + Environment.NewLine + "  Aşağıdaki linkten Teklif girişi yapabilirsiniz. <br/>  " + link;
                mailClient.Host = "89.0.0.72";
                mailClient.Port = 25;
                Email.From = new MailAddress("teklif@raysigorta.com.tr");
                mailClient.Send(Email);



            }

            catch (Exception ex)
            {


            }

            return true;
        }


        public bool sendSupplierOrderEmail(string email, int supplierRef, int ProjectId, string excelPath)
        {
            try
            {


                string[] mails = null;
                string[] ccmails = null;
                email = getOrderSupplierMail(ProjectId);
                string ccmail = getOrderCCMails(ProjectId);
                if (!string.IsNullOrEmpty(email))
                    mails = email.Split(';');
                if (!string.IsNullOrEmpty(ccmail))
                    ccmails = ccmail.Split(';');
                string explanation = getOrderExplanation(ProjectId, supplierRef);
                DateTime date = getOrderDueDate(ProjectId, supplierRef);


                MailMessage Email = new MailMessage();
                SmtpClient mailClient = new SmtpClient();
                if (mails != null)
                {
                    foreach (var item in mails)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            if (System.Text.RegularExpressions.Regex.IsMatch(item, "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*") == true)
                            {
                                Email.To.Add(item);
                            }
                        }
                    }
                }
                if (ccmails != null)
                {

                    foreach (var item in ccmails)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            if (System.Text.RegularExpressions.Regex.IsMatch(item, "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*") == true)
                            {
                                Email.CC.Add(item);
                            }
                        }
                    }
                }

                Email.IsBodyHtml = true;

                List<Ntl_OrderDocs> orderDocs = getOrderDocs(ProjectId);
                foreach (var orderDoc in orderDocs)
                {
                    if (!string.IsNullOrEmpty(orderDoc.UploadedFilePath))
                    {
                        byte[] fileBytes = System.IO.File.ReadAllBytes(orderDoc.UploadedFilePath);
                        Attachment att = new Attachment(new MemoryStream(fileBytes), orderDoc.UploadedFileName);
                        Email.Attachments.Add(att);
                    }


                }
                Email.Body = " Gönderdiğiniz teklif kabul edilmiştir. <br/>   <br/> ";
                Email.Body += getMailBodyForOrder(ProjectId);

                mailClient.Host = "89.0.0.72";
                mailClient.Port = 25;
                Email.From = new MailAddress("teklif@raysigorta.com.tr");
                mailClient.Send(Email);

            }

            catch (Exception ex)
            {


            }

            return true;
        }


        public bool sendOfferNotifyEmail(int ProjectId)
        {
            try
            {
                string email = "idariislersatinalma@raysigorta.com.tr";
                var offer = getWaitingOfferForNotify(ProjectId);
                MailMessage Email = new MailMessage();
                SmtpClient mailClient = new SmtpClient();
                string itemGroups = getItemGroups(ProjectId);

                if (System.Text.RegularExpressions.Regex.IsMatch(email, "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*") == true)
                {
                    Email.To.Add(email);
                }
                //if (System.Text.RegularExpressions.Regex.IsMatch("deniz.kurt@netline.net.tr", "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*") == true)
                //{
                //    Email.Bcc.Add("deniz.kurt@netline.net.tr");
                //}
                Email.IsBodyHtml = true;
                //byte[]  fileBytes = System.IO.File.ReadAllBytes(excelPath);
                //Attachment att = new Attachment(new MemoryStream(fileBytes), "TeklifUrunleri.xlsx");
                //Email.Attachments.Add(att);
                List<Ntl_OfferDocs> orderDocs = getOfferDocs(ProjectId);
                foreach (var orderDoc in orderDocs)
                {
                    byte[] fileBytes = System.IO.File.ReadAllBytes(orderDoc.UploadedFilePath);
                    Attachment att = new Attachment(new MemoryStream(fileBytes), orderDoc.UploadedFileName);
                    Email.Attachments.Add(att);
                }


                if (offer.OfferCount == 1)
                {
                    Email.Subject = $@"[{offer.ProjectNr}] Nolu İhale Teklif Talebi ";
                    Email.Body = $@" Ray Sigorta A.Ş. nin { offer.OfferDate.ToString("dd.MM.yyyy")} tarihinde başlatmış 
                        olduğu { offer.ProjectNr} Nolu {itemGroups} Grubu için ürünleri ile ilgili teklif alım süreci başlatılmıştır.<br/>
                  ";
                }
                else
                {
                    Email.Subject = $@"[{offer.ProjectNr}] İhale  {(offer.OfferCount - 1).ToString()}. Revize  Teklif Talebi ";
                    Email.Body = $@"   Ray Sigorta A.Ş.nin  { offer.OfferDate.ToString("dd.MM.yyyy")}
                tarihinde başlatmış olduğu  { offer.ProjectNr}
                Nolu { itemGroups}
                Grubu için ürün alımı ile ilgili ihale sürecimizde ilk değerlendirmelerimiz sonrasında revize tekliflerin alınmasına karar verilmiştir.<br/>
             ";
                }
                Email.Body += getMailBodyForOfferNotify(ProjectId);

                mailClient.Host = "89.0.0.72";
                mailClient.Port = 25;
                Email.From = new MailAddress("teklif@raysigorta.com.tr");
                mailClient.Send(Email);


            }
            catch (Exception ex)
            {


            }

            return true;
        }

        public bool sendRejectEmail(string RejectPerson, int ProjectId)
        {
            try
            {
                string email = getProjectUserMail(ProjectId);
                string projectNo = getProjeNo(ProjectId);




                MailMessage Email = new MailMessage();
                SmtpClient mailClient = new SmtpClient();


                if (System.Text.RegularExpressions.Regex.IsMatch(email, "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*") == true)
                {
                    Email.To.Add(email);
                }

                Email.IsBodyHtml = true;



                Email.Body = projectNo + " nolu proje " + RejectPerson + " tarafından onaylanmadan  geri gönderildi.";
                mailClient.Host = "89.0.0.72";
                mailClient.Port = 25;
                Email.From = new MailAddress("teklif@raysigorta.com.tr");
                mailClient.Send(Email);

            }

            catch (Exception ex)
            {


            }

            return true;
        }

        public bool sendConfirmEmail(int ProjectId)
        {
            try
            {
                string email = getProjectUserMail(ProjectId);
                string projectNo = getProjeNo(ProjectId);

                MailMessage Email = new MailMessage();
                SmtpClient mailClient = new SmtpClient();
                if (System.Text.RegularExpressions.Regex.IsMatch(email, "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*") == true)
                {
                    Email.To.Add(email);
                }

                Email.IsBodyHtml = true;
                Email.Body = projectNo + " nolu proje onaylandı.";
                mailClient.Host = "89.0.0.72";
                mailClient.Port = 25;
                Email.From = new MailAddress("teklif@raysigorta.com.tr");
                mailClient.Send(Email);

            }

            catch (Exception ex)
            {


            }

            return true;
        }


        public bool deleteOfferInfoEmail(int ProjectId, int supplierId, int requesNr, int userId, string exp)
        {
            try
            {
                string email = "idariislersatinalma@raysigorta.com.tr";
                // email = "ismail.ozer@netline.net.tr";
                string projectNo = getProjeNo(ProjectId);
                var userInfo = getUserById(userId);
                string mailBody = "";
                if (supplierId > 0)
                {
                    var supplier = getSupplierByRef(supplierId);
                    mailBody = projectNo + " nolu projede " + supplier.SupplierDesc + " tedarikçisine ait " + requesNr + ". teklif " + userInfo.FullName + "  tarafından silinmiştir.</br>";
                    mailBody += " Silinme Sebebi : " + exp;
                }
                else
                {
                    var supplier = getSupplierByRef(supplierId);
                    mailBody = projectNo + " nolu projede " + requesNr + ". teklifler " + userInfo.FullName + "  tarafından silinmiştir.</br>";
                    mailBody += " Silinme Sebebi : " + exp;
                }
                MailMessage Email = new MailMessage();
                SmtpClient mailClient = new SmtpClient();
                if (System.Text.RegularExpressions.Regex.IsMatch(email, "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*") == true)
                {
                    Email.To.Add(email);
                }
                //if (System.Text.RegularExpressions.Regex.IsMatch("deniz.kurt@netline.net.tr", "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*") == true)
                //{
                //    Email.Bcc.Add("deniz.kurt@netline.net.tr");
                //}
                Email.IsBodyHtml = true;
                Email.Subject = projectNo + " nolu proje hakkında ";
                Email.Body = mailBody;
                mailClient.Host = "89.0.0.72";
                mailClient.Port = 25;
                Email.From = new MailAddress("teklif@raysigorta.com.tr");
                mailClient.Send(Email);
            }

            catch (Exception ex)
            {


            }

            return true;
        }

        public bool sendConfirmEmail(string email, Guid confirmGuid, bool budget)
        {
            try
            {
                string baseUrl = "https://teklifver.raysigorta.com.tr/";
                // baseUrl = "http://89.0.0.212:8090/";
                MailMessage Email = new MailMessage();
                SmtpClient mailClient = new SmtpClient();


                if (System.Text.RegularExpressions.Regex.IsMatch(email, "\\w+([-+.']\\w+)*@\\w+([-.]\\w+)*\\.\\w+([-.]\\w+)*") == true)
                {
                    Email.To.Add(email);
                }
                Email.IsBodyHtml = true;
                string link = "";
                if (budget)
                {
                    link = baseUrl + "Confirm/BudgetConfirm?ConfirmId=" + confirmGuid.ToString();
                }
                else
                {
                    link = baseUrl + "Confirm/Confirm?ConfirmId=" + confirmGuid.ToString();
                }
                Email.Body = " Oluşturulan teklifler için onayınız isteniyor." + Environment.NewLine + "  Aşağıdaki linkten onay girişi yapabilirsiniz. <br/>  " + link;
                mailClient.Host = "89.0.0.72";
                mailClient.Port = 25;
                Email.From = new MailAddress("massrap@raysigorta.com.tr");
                mailClient.Send(Email);

            }

            catch (Exception ex)
            {


            }

            return true;
        }
        #endregion
        #region User

        public List<Ntl_User> getUsers()
        {
            List<Ntl_User> list = new List<Ntl_User>();
            string query = $@"  Select * from  Ntl_User ";
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            list = sqlConnection.Query<Ntl_User>(query).ToList();
            return list;
        }
        public Ntl_User getUser(int Id)
        {
            Ntl_User user = new Ntl_User();
            string query = $@"  Select * from  Ntl_User where Id=" + Id;
            SqlConnection sqlConnection = new SqlConnection(logoConStr);
            user = sqlConnection.Query<Ntl_User>(query).FirstOrDefault();
            user.Password = Crypter.Decrypt(user.Password, crypText);

            return user;
        }
        #endregion

        public static bool checkForSQLInjection(string userInput)

        {

            bool isSQLInjection = false;

            string[] sqlCheckList = { "--",
                                       ";--",
                                       ";",
                                       "/*",
                                       "*/",
                                        "@@",
                                        "@",
                                        "char",
                                       "nchar",
                                       "varchar",
                                       "nvarchar",
                                       "alter",
                                       "begin",
                                       "cast",
                                       "create",
                                       "cursor",
                                       "declare",
                                       "delete",
                                       "drop",
                                       "end",
                                       "exec",
                                       "union",
                                       "all",
                                       "execute",
                                       "fetch",
                                            "insert",
                                          "kill",
                                             "select",
                                           "sys",
                                            "sysobjects",
                                            "syscolumns",
                                           "table",
                                           "update"

                                       };

            string CheckString = userInput.Replace("'", "''");

            for (int i = 0; i <= sqlCheckList.Length - 1; i++)
            {
                if ((CheckString.IndexOf(sqlCheckList[i], StringComparison.OrdinalIgnoreCase) >= 0))

                { isSQLInjection = true; }
            }

            return isSQLInjection;
        }
    }
}

