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
                FolderName = "Scen_?-rep_?", //we should probably add this to the extension/scenario input file or we might be leaving this out because the extensions do not need to know anything about the replication (the hirarchy of the scenario-replications and their extensions are defined by the convention of folder structures)
                RasterOutCellArea = PlugIn.ModelCore.CellArea,
                TimeMin = PlugIn.ModelCore.StartTime,
                TimeMax = PlugIn.ModelCore.EndTime,
                ProjectionFilePath = "Projection.?" //How do we get projections???
            };

            Extension = new ExtensionMetadata(){
                Name = "Century-Succession",
                TimeInterval = PlugIn.SuccessionTimeStep,  //ModelCore.TimeSinceStart, //TimeInterval?
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
                MetadataFilePath = @"Century-Succession\MonthlyLog.xml"
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
                FilePath = @"Century-Succession\ag_npp-[timeStep].img"
                //MetadataFilePath = @"Century-Succession\ANPP.xml"
            };
            Extension.OutputMetadatas.Add(mapOut_ANPP);

            //---------------------------------------
            MetadataProvider mp = new MetadataProvider(Extension);
            mp.WriteMetadataToXMLFile(("Metadata", Extension.Name, Extension.Name);




        }
    }
}
