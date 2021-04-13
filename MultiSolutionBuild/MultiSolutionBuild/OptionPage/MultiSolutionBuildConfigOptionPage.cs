using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiSolutionBuild.OptionPage
{
    public class MultiSolutionBuildConfigOptionPage: DialogPage
    {
        /// <summary>
        /// remember add TypeConverterAttribute
        /// https://stackoverflow.com/questions/24291249/dialogpage-string-array-not-persisted
        /// </summary>
        [Category("MultiSolution Config Category")]
        [DisplayName("SolutionLocations")]
        [Description("solutions need to build.")]
        [TypeConverter(typeof(StringArrayConverter))]
        public string[] SolutionLocations { get; set; }

        public bool AutoUnloadAfterBuild { get; set; }


    }

    // if DialogPage string array not persisted
    // https://stackoverflow.com/questions/32751040/store-array-in-options-using-dialogpage

    /// <summary>
    /// while using custom class it must have a default ctor
    /// </summary>
    public class PathConfig
    {
        public string Path { get; set; }

        public PathConfig()
        {
        }
    }
}
