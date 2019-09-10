﻿/********************************************************
    * Project Name   : VAdvantage
    * Class Name     : InOutCreateInvoice
    * Purpose        : Create (Generate) Invoice from Shipment
    * Class Used     : ProcessEngine.SvrProcess
    * Chronological    Development
    * Raghunandan     20-Aug-2009
******************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VAdvantage.Classes;
using VAdvantage.Common;
using VAdvantage.Process;
using VAdvantage.Model;
using VAdvantage.DataBase;
using VAdvantage.SqlExec;
using VAdvantage.Utility;
using System.Windows.Forms;

using System.Data;
using System.Data.SqlClient;
using VAdvantage.ProcessEngine;


namespace ViennaAdvantage.Process
{
    public class InOutCreateInvoice : VAdvantage.ProcessEngine.SvrProcess
    {
        //	Shipment					
        private int _M_InOut_ID = 0;
        //Price List Version		
        private int _M_PriceList_ID = 0;
        //Document No					
        private String _InvoiceDocumentNo = null;

        //Checkbox for Generating charges proportional to line quantities on Invoice line. By Sukhwnder on 14 Dec, 2017
        // Generating charge make as default true as GOMEL Customer want
        private bool _GenerateCharges = true;


        /// <summary>
        /// Prepare - e.g., get Parameters.
        /// </summary>
        protected override void Prepare()
        {
            ProcessInfoParameter[] para = GetParameter();
            for (int i = 0; i < para.Length; i++)
            {
                String name = para[i].GetParameterName();
                if (para[i].GetParameter() == null)
                {
                    ;
                }
                else if (name.Equals("M_PriceList_ID"))
                {
                    _M_PriceList_ID = para[i].GetParameterAsInt();
                }
                else if (name.Equals("InvoiceDocumentNo"))
                {
                    _InvoiceDocumentNo = (String)para[i].GetParameter();
                }
                else if (name.Equals("GenerateCharges"))
                {
                    _GenerateCharges = "Y".Equals(para[i].GetParameter());
                }
                else
                {
                    //log.log(Level.SEVERE, "Unknown Parameter: " + name);
                }
            }
            _M_InOut_ID = GetRecord_ID();
        }

        /// <summary>
        /// Create Invoice.
        /// </summary>
        /// <returns>document no</returns>
        protected override String DoIt()
        {
            //log.info("M_InOut_ID=" + _M_InOut_ID 
            //    + ", M_PriceList_ID=" + _M_PriceList_ID
            //    + ", InvoiceDocumentNo=" + _InvoiceDocumentNo);
            //  Message Display Wrong  Done  Vikas and Assigned by Gurinder
            int count = Util.GetValueOfInt(DB.ExecuteScalar(" SELECT  Count(*)  FROM M_Inout WHERE  ISSOTRX='Y' AND  M_Inout_ID=" + GetRecord_ID()));
            MInOut ship = null;
            if (count > 0)
            {
                if (_M_InOut_ID == 0)
                {
                    throw new ArgumentException("No Shipment");
                }
                //
                ship = new MInOut(GetCtx(), _M_InOut_ID, Get_Trx());
                if (ship.Get_ID() == 0)
                {
                    throw new ArgumentException("Shipment not found");
                }
                if (!MInOut.DOCSTATUS_Completed.Equals(ship.GetDocStatus()))
                {
                    throw new ArgumentException("Shipment not completed");
                }
            }
            else
            {
                if (_M_InOut_ID == 0)
                {
                    throw new ArgumentException("No Material Receipt");
                }
                //
                ship = new MInOut(GetCtx(), _M_InOut_ID, Get_Trx());
                if (ship.Get_ID() == 0)
                {
                    throw new ArgumentException("Material Receipt not found");
                }
                if (!MInOut.DOCSTATUS_Completed.Equals(ship.GetDocStatus()))
                {
                    throw new ArgumentException("Material Receipt not completed");
                }
            }
            //***********************END*****************************//
            MInvoice invoice = new MInvoice(ship, null);
            //-------------Column Added by Anuj----------------------
            //int _CountVA009 = Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(AD_MODULEINFO_ID) FROM AD_MODULEINFO WHERE PREFIX='VA009_'  AND IsActive = 'Y'"));
            int _CountVA009 = Env.IsModuleInstalled("VA009_") ? 1 : 0;

            // Code by Mohit Amortization process
            //int _CountVA038 = Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(AD_MODULEINFO_ID) FROM AD_MODULEINFO WHERE PREFIX='VA038_'  AND IsActive = 'Y'"));
            int _CountVA038 = Env.IsModuleInstalled("VA038_") ? 1 : 0;
            // End Amortization Code

            if (_CountVA009 > 0)
            {
                int _PaymentMethod_ID = Util.GetValueOfInt(DB.ExecuteScalar("Select VA009_PaymentMethod_ID From C_Order Where C_Order_ID=" + ship.GetC_Order_ID()));
                if (_PaymentMethod_ID > 0)
                {
                    invoice.SetVA009_PaymentMethod_ID(_PaymentMethod_ID);
                }

            }
            //-------------Column Added by Anuj----------------------

            // added by Amit 26-may-2016
            //int _CountVA026 = Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(AD_MODULEINFO_ID) FROM AD_MODULEINFO WHERE PREFIX='VA026_'  AND IsActive = 'Y'"));
            int _CountVA026 = Env.IsModuleInstalled("VA026_") ? 1 : 0;
            if (_CountVA026 > 0)
            {
                MOrder order = new MOrder(GetCtx(), ship.GetC_Order_ID(), Get_Trx());
                if (order != null && order.GetC_Order_ID() > 0)
                {
                    invoice.SetVA026_LCDetail_ID(order.GetVA026_LCDetail_ID());
                }
            }
            //end

            if (ship.IsReturnTrx())
            {
                if (!ship.IsSOTrx())
                {
                    // Purchase Return
                    // set target document from documnet type window -- based on documnet type available on material receipt / return to vendor
                    if (invoice.GetC_DocTypeTarget_ID() == 0)
                    {
                        int C_DocTypeTarget_ID = DB.GetSQLValue(null,
                      "SELECT C_DocTypeInvoice_ID FROM C_DocType WHERE C_DocType_ID=@param1",
                      ship.GetC_DocType_ID());
                        if (C_DocTypeTarget_ID >= 0)
                            invoice.SetC_DocTypeTarget_ID(C_DocTypeTarget_ID);
                        else
                            invoice.SetC_DocTypeTarget_ID(ship.IsSOTrx() ? MDocBaseType.DOCBASETYPE_ARCREDITMEMO : MDocBaseType.DOCBASETYPE_APCREDITMEMO);
                    }
                    invoice.SetIsReturnTrx(ship.IsReturnTrx());
                    invoice.SetIsSOTrx(ship.IsSOTrx());
                }
                else
                {
                    // Sales Return
                    if (ship.GetC_Order_ID() >= 0)
                    {
                        MOrder order = new MOrder(GetCtx(), ship.GetC_Order_ID(), Get_Trx());
                        MDocType dt = MDocType.Get(GetCtx(), order.GetC_DocType_ID());
                        if (dt.GetC_DocTypeInvoice_ID() != 0)
                            invoice.SetC_DocTypeTarget_ID(dt.GetC_DocTypeInvoice_ID(), true);
                        else
                            invoice.SetC_DocTypeTarget_ID(ship.IsSOTrx() ? MDocBaseType.DOCBASETYPE_ARCREDITMEMO : MDocBaseType.DOCBASETYPE_APCREDITMEMO);
                    }
                    else
                    {
                        invoice.SetC_DocTypeTarget_ID(ship.IsSOTrx() ? MDocBaseType.DOCBASETYPE_ARCREDITMEMO : MDocBaseType.DOCBASETYPE_APCREDITMEMO);
                    }
                }
            }
            if (_M_PriceList_ID != 0)
            {
                invoice.SetM_PriceList_ID(_M_PriceList_ID);
            }
            if (_InvoiceDocumentNo != null && _InvoiceDocumentNo.Length > 0)
            {
                invoice.SetDocumentNo(_InvoiceDocumentNo);
            }

            // Added by Bharat on 30 Jan 2018 to set Inco Term from Order

            if (invoice.Get_ColumnIndex("C_IncoTerm_ID") > 0)
            {
                invoice.SetC_IncoTerm_ID(ship.GetC_IncoTerm_ID());
            }

            if (!invoice.Save())
            {
                //SI_0708 - message was not upto the mark
                ValueNamePair pp = VAdvantage.Logging.VLogger.RetrieveError();
                if (pp != null)
                    throw new ArgumentException("Cannot save Invoice. " + pp.GetName());
                throw new ArgumentException("Cannot save Invoice");
                //return GetReterivedError(invoice, "Cannot save Invoice");
            }
            MInOutLine[] shipLines = ship.GetLines(false);
            DateTime? AmortStartDate = null;
            DateTime? AmortEndDate = null;
            count = 0;
            DataSet ds = null;

            for (int i = 0; i < shipLines.Length; i++)
            {
                MInOutLine sLine = shipLines[i];
                // Changes done by Bharat on 06 July 2017 restrict to create invoice if Invoice already created against that for same quantity
                string sql = @"SELECT ml.QtyEntered - SUM(COALESCE(li.QtyEntered,0)) as QtyEntered, ml.MovementQty-SUM(COALESCE(li.QtyInvoiced,0)) as QtyInvoiced 
                FROM M_InOutLine ml INNER JOIN C_InvoiceLine li ON li.M_InOutLine_ID = ml.M_InOutLine_ID INNER JOIN C_Invoice ci ON ci.C_Invoice_ID = li.C_Invoice_ID 
                WHERE ci.DocStatus NOT IN ('VO', 'RE') AND ml.M_InOutLine_ID =" + sLine.GetM_InOutLine_ID() + " GROUP BY ml.MovementQty, ml.QtyEntered";
                ds = DB.ExecuteDataset(sql, null, Get_Trx());
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    decimal qtyEntered = Util.GetValueOfDecimal(ds.Tables[0].Rows[0][0]);
                    decimal qtyInvoiced = Util.GetValueOfDecimal(ds.Tables[0].Rows[0][1]);
                    if (qtyEntered <= 0)
                    {
                        ds.Dispose();
                        log.Info("Invoice Line already exist for Receipt Line ID - " + sLine.GetM_InOutLine_ID());
                        continue;
                    }
                    else
                    {
                        MInvoiceLine line = new MInvoiceLine(invoice);
                        line.SetShipLine(sLine);
                        //line.SetQtyEntered(sLine.GetQtyEntered());
                        //line.SetQtyInvoiced(sLine.GetMovementQty());
                        line.SetQtyEntered(qtyEntered);
                        line.SetQtyInvoiced(qtyInvoiced);
                        // Change By Mohit Amortization process -------------
                        if (_CountVA038 > 0)
                        {
                            if (line.GetM_Product_ID() > 0)
                            {
                                MProduct pro = new MProduct(GetCtx(), sLine.GetM_Product_ID(), Get_TrxName());
                                if (Util.GetValueOfInt(pro.Get_Value("VA038_AmortizationTemplate_ID")) > 0)
                                {
                                    line.Set_Value("VA038_AmortizationTemplate_ID", Util.GetValueOfInt(pro.Get_Value("VA038_AmortizationTemplate_ID")));
                                    DataSet amrtDS = DB.ExecuteDataset("SELECT VA038_AmortizationType,VA038_AmortizationPeriod,VA038_TermSource,VA038_PeriodType,Name FROM VA038_AmortizationTemplate WHERE IsActive='Y' AND VA038_AMORTIZATIONTEMPLATE_ID=" + Util.GetValueOfInt(pro.Get_Value("VA038_AmortizationTemplate_ID")));
                                    AmortStartDate = null;
                                    AmortEndDate = null;
                                    if (Util.GetValueOfString(amrtDS.Tables[0].Rows[0]["VA038_TermSource"]) == "A")
                                    {
                                        AmortStartDate = invoice.GetDateAcct();
                                    }
                                    if (Util.GetValueOfString(amrtDS.Tables[0].Rows[0]["VA038_TermSource"]) == "T")
                                    {
                                        AmortStartDate = invoice.GetDateInvoiced();
                                    }

                                    if (Util.GetValueOfString(amrtDS.Tables[0].Rows[0]["VA038_PeriodType"]) == "M")
                                    {
                                        AmortEndDate = AmortStartDate.Value.AddMonths(Util.GetValueOfInt(amrtDS.Tables[0].Rows[0]["VA038_AmortizationPeriod"]));
                                    }
                                    if (Util.GetValueOfString(amrtDS.Tables[0].Rows[0]["VA038_PeriodType"]) == "Y")
                                    {
                                        AmortEndDate = AmortStartDate.Value.AddYears(Util.GetValueOfInt(amrtDS.Tables[0].Rows[0]["VA038_AmortizationPeriod"]));
                                    }
                                    line.Set_Value("FROMDATE", AmortStartDate);
                                    line.Set_Value("EndDate", AmortEndDate);
                                    if (amrtDS != null)
                                    {
                                        amrtDS.Dispose();
                                    }
                                }
                            }
                            if (line.GetC_Charge_ID() > 0)
                            {
                                MCharge charge = new MCharge(GetCtx(), sLine.GetC_Charge_ID(), Get_TrxName());
                                if (Util.GetValueOfInt(charge.Get_Value("VA038_AmortizationTemplate_ID")) > 0)
                                {
                                    line.Set_Value("VA038_AmortizationTemplate_ID", Util.GetValueOfInt(charge.Get_Value("VA038_AmortizationTemplate_ID")));
                                    DataSet amrtDS = DB.ExecuteDataset("SELECT VA038_AmortizationType,VA038_AmortizationPeriod,VA038_TermSource,VA038_PeriodType,Name FROM VA038_AmortizationTemplate WHERE IsActive='Y' AND VA038_AMORTIZATIONTEMPLATE_ID=" + Util.GetValueOfInt(charge.Get_Value("VA038_AmortizationTemplate_ID")));
                                    AmortStartDate = null;
                                    AmortEndDate = null;
                                    if (Util.GetValueOfString(amrtDS.Tables[0].Rows[0]["VA038_TermSource"]) == "A")
                                    {
                                        AmortStartDate = invoice.GetDateAcct();
                                    }
                                    if (Util.GetValueOfString(amrtDS.Tables[0].Rows[0]["VA038_TermSource"]) == "T")
                                    {
                                        AmortStartDate = invoice.GetDateInvoiced();
                                    }

                                    if (Util.GetValueOfString(amrtDS.Tables[0].Rows[0]["VA038_PeriodType"]) == "M")
                                    {
                                        AmortEndDate = AmortStartDate.Value.AddMonths(Util.GetValueOfInt(amrtDS.Tables[0].Rows[0]["VA038_AmortizationPeriod"]));
                                    }
                                    if (Util.GetValueOfString(amrtDS.Tables[0].Rows[0]["VA038_PeriodType"]) == "Y")
                                    {
                                        AmortEndDate = AmortStartDate.Value.AddYears(Util.GetValueOfInt(amrtDS.Tables[0].Rows[0]["VA038_AmortizationPeriod"]));
                                    }
                                    line.Set_Value("FROMDATE", AmortStartDate);
                                    line.Set_Value("EndDate", AmortEndDate);
                                    if (amrtDS != null)
                                    {
                                        amrtDS.Dispose();
                                    }
                                }
                            }
                        }
                        // End Change Amortization process--------------
                        if (!line.Save())
                        {
                            //return GetReterivedError(line, "Cannot save Invoice Line");
                            //SI_0708 - message was not upto the mark
                            ValueNamePair pp = VAdvantage.Logging.VLogger.RetrieveError();
                            if (pp != null)
                                throw new ArgumentException("Cannot save Invoice Line. " + pp.GetName());
                            throw new ArgumentException("Cannot save Invoice Line");
                        }
                        count++;
                    }
                    ds.Dispose();
                }
                else
                {
                    MInvoiceLine line = new MInvoiceLine(invoice);
                    line.SetShipLine(sLine);
                    line.SetQtyEntered(sLine.GetQtyEntered());
                    line.SetQtyInvoiced(sLine.GetMovementQty());
                    line.Set_ValueNoCheck("IsDropShip", sLine.Get_Value("IsDropShip")); //Arpit Rai 20-Sept-2017
                    // Change By Mohit Amortization process -------------
                    if (_CountVA038 > 0)
                    {
                        if (line.GetM_Product_ID() > 0)
                        {
                            MProduct pro = new MProduct(GetCtx(), sLine.GetM_Product_ID(), Get_TrxName());
                            if (Util.GetValueOfInt(pro.Get_Value("VA038_AmortizationTemplate_ID")) > 0)
                            {
                                line.Set_Value("VA038_AmortizationTemplate_ID", Util.GetValueOfInt(pro.Get_Value("VA038_AmortizationTemplate_ID")));
                                DataSet amrtDS = DB.ExecuteDataset("SELECT VA038_AmortizationType,VA038_AmortizationPeriod,VA038_TermSource,VA038_PeriodType,Name FROM VA038_AmortizationTemplate WHERE IsActive='Y' AND VA038_AMORTIZATIONTEMPLATE_ID=" + Util.GetValueOfInt(pro.Get_Value("VA038_AmortizationTemplate_ID")));
                                AmortStartDate = null;
                                AmortEndDate = null;
                                if (Util.GetValueOfString(amrtDS.Tables[0].Rows[0]["VA038_TermSource"]) == "A")
                                {
                                    AmortStartDate = invoice.GetDateAcct();
                                }
                                if (Util.GetValueOfString(amrtDS.Tables[0].Rows[0]["VA038_TermSource"]) == "T")
                                {
                                    AmortStartDate = invoice.GetDateInvoiced();
                                }

                                if (Util.GetValueOfString(amrtDS.Tables[0].Rows[0]["VA038_PeriodType"]) == "M")
                                {
                                    AmortEndDate = AmortStartDate.Value.AddMonths(Util.GetValueOfInt(amrtDS.Tables[0].Rows[0]["VA038_AmortizationPeriod"]));
                                }
                                if (Util.GetValueOfString(amrtDS.Tables[0].Rows[0]["VA038_PeriodType"]) == "Y")
                                {
                                    AmortEndDate = AmortStartDate.Value.AddYears(Util.GetValueOfInt(amrtDS.Tables[0].Rows[0]["VA038_AmortizationPeriod"]));
                                }
                                line.Set_Value("FROMDATE", AmortStartDate);
                                line.Set_Value("EndDate", AmortEndDate);
                                if (amrtDS != null)
                                {
                                    amrtDS.Dispose();
                                }
                            }
                        }
                        if (line.GetC_Charge_ID() > 0)
                        {
                            MCharge charge = new MCharge(GetCtx(), sLine.GetC_Charge_ID(), Get_TrxName());
                            if (Util.GetValueOfInt(charge.Get_Value("VA038_AmortizationTemplate_ID")) > 0)
                            {
                                line.Set_Value("VA038_AmortizationTemplate_ID", Util.GetValueOfInt(charge.Get_Value("VA038_AmortizationTemplate_ID")));
                                DataSet amrtDS = DB.ExecuteDataset("SELECT VA038_AmortizationType,VA038_AmortizationPeriod,VA038_TermSource,VA038_PeriodType,Name FROM VA038_AmortizationTemplate WHERE IsActive='Y' AND VA038_AMORTIZATIONTEMPLATE_ID=" + Util.GetValueOfInt(charge.Get_Value("VA038_AmortizationTemplate_ID")));
                                AmortStartDate = null;
                                AmortEndDate = null;
                                if (Util.GetValueOfString(amrtDS.Tables[0].Rows[0]["VA038_TermSource"]) == "A")
                                {
                                    AmortStartDate = invoice.GetDateAcct();
                                }
                                if (Util.GetValueOfString(amrtDS.Tables[0].Rows[0]["VA038_TermSource"]) == "T")
                                {
                                    AmortStartDate = invoice.GetDateInvoiced();
                                }

                                if (Util.GetValueOfString(amrtDS.Tables[0].Rows[0]["VA038_PeriodType"]) == "M")
                                {
                                    AmortEndDate = AmortStartDate.Value.AddMonths(Util.GetValueOfInt(amrtDS.Tables[0].Rows[0]["VA038_AmortizationPeriod"]));
                                }
                                if (Util.GetValueOfString(amrtDS.Tables[0].Rows[0]["VA038_PeriodType"]) == "Y")
                                {
                                    AmortEndDate = AmortStartDate.Value.AddYears(Util.GetValueOfInt(amrtDS.Tables[0].Rows[0]["VA038_AmortizationPeriod"]));
                                }
                                line.Set_Value("FROMDATE", AmortStartDate);
                                line.Set_Value("EndDate", AmortEndDate);
                                if (amrtDS != null)
                                {
                                    amrtDS.Dispose();
                                }
                            }
                        }
                    }
                    // End Change Amortization process--------------
                    if (!line.Save())
                    {
                        //return GetReterivedError(line, "Cannot save Invoice Line");
                        //SI_0708 - message was not upto the mark
                        ValueNamePair pp = VAdvantage.Logging.VLogger.RetrieveError();
                        if (pp != null)
                            throw new ArgumentException("Cannot save Invoice Line. " + pp.GetName());
                        throw new ArgumentException("Cannot save Invoice Line");
                    }
                    count++;
                }
            }


            #region[Enhancement for Charges Distribution/Generation by Sukhwinder on 13 December 2017]

            if (_GenerateCharges && count > 0)
            {
                StringBuilder OrderSql = new StringBuilder();
                OrderSql.Append("   SELECT CO.C_ORDER_ID,                    "
                               + "      SUM(ML.QTYENTERED) AS MRLINEQTY,     "
                               + "      SUM(OL.QTYENTERED) AS ORDERQTY,       "
                               + "   SUM(OL.QTYENTERED *OL.PRICEENTERED) AS LINEAMT, "
                                + "      SUM(ML.QTYENTERED*OL.PRICEENTERED) AS MRLINEVALUE "
                               + "  FROM M_INOUTLINE ML                      "
                               + "  INNER JOIN C_ORDERLINE OL                "
                               + "  ON OL.C_ORDERLINE_ID = ML.C_ORDERLINE_ID "
                               + " INNER JOIN C_ORDER CO                     "
                               + " ON CO.C_ORDER_ID     = OL.C_ORDER_ID      "
                               + " WHERE ML.M_INOUT_ID  = " + _M_InOut_ID
                               + " AND (OL.C_CHARGE_ID IS NULL               "
                               + " OR OL.C_CHARGE_ID    = 0)                 "
                               + " GROUP BY CO.C_ORDER_ID                    ");

                DataSet OrderDS = DB.ExecuteDataset(OrderSql.ToString(), null, Get_Trx());

                if (OrderDS != null && OrderDS.Tables[0].Rows.Count > 0)
                {
                    StringBuilder ChargesSql = new StringBuilder();
                    for (int index = 0; index < OrderDS.Tables[0].Rows.Count; index++)
                    {
                        ds = null;
                        ChargesSql.Clear();
                        ChargesSql.Append(" SELECT C_CHARGE_ID,                                             "
                                 + "   C_ORDERLINE_ID,                                                      "
                                 + "   C_ORDER_ID,                                                          "
                                 + "   C_CURRENCY_ID,                                                       "
                                 + "   PRICEENTERED,                                                        "
                                 + "   PRICEACTUAL,                                                         "
                                 + "   LINENETAMT,                                                          "
                                 + "   QTYENTERED,                                                          "
                                 + "   C_UOM_ID,                                                            "
                                 + "   C_Tax_ID,                                                            "
                                 + "   IsDropShip                                                           "
                                 + " FROM C_ORDERLINE                                                       "
                                 + " WHERE C_ORDER_ID IN                                                    "
                                 + "   ( " + Util.GetValueOfInt(OrderDS.Tables[0].Rows[index]["C_ORDER_ID"])
                                 + "   )                                                                    "
                                 + " AND C_CHARGE_ID IS NOT NULL                                            "
                                 + " AND C_CHARGE_ID  > 0                                                   ");


                        ds = DB.ExecuteDataset(ChargesSql.ToString(), null, Get_Trx());

                        if (ds != null && ds.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                            {


                                MInvoiceLine line = new MInvoiceLine(invoice);
                                line.SetQty(1);
                                line.SetQtyEntered(1);
                                line.SetQtyInvoiced(1);
                                line.SetOrderLine(new MOrderLine(GetCtx(), Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_ORDERLINE_ID"]), Get_Trx()));
                                line.SetC_Charge_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_CHARGE_ID"]));
                                line.SetC_UOM_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_UOM_ID"]));
                                line.SetC_Tax_ID(Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_TAX_ID"]));

                                decimal SumOfQty = 0;
                                decimal SumOfvalue = 0;
                                decimal sototval = 0;

                                string sql = "SELECT SUM(OL.QTYENTERED *OL.PRICEENTERED) AS LINEAMT from C_ORDER CO,C_ORDERLINE OL WHERE CO.C_ORDER_ID=OL.C_ORDER_ID  AND (OL.C_CHARGE_ID IS NULL OR OL.C_CHARGE_ID    = 0) and CO.C_ORDER_ID  =" + ship.GetC_Order_ID();
                                sototval = VAdvantage.Utility.Util.GetValueOfDecimal(DB.ExecuteScalar(sql));




                                if (Util.GetValueOfInt(OrderDS.Tables[0].Rows[index]["ORDERQTY"]) == 0)
                                {
                                    SumOfQty = 1;
                                }
                                else
                                {
                                    SumOfQty = Util.GetValueOfInt(OrderDS.Tables[0].Rows[index]["ORDERQTY"]);
                                    SumOfvalue = Util.GetValueOfDecimal(OrderDS.Tables[0].Rows[index]["LINEAMT"]);


                                }
                                //decimal amt = (Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["LINENETAMT"]) / SumOfQty) * Util.GetValueOfInt(OrderDS.Tables[0].Rows[index]["MRLINEQTY"]);
                                decimal amt = (Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["LINENETAMT"]) / sototval) * Util.GetValueOfInt(OrderDS.Tables[0].Rows[index]["MRLINEVALUE"]);

                                line.SetPrice(Decimal.Round(amt, 3));
                                line.SetPriceActual(Decimal.Round(amt, 3));
                                line.SetPriceEntered(Decimal.Round(amt, 3));
                                line.SetLineNetAmt(Decimal.Round(amt, 3));

                                line.Set_ValueNoCheck("IsDropShip", Util.GetValueOfString(ds.Tables[0].Rows[i]["ISDROPSHIP"]));

                                if (_CountVA038 > 0)
                                {
                                    if (line.GetC_Charge_ID() > 0)
                                    {
                                        MCharge charge = new MCharge(GetCtx(), line.GetC_Charge_ID(), Get_TrxName());
                                        if (Util.GetValueOfInt(charge.Get_Value("VA038_AmortizationTemplate_ID")) > 0)
                                        {
                                            line.Set_Value("VA038_AmortizationTemplate_ID", Util.GetValueOfInt(charge.Get_Value("VA038_AmortizationTemplate_ID")));
                                            DataSet amrtDS = DB.ExecuteDataset("SELECT VA038_AmortizationType,VA038_AmortizationPeriod,VA038_TermSource,VA038_PeriodType,Name FROM VA038_AmortizationTemplate WHERE IsActive='Y' AND VA038_AMORTIZATIONTEMPLATE_ID=" + Util.GetValueOfInt(charge.Get_Value("VA038_AmortizationTemplate_ID")));
                                            AmortStartDate = null;
                                            AmortEndDate = null;
                                            if (Util.GetValueOfString(amrtDS.Tables[0].Rows[0]["VA038_TermSource"]) == "A")
                                            {
                                                AmortStartDate = invoice.GetDateAcct();
                                            }
                                            if (Util.GetValueOfString(amrtDS.Tables[0].Rows[0]["VA038_TermSource"]) == "T")
                                            {
                                                AmortStartDate = invoice.GetDateInvoiced();
                                            }

                                            if (Util.GetValueOfString(amrtDS.Tables[0].Rows[0]["VA038_PeriodType"]) == "M")
                                            {
                                                AmortEndDate = AmortStartDate.Value.AddMonths(Util.GetValueOfInt(amrtDS.Tables[0].Rows[0]["VA038_AmortizationPeriod"]));
                                            }
                                            if (Util.GetValueOfString(amrtDS.Tables[0].Rows[0]["VA038_PeriodType"]) == "Y")
                                            {
                                                AmortEndDate = AmortStartDate.Value.AddYears(Util.GetValueOfInt(amrtDS.Tables[0].Rows[0]["VA038_AmortizationPeriod"]));
                                            }
                                            line.Set_Value("FROMDATE", AmortStartDate);
                                            line.Set_Value("EndDate", AmortEndDate);
                                            if (amrtDS != null)
                                            {
                                                amrtDS.Dispose();
                                            }
                                        }
                                    }
                                }

                                if (!line.Save())
                                {
                                    //return GetReterivedError(line, "Cannot save Invoice Line");
                                    //SI_0708 - message was not upto the mark
                                    ValueNamePair pp = VAdvantage.Logging.VLogger.RetrieveError();
                                    if (pp != null)
                                        throw new ArgumentException("Cannot save Invoice Line. " + pp.GetName());
                                    throw new ArgumentException("Cannot save Invoice Line");
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            if (count > 0)
            {
                return invoice.GetDocumentNo();
            }
            else
            {
                Get_Trx().Rollback();
                return Msg.GetMsg(GetCtx(), "InvoiceExist");
            }
        }
    }
}
