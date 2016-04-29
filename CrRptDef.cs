using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

namespace MemberCommunications.Web.Controllers
{
    class CrRptDef
    {
        public string filepath;
        public string name;
        public string description;
        public List<CrParamDef> parameters;
        public List<CrFormulaDef> forumulafields;

        public CrRptDef(string filename)
        {
            filepath = filename;

            //load the document
            ReportDocument cryRpt = new ReportDocument();
            cryRpt.Load(filename);

            //set variables
            this.forumulafields = new List<CrFormulaDef>();
            this.parameters = new List<CrParamDef>();
            this.name = cryRpt.Name;
            this.description = cryRpt.FileName;
            foreach (FormulaFieldDefinition f in cryRpt.DataDefinition.FormulaFields)
            {
                try
                {

                    CrFormulaDef iformula = new CrFormulaDef();
                    iformula.name = f.Name;
                    iformula.formula_name = f.FormulaName;
                    iformula.code = f.Text;
                    iformula.use_formula = false;
                    this.forumulafields.Add(iformula);
                }
                catch (Exception)
                {
                    //this.forumulafields.Add("error");
                }

            }

            foreach (ParameterFieldDefinition p in cryRpt.DataDefinition.ParameterFields)
            {
                if (!(p.ParameterFieldUsage2.ToString().Contains(ParameterFieldUsage2.NotInUse.ToString())) && !(p.IsLinked()))
                {
                    try
                    {
                        CrParamDef cP = new CrParamDef();
                        cP.name = p.Name;
                        cP.type = p.ParameterValueKind.ToString();
                        cP.default_value = cP.default_value;
                        cP.value = "";

                        parameters.Add(cP);
                    }
                    catch (Exception)
                    {
                        //this.forumulafields.Add("error");
                    }
                }
            }
        }
    }

    public class CrParamDef
    {
        public string name;
        public string type;
        public string value;
        public string default_value;

    }

    public class CrFormulaDef
    {
        public string name;
        public string formula_name;
        public string code;
        public bool use_formula;
    }

}
