﻿/********************************************************
 * Project Name   : VAdvantage
 * Class Name     : MRevenueRecognitionPlan
 * Purpose        : Revenue Recognition Plan.
 * Class Used     : X_C_RevenueRecognition_Plan
 * Chronological    Development
 * Raghunandan      19-Jan-2010
  ******************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using VAdvantage.DataBase;
using VAdvantage.Logging;
using VAdvantage.Model;
using VAdvantage.Utility;

namespace ViennaAdvantage.Model
{
    /// <summary>
    /// Revenue Recognition Plan
    /// </summary>
    public class MRevenueRecognitionPlan : X_C_RevenueRecognition_Plan
    {
        private static VLogger _log = VLogger.GetVLogger(typeof(MRevenueRecognitionPlan).FullName);
        /// <summary>
        /// 	Standard Constructor
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="C_RevenueRecognition_Plan_ID"></param>
        /// <param name="trxName"></param>
        public MRevenueRecognitionPlan(Ctx ctx, int C_RevenueRecognition_Plan_ID, Trx trxName)
            : base(ctx, C_RevenueRecognition_Plan_ID, trxName)
        {
            if (C_RevenueRecognition_Plan_ID == 0)
            {
                //	setC_AcctSchema_ID (0);
                //	setC_Currency_ID (0);
                //	setC_InvoiceLine_ID (0);
                //	setC_RevenueRecognition_ID (0);
                //	setC_RevenueRecognition_Plan_ID (0);
                //	setP_Revenue_Acct (0);
                //	setUnEarnedRevenue_Acct (0);
                SetTotalAmt(Env.ZERO);
                SetRecognizedAmt(Env.ZERO);
            }
        }

        /// <summary>
        /// Load Constructor
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="idr"></param>
        /// <param name="trxName"></param>
        public MRevenueRecognitionPlan(Ctx ctx, DataRow rs, Trx trxName) : base(ctx, rs, trxName) { }

        /// <summary>
        /// After Save
        /// </summary>
        /// <param name="newRecord"></param>
        /// <param name="success"></param>
        /// <returns>Success</returns>
        /// 
        public void SetRecognitionPlan(MInvoiceLine invoiceLine, MInvoice invoice, int C_RevenueRecognition_ID)
        {
            try
            {
                SetAD_Client_ID(invoice.GetAD_Client_ID());
                SetAD_Org_ID(invoice.GetAD_Org_ID());
                SetC_Currency_ID(invoice.GetC_Currency_ID());
                SetC_InvoiceLine_ID(invoiceLine.GetC_InvoiceLine_ID());
                SetC_RevenueRecognition_ID(C_RevenueRecognition_ID);
                SetP_Revenue_Acct(0);
                SetUnEarnedRevenue_Acct(0);
                SetTotalAmt(invoiceLine.GetLineNetAmt());
                SetRecognizedAmt(Env.ZERO);
            }
            catch (Exception ex)
            {
                // MessageBox.Show("MInvoiceLine--SetInvoice");
            }
        }
        protected override bool AfterSave(bool newRecord, bool success)
        {
            if (newRecord)
            {
                MRevenueRecognition rr = new MRevenueRecognition(GetCtx(), GetC_RevenueRecognition_ID(), Get_TrxName());
                if (rr.IsTimeBased())
                {
                    /**	Get InvoiveQty
                    SELECT	QtyInvoiced, M_Product_ID 
                      INTO	v_Qty, v_M_Product_ID
                    FROM	C_InvoiceLine 
                    WHERE 	C_InvoiceLine_ID=:new.C_InvoiceLine_ID;
                    --	Insert
                    AD_Sequence_Next ('C_ServiceLevel', :new.AD_Client_ID, v_NextNo);
                    INSERT INTO C_ServiceLevel
                        (C_ServiceLevel_ID, C_RevenueRecognition_Plan_ID,
                        AD_Client_ID,AD_Org_ID,IsActive,Created,CreatedBy,Updated,UpdatedBy,
                        M_Product_ID, Description, ServiceLevelInvoiced, ServiceLevelProvided,
                        Processing,Processed)
                    VALUES
                        (v_NextNo, :new.C_RevenueRecognition_Plan_ID,
                        :new.AD_Client_ID,:new.AD_Org_ID,'Y',SysDate,:new.CreatedBy,SysDate,:new.UpdatedBy,
                        v_M_Product_ID, NULL, v_Qty, 0,
                        'N', 'N');
                    **/
                }
            }
            return success;
        }	//	afterSave

        public static MRevenueRecognitionPlan[] GetRecognitionPlans(MRevenueRecognition revenueRecognition, int invoiceLine_ID)
        {
            List<MRevenueRecognitionPlan> list = new List<MRevenueRecognitionPlan>();
            string sql = "Select * from C_RevenueRecognition_Plan pl";
            if (invoiceLine_ID > 0)
            {
                sql += " inner join c_invoiceline invl on invl.c_invoiceline_id = pl.c_invoiceline_id Where pl.C_RevenueRecognition_ID=" + revenueRecognition.GetC_RevenueRecognition_ID() + "" +
                        " and invl.c_invoiceLine_id=" + invoiceLine_ID;
            }
            else
            {
                sql += " Where pl.C_RevenueRecognition_ID=" + revenueRecognition.GetC_RevenueRecognition_ID();
            }
            DataTable dt = null;
            IDataReader idr = null;
            try
            {
                idr = DB.ExecuteReader(sql, null, revenueRecognition.Get_Trx());
                dt = new DataTable();
                dt.Load(idr);
                idr.Close();
                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(new MRevenueRecognitionPlan(revenueRecognition.GetCtx(), dr, revenueRecognition.Get_Trx()));
                }
            }
            catch (Exception e)
            {
                if (idr != null)
                {
                    idr.Close();
                }
                _log.Log(Level.SEVERE, sql, e);
            }
            finally
            {
                dt = null;
            }

            MRevenueRecognitionPlan[] retValue = new MRevenueRecognitionPlan[list.Count];
            retValue = list.ToArray();
            return retValue;
        }
    }
}
