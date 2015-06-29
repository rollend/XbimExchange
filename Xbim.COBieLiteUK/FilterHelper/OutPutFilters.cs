﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.IO;
using System.Reflection;
using Xbim.COBieLiteUK.FilterHelper;
using Xbim.COBieLiteUK;
using Newtonsoft.Json;

namespace XbimExchanger.COBieLiteHelpers
{
    public class OutPutFilters
    {
        #region Properties
        /// <summary>
        /// IfcProduct Exclude filters
        /// </summary>
        public ObjectFilter IfcProductFilter { get;  set; }
        /// <summary>
        /// IfcTypeObject Exclude filters
        /// </summary>
        public ObjectFilter IfcTypeObjectFilter { get;  set; }
        /// <summary>
        /// IfcAssembly Exclude filters
        /// </summary>
        public ObjectFilter IfcAssemblyFilter { get; set; }

        /// <summary>
        /// Zone attribute filters
        /// </summary>
        public PropertyFilter ZoneFilter { get;  set; }
        /// <summary>
        /// Type attribute filters
        /// </summary>
        public PropertyFilter TypeFilter { get;  set; }
        /// <summary>
        /// Space attribute filters
        /// </summary>
        public PropertyFilter SpaceFilter { get;  set; }
        /// <summary>
        /// Floor attribute filters
        /// </summary>
        public PropertyFilter FloorFilter { get;  set; }
        /// <summary>
        /// Facility attribute filters
        /// </summary>
        public PropertyFilter FacilityFilter { get;  set; }
        /// <summary>
        /// Spare attribute filters
        /// </summary>
        public PropertyFilter SpareFilter { get;  set; }
        /// <summary>
        /// Component attribute filters
        /// </summary>
        public PropertyFilter ComponentFilter { get;  set; }
        /// <summary>
        /// Common attribute filters
        /// </summary>
        public PropertyFilter CommonFilter { get;  set; }
        #endregion

        #region Constructor methods

        /// <summary>
        /// Empty constructor for Serialize
        /// </summary>
        public OutPutFilters()
        {

        }
        
        /// <summary>
        /// Constructor for default set configFileName = null, or passed in configuration file path
        /// </summary>
        /// <param name="configFileName"></param>
        public OutPutFilters(string configFileName)
        {
            FiltersHelperInit(configFileName);
        }
       

        /// <summary>
        /// Constructor, will read Configuration file if passed, or default COBieAttributesFilters.config
        /// </summary>
        /// <param name="configFileName">Full path/name for config file</param>
        private void FiltersHelperInit(string configFileName = null)
        {
            string resFile = configFileName;
            //set default
            if (resFile == null)
            {
                 resFile = "Xbim.COBieLiteUK.FilterHelper.COBieAttributesFilters.config";               
            }
            
            Configuration config = GetResourceConfig(resFile);

            //IfcProduct and IfcTypeObject filters
            IfcProductFilter = new ObjectFilter(config.GetSection("IfcElementInclusion"));
            IfcTypeObjectFilter = new ObjectFilter(config.GetSection("IfcTypeInclusion"));
            IfcAssemblyFilter = new ObjectFilter(config.GetSection("IfcAssemblyInclusion"));
            
            //Property name filters
            ZoneFilter = new PropertyFilter(config.GetSection("ZoneFilter"));
            TypeFilter = new PropertyFilter(config.GetSection("TypeFilter"));
            SpaceFilter = new PropertyFilter(config.GetSection("SpaceFilter"));
            FloorFilter = new PropertyFilter(config.GetSection("FloorFilter"));
            FacilityFilter = new PropertyFilter(config.GetSection("FacilityFilter"));
            SpareFilter = new PropertyFilter(config.GetSection("SpareFilter"));
            ComponentFilter = new PropertyFilter(config.GetSection("ComponentFilter"));
            CommonFilter = new PropertyFilter(config.GetSection("CommonFilter"));

            if (configFileName == null)
            {
                File.Delete(config.FilePath);
            }
            
        }

        private Configuration GetResourceConfig(string resFileName)
        {
            string tmpFile = resFileName;
            bool isResourcFile = !File.Exists(resFileName);
            if (isResourcFile)
            {
                tmpFile = Path.GetTempPath() + Guid.NewGuid().ToString() + ".tmp";

                var asss = System.Reflection.Assembly.GetExecutingAssembly();

                using (var input = asss.GetManifestResourceStream(resFileName))
                {
                    if (input != null)
                    {
                        using (var output = File.Create(tmpFile))
                        {
                            if (input != null) input.CopyTo(output);
                        }
                    }
                }

                if (!File.Exists(tmpFile))
                {
                    var directory = new DirectoryInfo(".");
                    throw new FileNotFoundException(string.Format(@"Error loading configuration file ""{0}"". App folder is ""{1}"".", tmpFile, directory.FullName));
                }
            }

            Configuration config;
            try
            {
                var configMap = new ExeConfigurationFileMap { ExeConfigFilename = tmpFile };
                config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException(string.Format(@"Error loading configuration file ""{0}"". Error: {1}", tmpFile, ex.Message));
            }

            return config;
        }
        #endregion

        #region Filter Methods

