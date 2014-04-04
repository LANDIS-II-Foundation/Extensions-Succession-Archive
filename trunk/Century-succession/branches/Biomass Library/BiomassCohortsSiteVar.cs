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

using BiomassCohorts = Landis.Library.BiomassCohorts;
using LeafBiomassCohorts = Landis.Library.LeafBiomassCohorts;
using Landis.SpatialModeling;

namespace Landis.Extension.Succession.Century
{
    /// <summary>
    /// Wraps a biomass-cohorts site variable and provides access to it as a
    /// site variable of base cohorts.
    /// </summary>
    public class BiomassCohortsSiteVar
        : ISiteVar<BiomassCohorts.ISiteCohorts>
    {
        private ISiteVar<LeafBiomassCohorts.ISiteCohorts> leafBiomassCohortSiteVar;

        public BiomassCohortsSiteVar(ISiteVar<LeafBiomassCohorts.ISiteCohorts> siteVar)
        {
            leafBiomassCohortSiteVar = siteVar;
        }

        #region ISiteVariable members
        System.Type ISiteVariable.DataType
        {
            get
            {
                return typeof(BiomassCohorts.ISiteCohorts);
            }
        }

        InactiveSiteMode ISiteVariable.Mode
        {
            get
            {
                return leafBiomassCohortSiteVar.Mode;
            }
        }

        ILandscape ISiteVariable.Landscape
        {
            get
            {
                return leafBiomassCohortSiteVar.Landscape;
            }
        }
        #endregion

        #region ISiteVar<BaseCohorts.ISiteCohorts> members
        // Extensions other than succession have no need to assign the whole
        // site-cohorts object at any site.

        BiomassCohorts.ISiteCohorts ISiteVar<BiomassCohorts.ISiteCohorts>.this[Site site]
        {
            get
            {
                return (BiomassCohorts.ISiteCohorts) leafBiomassCohortSiteVar[site];
                //return biomassCohortSiteVar[site]; 
            }
            set
            {
                throw new System.InvalidOperationException("Operation restricted to succession extension");
            }
        }

        BiomassCohorts.ISiteCohorts ISiteVar<BiomassCohorts.ISiteCohorts>.ActiveSiteValues
        {
            set
            {
                throw new System.InvalidOperationException("Operation restricted to succession extension");
            }
        }

        BiomassCohorts.ISiteCohorts ISiteVar<BiomassCohorts.ISiteCohorts>.InactiveSiteValues
        {
            set
            {
                throw new System.InvalidOperationException("Operation restricted to succession extension");
            }
        }

        BiomassCohorts.ISiteCohorts ISiteVar<BiomassCohorts.ISiteCohorts>.SiteValues
        {
            set
            {
                throw new System.InvalidOperationException("Operation restricted to succession extension");
            }
        }
        #endregion
    }
}
