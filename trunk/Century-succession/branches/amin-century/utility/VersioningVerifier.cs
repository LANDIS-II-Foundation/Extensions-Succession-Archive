/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;
using System.Security.Policy;
using System.Diagnostics;  //for Evidence object 
//using System.Runtime.CompilerServices;

namespace Landis.Core
{
    



    class VersioningHelper
    {
        public static void Verify(string[] assembly_Names_Versions) 
	    {
		    AppDomain currentDomain = AppDomain.CurrentDomain;
		    //Provide the current application domain evidence for the assembly.
		    //Evidence asEvidence = currentDomain.Evidence;
		    //Load the assembly from the application directory using a simple name. 

		    //Create an assembly called CustomLibrary to run this sample.
		    //currentDomain.Load("customLibrary", asEvidence);

		    //Make an array for the list of assemblies.
           
            Assembly[] assems = currentDomain.GetAssemblies();
            
	
		    //List the assemblies in the current application domain.
		    //Console.WriteLine("List of assemblies loaded in current appdomain:");

            //Landis.Core

            foreach (string nv in assembly_Names_Versions)
            {
                foreach (Assembly assembly in assems)
                { 
                    //assembly.GetName().Version.CompareTo(new Version(
                    //string[] nvArr = nv.Split(";".ToCharArray());
                    //if (assembly.FullName.ToLower().Contains(nvArr[0].Trim().ToLower()))
                    //{
                    //    //FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                    //    //Console.WriteLine(assembly.FullName + "_FileVersion: " + fvi.FileVersion);
                    //    ////int startIndex = assembly.FullName.IndexOf("Version=") + 8;
                    //    ////int endIndex = assembly.FullName.IndexOf(", Culture");
                    //    ////string version = assembly.FullName.Substring(startIndex, endIndex - startIndex);
                    //    if (nvArr[1] != version)
                    //    Console.WriteLine("");
                    //}

                }
            }
	    }

        //public static void VerifyReferences(dynamic[] assembly_Names_Versions)
        //{
        //    Assembly execAssembly = System.Reflection.Assembly.GetExecutingAssembly();
        //    AssemblyName[] refAssemNames = execAssembly.GetReferencedAssemblies();
        //    List<string> unmaches = new List<string>();

        //    foreach (dynamic nv in assembly_Names_Versions)
        //    {
        //        foreach (AssemblyName assemblyName in refAssemNames)
        //        {
        //            if (assemblyName.Name.ToLower() == nv.Name.ToLower())
        //            { 
        //                if(assemblyName.Version.CompareTo(new Version(nv.Major, nv.Minor, nv.Build, nv.Revision)) != 0)
        //                    unmaches.Add(assemblyName.Name + " " + assemblyName.Version.ToString() + "does not maches " + nv.Name + " " + nv.Major+"."+nv.Minor+"."+nv.Build+"."+nv.Revision;
        //            }
        //        }
        //    }
        //}


        public static void VerifyReferences(string[] assembly_Names_Versions)
        {
            Assembly execAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            AssemblyName[] refAssemNames = execAssembly.GetReferencedAssemblies();
            List<string> unmaches = new List<string>();

            foreach (string nv in assembly_Names_Versions)
            {
                string[] nvArr = nv.Split(";".ToCharArray());
                foreach (AssemblyName assemblyName in refAssemNames)
                {
                    if (assemblyName.Name.ToLower() == nvArr[0].ToLower())
                    { 
                        if(assemblyName.Version.CompareTo(new Version(nv.Major, nv.Minor, nv.Build, nv.Revision)) != 0)
                            unmaches.Add(assemblyName.Name + " " + assemblyName.Version.ToString() + "does not maches " + nv.Name + " " + nv.Major+"."+nv.Minor+"."+nv.Build+"."+nv.Revision;
                    }
                }
            }
        }



    }

}

*/