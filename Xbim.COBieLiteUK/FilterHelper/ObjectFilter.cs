﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xbim.COBieLiteUK.FilterHelper
{
    /// <summary>
    /// Filter on object type names, used to filter Type and Component COBie Sheets
    /// </summary>
    public class ObjectFilter
    {

        

        #region Properties
        /// <summary>
        /// Keyed list with true or false values, true to include. false to exclude
        /// </summary>
        public SerializableDictionary<string, bool> Items { get; set; }

        /// <summary>
        /// keyed by IfcElement to element property PredefinedType
        /// </summary>
        public SerializableDictionary<string, string[]> PreDefinedType { get; set; }

        /// <summary>
        /// Items to filter out
        /// </summary>
        private List<string> _itemsToExclude = null;
        private List<string> ItemsToExclude
        {
            get
            {
                return _itemsToExclude != null ? _itemsToExclude : Items.Where(e => e.Value == false).Select(e => e.Key).ToList();
            }
        }
        #endregion

        public ObjectFilter()
        {
            Items = new SerializableDictionary<string, bool>();
            PreDefinedType = new SerializableDictionary<string, string[]>();
        }

        /// <summary>
        /// Set Property Filters constructor via ConfigurationSection from configuration file
        /// </summary>
        /// <param name="section">ConfigurationSection from configuration file</param>
        public ObjectFilter(ConfigurationSection section) : this()
        {
            if (section != null)
            {
                foreach (KeyValueConfigurationElement keyVal in ((AppSettingsSection)section).Settings)
                {
                    if (!string.IsNullOrEmpty(keyVal.Key))
                    {
                        bool include = false;
                        if (String.Compare(keyVal.Value, "YES", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            include = true;
                        }
                        Items.Add(keyVal.Key.ToUpper(), include);
                    }
                }
            }
        }

        /// <summary>
        /// add PreDefined types associated with ifcElements
        /// </summary>
        /// <param name="ifcElement">string name of ifcElement</param>
        /// <param name="definedTypes">array of strings for the ifcElement predefinedtype enum property </param>
        /// <returns></returns>
        public bool AddPreDefinedType(string ifcElement, string[] definedTypes)
        {
            if (PreDefinedType.ContainsKey(ifcElement))
            { 
                return false;
            }
            else
            {
                PreDefinedType.Add(ifcElement, definedTypes);
            }
            return true;
        }
        
        /// <summary>
        /// Test for string exists in ItemsToExclude string lists
        /// </summary>
        /// <param name="testStr">String to test</param>
        /// <param name="preDefinedType">strings for the ifcElement predefinedtype enum property</param>
        /// <returns>bool</returns>
        public bool ItemsFilter(string testStr, string preDefinedType = null)
        {
            testStr = testStr.ToUpper();
            //check for predefinedtype enum value passed as string
            bool hasDefinedType = true; //if preDefinedType is null or preDefinedType does not exist in PredefinedType dictionary we need to just test on testStr in return so set to true as default
            if ((preDefinedType != null) &&
                PreDefinedType.ContainsKey(testStr)
                )
            {
                preDefinedType = preDefinedType.ToUpper();
                hasDefinedType = PreDefinedType[testStr].Contains(preDefinedType);
            }

            return (hasDefinedType && (ItemsToExclude.Where(a => testStr.Equals(a)).Count() > 0));
        }

        /// <summary>
        /// Merge together ObjectFilter
        /// </summary>
        /// <param name="mergeFilter">ObjectFilter to merge</param>
        public void Merge(ObjectFilter mergeFilter)
        {
            _itemsToExclude = null; //reset exclude

            //find all includes for the incoming merge ObjectFilter
            var mergeInc = mergeFilter.Items.Where(i => i.Value == true).ToDictionary(i => i.Key, v => v.Value);
            //set the true flag on 'this' Items with same key as incoming merges found above in mergeInc
            foreach (var pair in mergeInc)
            {
                Items[pair.Key] = pair.Value;
            }
            
            var mergeData = this.PreDefinedType.Concat(mergeFilter.PreDefinedType).GroupBy(v => v.Key).ToDictionary(k => k.Key, v => v.SelectMany(x => x.Value).Distinct().ToArray());
            //rebuild PreDefinedType from merge linq statement
            this.PreDefinedType.Clear();
            foreach (var item in mergeData)
            {
                this.PreDefinedType.Add(item.Key, item.Value);
            }

        }

        
    }
}