        /// <summary>
        /// Test property Names against sheets
        /// </summary>
        /// <param name="testStr">Name string to test</param>
        /// <param name="parent">Parent object</param>
        /// <returns>bool</returns>
        public bool NameFilterOnParent(string testStr, CobieObject parent = null)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(testStr))
            {
                result = CommonFilter.NameFilter(testStr);
                if (!result)
                {
                    if (parent == null)
                        result = false;
                    else if ((parent is Zone) && (ZoneFilter != null))
                        result = ZoneFilter.NameFilter(testStr);
                    else if ((parent is AssetType) && (TypeFilter != null))
                        result = TypeFilter.NameFilter(testStr);
                    else if ((parent is Space) && (SpaceFilter != null))
                        result = SpaceFilter.NameFilter(testStr);
                    else if ((parent is Floor) && (FloorFilter != null))
                        result = FloorFilter.NameFilter(testStr);
                    else if ((parent is Facility) && (FacilityFilter != null))
                        result = FacilityFilter.NameFilter(testStr);
                    else if ((parent is Spare) && (SpareFilter != null))
                        result = SpareFilter.NameFilter(testStr);
                    else if ((parent is Asset) && (ComponentFilter != null))
                        result = ComponentFilter.NameFilter(testStr);
                    else
                        result = false;
                }
            }
            return result;
        }

        /// <summary>
        /// Test Property Set Names against sheets
        /// </summary>
        /// <param name="testStr">Name string to test</param>
        /// <param name="parent">Parent object</param>
        /// <returns>bool</returns>
        public bool PSetNameFilterOnSheetName(string testStr, CobieObject parent = null)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(testStr))
            {
                result = CommonFilter.PSetNameFilter(testStr);
                if (!result)
                {
                    if (parent == null)
                        result = false;
                    else if ((parent is Zone) && (ZoneFilter != null))
                        result = ZoneFilter.PSetNameFilter(testStr);
                    else if ((parent is AssetType) && (TypeFilter != null))
                        result = TypeFilter.PSetNameFilter(testStr);
                    else if ((parent is Space) && (SpaceFilter != null))
                        result = SpaceFilter.PSetNameFilter(testStr);
                    else if ((parent is Floor) && (FloorFilter != null))
                        result = FloorFilter.PSetNameFilter(testStr);
                    else if ((parent is Facility) && (FacilityFilter != null))
                        result = FacilityFilter.PSetNameFilter(testStr);
                    else if ((parent is Spare) && (SpareFilter != null))
                        result = SpareFilter.PSetNameFilter(testStr);
                    else if ((parent is Asset) && (ComponentFilter != null))
                        result = ComponentFilter.PSetNameFilter(testStr);
                    else
                        result = false;
                }
            }
            return result;
        }

        /// <summary>
        /// Filter IfcProduct and IfcTypeObject types
        /// </summary>
        /// <param name="obj">CobieObject</param>
        /// <returns>bool</returns>
        public bool ObjFilter(CobieObject obj, string preDefinedType = null)
        {
            if (!string.IsNullOrEmpty(obj.ExternalEntity))
            {
                if ((obj is Asset) && (IfcProductFilter != null))
                    return IfcProductFilter.ItemsFilter(obj.ExternalEntity, preDefinedType);
                else if ((obj is AssetType) && (IfcTypeObjectFilter != null))
                    return IfcTypeObjectFilter.ItemsFilter(obj.ExternalEntity);
            }
            return false;
        }
        #endregion

        /// <summary>
        /// Merge OutPutFilters
        /// </summary>
        /// <param name="mergeFilter">OutPutFilters</param>
        public void Merge(OutPutFilters mergeFilter)
        {
            IfcProductFilter.Merge(mergeFilter.IfcProductFilter);
            IfcTypeObjectFilter.Merge(mergeFilter.IfcTypeObjectFilter);
            IfcAssemblyFilter.Merge(mergeFilter.IfcAssemblyFilter);


        }

        #region Serialize
        
        /// <summary>
        /// Save object as xml file
        /// </summary>
        /// <param name="filename">FileInfo</param>
        public void SerializeXML(FileInfo filename)
        {
            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(OutPutFilters));
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename.FullName))
            {
                writer.Serialize(file, this);
            }
        }

        /// <summary>
        /// Create a OutPutFilters object from a XML file
        /// </summary>
        /// <param name="filename">FileInfo</param>
        /// <returns>OutPutFilters</returns>
        public static OutPutFilters DeserializeXML(FileInfo filename)
        {
            OutPutFilters result = null;
            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(OutPutFilters));
            using (System.IO.StreamReader file = new System.IO.StreamReader(filename.FullName))
            {
                result =  (OutPutFilters)writer.Deserialize(file);
            }
            return result;
        }

        /// <summary>
        /// Save object as JSON 
        /// </summary>
        /// <param name="filename">FileInfo</param>
        public void SerializeJSON (FileInfo filename)
        {
            JsonSerializer writer = new JsonSerializer();
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename.FullName))
            {
                writer.Serialize(file, this);
            }
        }

        /// <summary>
        /// Create a OutPutFilters object from a JSON file
        /// </summary>
        /// <param name="filename">FileInfo</param>
        /// <returns>OutPutFilters</returns>
        public static OutPutFilters DeserializeJSON(FileInfo filename)
        {
            OutPutFilters result = null;
            JsonSerializer writer = new JsonSerializer();
            using (System.IO.StreamReader file = new System.IO.StreamReader(filename.FullName))
            {
                result = (OutPutFilters)writer.Deserialize(file, typeof(OutPutFilters));
            }
            return result;
        }

        #endregion
    }
}
