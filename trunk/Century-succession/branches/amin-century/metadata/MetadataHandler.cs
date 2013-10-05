using System;
using System.Collections.Generic;
using System.Linq;
//using System.Data;
using System.Text;
using Landis.Library.Metadata;
using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Extension.Succession.Century
{
    public static class MetadataHandler
    {
        
        public static ExtensionMetadata Extension {get; set;}

        public static void InitializeMetadata()
        {
            ScenarioReplicationMetadata scenRep = new ScenarioReplicationMetadata() {
                //String outputFolder = OutputPath.ReplaceTemplateVars("", FINISH ME LATER);
                FolderName = System.IO.Directory.GetCurrentDirectory().Split("\\".ToCharArray()).Last(),//"Scen_?-rep_?", //we should probably add this to the extension/scenario input file or we might be leaving this out because the extensions do not need to know anything about the replication (the hirarchy of the scenario-replications and their extensions are defined by the convention of folder structures)
                RasterOutCellArea = PlugIn.ModelCore.CellArea,
                TimeMin = PlugIn.ModelCore.StartTime,
                TimeMax = PlugIn.ModelCore.EndTime,
                ProjectionFilePath = @"Metadata\projection.xml" //How do we get projections???
            };

            Extension = new ExtensionMetadata(){
                Name = "Century-Succession",
                TimeInterval = 5, //I hard coded this bcause PlugIn.SuccessionTimeStep returns 0 Rob please take a look at this.    PlugIn.SuccessionTimeStep, //change this to PlugIn.TimeStep for other extensions
                ScenarioReplicationMetadata = scenRep
            };

            //---------------------------------------
            //          table outputs:   
            //---------------------------------------

            OutputMetadata tblOut_monthly = new OutputMetadata()
            {
                Type = OutputType.Table,
                Name = "MonthlyLog",
                FilePath = Outputs.dtMonthly.FilePath,
                //MetadataFilePath = @"Century-Succession\MonthlyLog.xml" //this is set automatically and it's not required to be set
            };
            tblOut_monthly.RetriveFields(typeof(MonthlyLog));
            Extension.OutputMetadatas.Add(tblOut_monthly);


            //---------------------------------------            
            //          map outputs:         
            //---------------------------------------

            OutputMetadata mapOut_ANPP = new OutputMetadata()
            {
                Type = OutputType.Map,
                Name = "ag_npp",
                FilePath = @"century\ag_npp-{timestep}.gis",  //century
                Map_DataType = MapDataType.Quantitative,
                Map_Unit = FiledUnits.g_C_m_2,
                //MetadataFilePath = @"Century-Succession\ANPP.xml"
            };
            Extension.OutputMetadatas.Add(mapOut_ANPP);


            OutputMetadata mapOut_Nee = new OutputMetadata()
            {
                Type = OutputType.Map,
                Name = "nee",
                FilePath = @"century\nee-{timestep}.gis",  //century
                Map_DataType = MapDataType.Quantitative,
                Map_Unit = FiledUnits.g_C_m_2,
                //MetadataFilePath = @"Century-Succession\ANPP.xml"
            };
            Extension.OutputMetadatas.Add(mapOut_Nee);

            //---------------------------------------
            MetadataProvider mp = new MetadataProvider(Extension);
            mp.WriteMetadataToXMLFile("Metadata", Extension.Name, Extension.Name);




        }
    }
}
