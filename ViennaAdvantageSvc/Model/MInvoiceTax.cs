﻿/********************************************************
 * Project Name   : VAdvantage
 * Class Name     : MInvoiceTax
 * Purpose        : Invoice tex calculation
 * Class Used     : X_C_InvoiceTax
 * Chronological    Development
 * Raghunandan     22-Jun-2009
  ******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using VAdvantage.Classes;
using VAdvantage.Utility;
using VAdvantage.DataBase;
using System.Windows.Forms;
using VAdvantage.Logging;
using VAdvantage.Model;

namespace ViennaAdvantage.Model
{
   public class MInvoiceTax : X_C_InvoiceTax
    {
        /**
	     * 	Get Tax Line for Invoice Line
	     *	@param line invoice line
	     *	@param precision currency precision
	     *	@param oldTax if true old tax is returned
	     *	@param trxName transaction name
	     *	@return existing or new tax
	     */
        public static MInvoiceTax Get(MInvoiceLine line, int precision,
            Boolean oldTax, Trx trxName)
        {
            MInvoiceTax retValue = null;
            try
            {
                if (line == null || line.GetC_Invoice_ID() == 0 || line.IsDescription())
                    return null;
                int C_Tax_ID = line.GetC_Tax_ID();
                if (oldTax && line.Is_ValueChanged("C_Tax_ID"))
                {
                    Object old = line.Get_ValueOld("C_Tax_ID");
                    if (old == null)
                        return null;
                    C_Tax_ID = int.Parse(old.ToString());
                }
                if (C_Tax_ID == 0)
                {
                    _log.Warning("C_Tax_ID=0");
                    return null;
                }

                String sql = "SELECT * FROM C_InvoiceTax WHERE C_Invoice_ID=" + line.GetC_Invoice_ID() + " AND C_Tax_ID=" + C_Tax_ID;
                try
                {
                    DataSet ds = DB.ExecuteDataset(sql, null, trxName);
                    if (ds.Tables.Count > 0)
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            retValue = new MInvoiceTax(line.GetCtx(), dr, trxName);
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.Log(Level.SEVERE, sql, e); 
                }

                if (retValue != null)
                {
                    retValue.Set_TrxName(trxName);
                    retValue.SetPrecision(precision);
                    _log.Fine("(old=" + oldTax + ") " + retValue);
                    return retValue;
                }

                //	Create New
                retValue = new MInvoiceTax(line.GetCtx(), 0, trxName);
                retValue.Set_TrxName(trxName);
                retValue.SetClientOrg(line);
                retValue.SetC_Invoice_ID(line.GetC_Invoice_ID());
                retValue.SetC_Tax_ID(line.GetC_Tax_ID());
                retValue.SetPrecision(precision);
                retValue.SetIsTaxIncluded(line.IsTaxIncluded());
                _log.Fine("(new) " + retValue);
            }
            catch 
            {
               // MessageBox.Show("MInvoiceTax--Get");
            }
            return retValue;
        }

        //	Static Logger	
        private static VLogger _log = VLogger.GetVLogger(typeof(MInvoiceTax).FullName);


        /**************************************************************************
         * 	Persistency Constructor
         *	@param ctx context
         *	@param ignored ignored
         *	@param trxName transaction
         */
        public MInvoiceTax(Ctx ctx, int ignored, Trx trxName)
            : base(ctx, 0, trxName)
        {
            if (ignored != 0)
                throw new ArgumentException("Multi-Key");
            SetTaxAmt(Env.ZERO);
            SetTaxBaseAmt(Env.ZERO);
            SetIsTaxIncluded(false);
        }

        /**
         * 	Load Constructor.
         * 	Set Precision and TaxIncluded for tax calculations!
         *	@param ctx context
         *	@param dr result set
         *	@param trxName transaction
         */
        public MInvoiceTax(Ctx ctx, DataRow dr, Trx trxName)
            : base(ctx, dr, trxName)
        {
        }

        /** Tax							*/
        private MTax _tax = null;
        /** Cached Precision			*/
        private int _precision = -1;


        /**
         * 	Get Precision
         * 	@return Returns the precision or 2
         */
        private int GetPrecision()
        {
            if (_precision == -1)
                return 2;
            return _precision;
        }

        /**
         * 	Set Precision
         *	@param precision The precision to set.
         */
        public void SetPrecision(int precision)
        {
            _precision = precision;
        }

        /**
         * 	Get Tax
         *	@return tax
         */
        public MTax GetTax()
        {
            if (_tax == null)
                _tax = MTax.Get(GetCtx(), GetC_Tax_ID());
            return _tax;
        }


        /**************************************************************************
         * 	Calculate/Set Tax Base Amt from Invoice Lines
         * 	@return true if tax calculated
         */
        public bool CalculateTaxFromLines()
        {
            Decimal? taxBaseAmt = Env.ZERO;
            Decimal taxAmt = Env.ZERO;
            //
            bool documentLevel = GetTax().IsDocumentLevel();
            MTax tax = GetTax();
            //
            String sql = "SELECT il.LineNetAmt, COALESCE(il.TaxAmt,0), i.IsSOTrx , i.C_Currency_ID , i.DateAcct , i.C_ConversionType_ID "
                + "FROM C_InvoiceLine il"
                + " INNER JOIN C_Invoice i ON (il.C_Invoice_ID=i.C_Invoice_ID) "
                + "WHERE il.C_Invoice_ID=" + GetC_Invoice_ID() + " AND il.C_Tax_ID=" + GetC_Tax_ID();
            IDataReader idr = null;
            int c_Currency_ID = 0;
            DateTime? dateAcct = null;
            int c_ConversionType_ID = 0;
            try
            {
                idr = DB.ExecuteReader(sql, null, Get_TrxName());
                while (idr.Read())
                {
                    //Get References from invoiice header
                    c_Currency_ID = Util.GetValueOfInt(idr[3]);
                    dateAcct = Util.GetValueOfDateTime(idr[4]);
                    c_ConversionType_ID = Util.GetValueOfInt(idr[5]);
                    //	BaseAmt
                    Decimal baseAmt = Util.GetValueOfDecimal(idr[0]);
                    taxBaseAmt = Decimal.Add((Decimal)taxBaseAmt, baseAmt);
                    //	TaxAmt
                    Decimal amt = Util.GetValueOfDecimal(idr[1]);
                    //if (amt == null)
                    //    amt = Env.ZERO;
                    bool isSOTrx = "Y".Equals(idr[2].ToString());
                    //
                    if (documentLevel || Env.Signum(baseAmt) == 0)
                    { amt = Env.ZERO; 
                    }
                    else if (Env.Signum(amt) != 0 && !isSOTrx)	//	manually entered
                    { ;
                    }
                    else	// calculate line tax
                    {
                        amt = tax.CalculateTax(baseAmt, IsTaxIncluded(), GetPrecision());
                    }
                    //
                    taxAmt = Decimal.Add(taxAmt, amt);
                }
                idr.Close();
            }
            catch (Exception e)
            {
                if (idr != null)
                {
                    idr.Close();
                }
                log.Log(Level.SEVERE, "setTaxBaseAmt", e);
                taxBaseAmt = null;
            }
            if (taxBaseAmt == null)
                return false;

            //	Calculate Tax
            if (documentLevel || Env.Signum(taxAmt) == 0)
                taxAmt = tax.CalculateTax((Decimal)taxBaseAmt, IsTaxIncluded(), GetPrecision());
            SetTaxAmt(taxAmt);

            // set Tax Amount in base currency 
            if (Get_ColumnIndex("TaxBaseCurrencyAmt") >= 0)
            {
                decimal taxAmtBaseCurrency = GetTaxAmt();
                int primaryAcctSchemaCurrency = Util.GetValueOfInt(DB.ExecuteScalar(@"SELECT C_Currency_ID FROM C_AcctSchema WHERE C_AcctSchema_ID = 
                                            (SELECT c_acctschema1_id FROM ad_clientinfo WHERE ad_client_id = " + GetAD_Client_ID() + ")", null, Get_Trx()));
                if (c_Currency_ID != primaryAcctSchemaCurrency)
                {
                    taxAmtBaseCurrency = MConversionRate.Convert(GetCtx(), GetTaxAmt(), primaryAcctSchemaCurrency, c_Currency_ID,
                                                                               dateAcct, c_ConversionType_ID, GetAD_Client_ID(), GetAD_Org_ID());
                }
                SetTaxBaseCurrencyAmt(taxAmtBaseCurrency);
            }

            //	Set Base
            if (IsTaxIncluded())
                SetTaxBaseAmt(Decimal.Subtract((Decimal)taxBaseAmt, taxAmt));
            else
                SetTaxBaseAmt((Decimal)taxBaseAmt);
            return true;
        }

        /**
         * 	String Representation
         *	@return info
         */
        public override String ToString()
        {
            StringBuilder sb = new StringBuilder("MInvoiceTax[");
            sb.Append("C_Invoice_ID=").Append(GetC_Invoice_ID())
                .Append(",C_Tax_ID=").Append(GetC_Tax_ID())
                .Append(", Base=").Append(GetTaxBaseAmt()).Append(",Tax=").Append(GetTaxAmt())
                .Append("]");
            return sb.ToString();
        }
    }
}