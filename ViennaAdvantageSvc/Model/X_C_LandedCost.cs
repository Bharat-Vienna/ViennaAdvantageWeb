namespace ViennaAdvantage.Model
{

/** Generated Model - DO NOT CHANGE */
using System;
using System.Text;
using VAdvantage.DataBase;
using VAdvantage.Common;
using VAdvantage.Classes;
using VAdvantage.Process;
using VAdvantage.Model;
using VAdvantage.Utility;
using System.Data;
/** Generated Model for C_LandedCost
 *  @author Jagmohan Bhatt (generated) 
 *  @version Vienna Framework 1.1.1 - $Id$ */
public class X_C_LandedCost : PO
{
public X_C_LandedCost (Context ctx, int C_LandedCost_ID, Trx trxName) : base (ctx, C_LandedCost_ID, trxName)
{
/** if (C_LandedCost_ID == 0)
{
SetC_InvoiceLine_ID (0);
SetC_LandedCost_ID (0);
SetLandedCostDistribution (null);	// Q
SetM_CostElement_ID (0);
}
 */
}
public X_C_LandedCost (Ctx ctx, int C_LandedCost_ID, Trx trxName) : base (ctx, C_LandedCost_ID, trxName)
{
/** if (C_LandedCost_ID == 0)
{
SetC_InvoiceLine_ID (0);
SetC_LandedCost_ID (0);
SetLandedCostDistribution (null);	// Q
SetM_CostElement_ID (0);
}
 */
}
/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
public X_C_LandedCost (Context ctx, DataRow rs, Trx trxName) : base(ctx, rs, trxName)
{
}
/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
public X_C_LandedCost (Ctx ctx, DataRow rs, Trx trxName) : base(ctx, rs, trxName)
{
}
/** Load Constructor 
@param ctx context
@param rs result set 
@param trxName transaction
*/
public X_C_LandedCost (Ctx ctx, IDataReader dr, Trx trxName) : base(ctx, dr, trxName)
{
}
/** Static Constructor 
 Set Table ID By Table Name
 added by ->Harwinder */
static X_C_LandedCost()
{
 Table_ID = Get_Table_ID(Table_Name);
 model = new KeyNamePair(Table_ID,Table_Name);
}
/** Serial Version No */
//static long serialVersionUID 27562514372924L;
/** Last Updated Timestamp 7/29/2010 1:07:36 PM */
public static long updatedMS = 1280389056135L;
/** AD_Table_ID=759 */
public static int Table_ID;
 // =759;

/** TableName=C_LandedCost */
public static String Table_Name="C_LandedCost";

protected static KeyNamePair model;
protected Decimal accessLevel = new Decimal(1);
/** AccessLevel
@return 1 - Org 
*/
protected override int Get_AccessLevel()
{
return Convert.ToInt32(accessLevel.ToString());
}
/** Load Meta Data
@param ctx context
@return PO Info
*/
protected override POInfo InitPO (Ctx ctx)
{
POInfo poi = POInfo.GetPOInfo (ctx, Table_ID);
return poi;
}
/** Load Meta Data
@param ctx context
@return PO Info
*/
protected override POInfo InitPO(Context ctx)
{
POInfo poi = POInfo.GetPOInfo (ctx, Table_ID);
return poi;
}
/** Info
@return info
*/
public override String ToString()
{
StringBuilder sb = new StringBuilder ("X_C_LandedCost[").Append(Get_ID()).Append("]");
return sb.ToString();
}
/** Set Invoice Line.
@param C_InvoiceLine_ID Invoice Detail Line */
public void SetC_InvoiceLine_ID (int C_InvoiceLine_ID)
{
if (C_InvoiceLine_ID < 1) throw new ArgumentException ("C_InvoiceLine_ID is mandatory.");
Set_ValueNoCheck ("C_InvoiceLine_ID", C_InvoiceLine_ID);
}
/** Get Invoice Line.
@return Invoice Detail Line */
public int GetC_InvoiceLine_ID() 
{
Object ii = Get_Value("C_InvoiceLine_ID");
if (ii == null) return 0;
return Convert.ToInt32(ii);
}
/** Get Record ID/ColumnName
@return ID/ColumnName pair */
public KeyNamePair GetKeyNamePair() 
{
return new KeyNamePair(Get_ID(), GetC_InvoiceLine_ID().ToString());
}
/** Set Landed Cost.
@param C_LandedCost_ID Landed cost to be allocated to material receipts */
public void SetC_LandedCost_ID (int C_LandedCost_ID)
{
if (C_LandedCost_ID < 1) throw new ArgumentException ("C_LandedCost_ID is mandatory.");
Set_ValueNoCheck ("C_LandedCost_ID", C_LandedCost_ID);
}
/** Get Landed Cost.
@return Landed cost to be allocated to material receipts */
public int GetC_LandedCost_ID() 
{
Object ii = Get_Value("C_LandedCost_ID");
if (ii == null) return 0;
return Convert.ToInt32(ii);
}
/** Set Description.
@param Description Optional short description of the record */
public void SetDescription (String Description)
{
if (Description != null && Description.Length > 255)
{
log.Warning("Length > 255 - truncated");
Description = Description.Substring(0,255);
}
Set_Value ("Description", Description);
}
/** Get Description.
@return Optional short description of the record */
public String GetDescription() 
{
return (String)Get_Value("Description");
}

/** LandedCostDistribution AD_Reference_ID=339 */
public static int LANDEDCOSTDISTRIBUTION_AD_Reference_ID=339;
/** Costs = C */
public static String LANDEDCOSTDISTRIBUTION_Costs = "C";
/** Line = L */
public static String LANDEDCOSTDISTRIBUTION_Line = "L";
/** Quantity = Q */
public static String LANDEDCOSTDISTRIBUTION_Quantity = "Q";
/** Volume = V */
public static String LANDEDCOSTDISTRIBUTION_Volume = "V";
/** Weight = W */
public static String LANDEDCOSTDISTRIBUTION_Weight = "W";
/** Is test a valid value.
@param test testvalue
@returns true if valid **/
public bool IsLandedCostDistributionValid (String test)
{
return test.Equals("C") || test.Equals("L") || test.Equals("Q") || test.Equals("V") || test.Equals("W");
}
/** Set Cost Distribution.
@param LandedCostDistribution Landed Cost Distribution */
public void SetLandedCostDistribution (String LandedCostDistribution)
{
if (LandedCostDistribution == null) throw new ArgumentException ("LandedCostDistribution is mandatory");
if (!IsLandedCostDistributionValid(LandedCostDistribution))
throw new ArgumentException ("LandedCostDistribution Invalid value - " + LandedCostDistribution + " - Reference_ID=339 - C - L - Q - V - W");
if (LandedCostDistribution.Length > 1)
{
log.Warning("Length > 1 - truncated");
LandedCostDistribution = LandedCostDistribution.Substring(0,1);
}
Set_Value ("LandedCostDistribution", LandedCostDistribution);
}
/** Get Cost Distribution.
@return Landed Cost Distribution */
public String GetLandedCostDistribution() 
{
return (String)Get_Value("LandedCostDistribution");
}
/** Set Cost Element.
@param M_CostElement_ID Product Cost Element */
public void SetM_CostElement_ID (int M_CostElement_ID)
{
if (M_CostElement_ID < 1) throw new ArgumentException ("M_CostElement_ID is mandatory.");
Set_Value ("M_CostElement_ID", M_CostElement_ID);
}
/** Get Cost Element.
@return Product Cost Element */
public int GetM_CostElement_ID() 
{
Object ii = Get_Value("M_CostElement_ID");
if (ii == null) return 0;
return Convert.ToInt32(ii);
}
/** Set Shipment/Receipt Line.
@param M_InOutLine_ID Line on Shipment or Receipt document */
public void SetM_InOutLine_ID (int M_InOutLine_ID)
{
if (M_InOutLine_ID <= 0) Set_Value ("M_InOutLine_ID", null);
else
Set_Value ("M_InOutLine_ID", M_InOutLine_ID);
}
/** Get Shipment/Receipt Line.
@return Line on Shipment or Receipt document */
public int GetM_InOutLine_ID() 
{
Object ii = Get_Value("M_InOutLine_ID");
if (ii == null) return 0;
return Convert.ToInt32(ii);
}
/** Set Shipment/Receipt.
@param M_InOut_ID Material Shipment Document */
public void SetM_InOut_ID (int M_InOut_ID)
{
if (M_InOut_ID <= 0) Set_Value ("M_InOut_ID", null);
else
Set_Value ("M_InOut_ID", M_InOut_ID);
}
/** Get Shipment/Receipt.
@return Material Shipment Document */
public int GetM_InOut_ID() 
{
Object ii = Get_Value("M_InOut_ID");
if (ii == null) return 0;
return Convert.ToInt32(ii);
}
/** Set Product.
@param M_Product_ID Product, Service, Item */
public void SetM_Product_ID (int M_Product_ID)
{
if (M_Product_ID <= 0) Set_Value ("M_Product_ID", null);
else
Set_Value ("M_Product_ID", M_Product_ID);
}
/** Get Product.
@return Product, Service, Item */
public int GetM_Product_ID() 
{
Object ii = Get_Value("M_Product_ID");
if (ii == null) return 0;
return Convert.ToInt32(ii);
}
/** Set Process Now.
@param Processing Process Now */
public void SetProcessing (Boolean Processing)
{
Set_Value ("Processing", Processing);
}
/** Get Process Now.
@return Process Now */
public Boolean IsProcessing() 
{
Object oo = Get_Value("Processing");
if (oo != null) 
{
 if (oo.GetType() == typeof(bool)) return Convert.ToBoolean(oo);
 return "Y".Equals(oo);
}
return false;
}
}

}
