// Copyright 2010 Green Code LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

//using LeafBiomassCohorts = Landis.Library.LeafBiomassCohorts;
//using BiomassCohorts = Landis.Library.BiomassCohorts;
using Landis.Library;
using Landis.SpatialModeling;

namespace Landis.Extension.Succession.Century
{
    /// <summary>
    /// Wraps a biomass-cohorts site variable and provides access to it as a
    /// site variable of base cohorts.
    /// </summary>
    public class BiomassCohortsSiteVar
        : ISiteVar<Landis.Library.BiomassCohorts.ISiteCohorts>
    {
        private ISiteVar<Landis.Library.LeafBiomassCohorts.ISiteCohorts> LeafCohortSiteVar;

        public BiomassCohortsSiteVar(ISiteVar<Landis.Library.LeafBiomassCohorts.ISiteCohorts> siteVar)
        {
            LeafCohortSiteVar = siteVar;
        }

        #region ISiteVariable members
        System.Type ISiteVariable.DataType
        {
            get
            {
                return typeof(Landis.Library.BiomassCohorts.ISiteCohorts);
            }
        }

        InactiveSiteMode ISiteVariable.Mode
        {
            get
            {
                return LeafCohortSiteVar.Mode;
            }
        }

        ILandscape ISiteVariable.Landscape
        {
            get
            {
                return LeafCohortSiteVar.Landscape;
            }
        }
        #endregion

        #region ISiteVar<BaseCohorts.ISiteCohorts> members
        // Extensions other than succession have no need to assign the whole
        // site-cohorts object at any site.

        Landis.Library.BiomassCohorts.ISiteCohorts ISiteVar<Landis.Library.BiomassCohorts.ISiteCohorts>.this[Site site]
        {
            get
            {
                Landis.Library.LeafBiomassCohorts.SiteCohorts test = (Landis.Library.LeafBiomassCohorts.SiteCohorts)LeafCohortSiteVar[site];
                Landis.Library.BiomassCohorts.ISiteCohorts test2 = (Landis.Library.BiomassCohorts.ISiteCohorts)test;
                //Landis.Library.BiomassCohorts.SiteCohorts newSiteCohorts = new Landis.Library.BiomassCohorts.SiteCohorts();
                //foreach (Landis.Library.LeafBiomassCohorts.ISpeciesCohorts speciesCohorts in test2)
                //{
                //    Landis.Library.BiomassCohorts.ISpeciesCohorts test3 = (Landis.Library.BiomassCohorts.ISpeciesCohorts)speciesCohorts;
                    
                //}
               
                return  test2;
                //return biomassCohortSiteVar[site]; 
            }
            set
            {
                throw new System.InvalidOperationException("Operation restricted to succession extension");
            }
        }

        Landis.Library.BiomassCohorts.ISiteCohorts ISiteVar<Landis.Library.BiomassCohorts.ISiteCohorts>.ActiveSiteValues
        {
            set
            {
                throw new System.InvalidOperationException("Operation restricted to succession extension");
            }
        }

        Landis.Library.BiomassCohorts.ISiteCohorts ISiteVar<Landis.Library.BiomassCohorts.ISiteCohorts>.InactiveSiteValues
        {
            set
            {
                throw new System.InvalidOperationException("Operation restricted to succession extension");
            }
        }

        Landis.Library.BiomassCohorts.ISiteCohorts ISiteVar<Landis.Library.BiomassCohorts.ISiteCohorts>.SiteValues
        {
            set
            {
                throw new System.InvalidOperationException("Operation restricted to succession extension");
            }
        }
        #endregion
    }
}
