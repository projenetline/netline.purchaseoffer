using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Netline.Ray.Integration
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        System.Timers.Timer timerForService;
        SqlConnectionStringBuilder connStr=new SqlConnectionStringBuilder();

        protected override void OnStart(string[] args)
        {
            connStr.DataSource = "89.0.0.179";
            connStr.InitialCatalog = "RAYJAVA";
            connStr.UserID = "rayadm";
            connStr.Password = "#Wf5$GefhLVx";

        

            timerForService = new System.Timers.Timer(1000 * 60 * 5);
            timerForService.Start();
            timerForService.Elapsed += new System.Timers.ElapsedEventHandler(timerForService_Elapsed);
        }
        private void timerForService_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timerForService.Stop();
            string query=$@" select INVBUDGET.BUDGETID,INVBUDGET.AMOUNT  , INVBUDGET.LOGICALREF from U_001_01_INVOICES INV
                            INNER JOIN X_001_01_CT_INVBUDGET INVBUDGET  ON INVBUDGET.INVOICEREF=INV.LOGICALREF
                            WHERE SLIPTYPE=1 AND BOSTATUS=1 AND PARENTREF=0  and INVBUDGET.BUDGETID>0 ";

            SqlConnection sql= new SqlConnection(connStr.ConnectionString);
            List<Budget> list =    sql.Query<Budget>(query).ToList();
            foreach (var budget in list)
            {
                query = $@"update  RAYSIGORTA_BUTCE..ButceDetay  set BlokeTutari=BlokeTutari-@AMOUNT where Id=@BUDGETID";
                if (sql.Execute(query, budget) > 0)
                {
                    query = $@"update  X_001_01_CT_INVBUDGET  set PARENTREF=@BUDGETID where LOGICALREF=@LOGICALREF";
                    sql.Execute(query, budget);
                }
            }



            query = $@" INSERT INTO [dbo].[X_001_01_CT_BUDGET]
           ([LOGICALREF]
           ,[PARENTREF]
           ,[DEMANDNR]
           ,[BUDGET]
           ,[MONTH_]
           ,[AMOUNT]
           ,[BUDGETID]
           ,[INVOICEREF]
           ,[TE_RECSTATUS]
           ,[TE_WPIID]
           ,[TE_WFIID]
           ,[TE_RIGHTS]
           ,[TE_LABELS]
           ,[ORDERREF])


            SELECT NEXT VALUE FOR X_001_01_CT_BUDGETSEQ,0,FisNo,gl.DESCRIPTION,
            case MONTH(Tarih) 
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
            WHEN 12 THEN 'ARALIK' END,BD.BlokeTutari,BD.Id,0,1,1,1,NULL,NULL,ODT.ORDSLIPREF
            
            
            FROM RAYSIGORTA_BUTCE..[ButceDetay] BD
            LEFT JOIN RAYJAVA..U_001_GLACCOUNTS GL ON GL.EXTDESCRIPTION=BD.ButceKodu
            LEFT JOIN RAYJAVA..U_001_01_DEMANDSLIPS SLP ON BD.FisNo=slp.SLIPNR
            LEFT JOIN RAYJAVA..U_001_01_DEMANDTRANS TRN ON SLP.LOGICALREF=TRN.DEMANDSLIPREF
            LEFT JOIN RAYJAVA..U_001_01_ORDERTRANS ODT ON TRN.LOGICALREF=ODT.SRCDEMTRANSREF
            
            
            WHERE slp.AUXCODE IN('G','CG') AND ORDSLIPREF>0 AND BD.FisNo NOT IN(SELECT DEMANDNR FROM [X_001_01_CT_BUDGET]) ";

            sql = new SqlConnection(connStr.ConnectionString);
            sql.Execute(query);


            timerForService.Start();
        }
        protected override void OnStop()
        {
        }



    }
    public class Budget
    {
        public int BUDGETID { get; set; }
        public int LOGICALREF { get; set; }
        public int AMOUNT { get; set; }

    }
}
