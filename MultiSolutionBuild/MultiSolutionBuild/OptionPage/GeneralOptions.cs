using System.ComponentModel;

namespace MultiSolutionBuild.OptionPage
{
    internal class GeneralOptions : BaseOptionModel<GeneralOptions>
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

        [Category("MultiSolution Config Category")]
        [DisplayName("CompileEndAction")]
        [Description("take action after compile end")]
        [TypeConverter(typeof(EnumConverter))] // This will make use of enums more resilient
        public CompileEndAction CompileEndAction { get; set; }

        [DefaultValue(true)]
        public bool CopyToTargetFolder { get; set; }


        [Browsable(false)] // This will hide it from the Tools -> Options page, but still work like normal
        public bool HiddenProperty { get; set; } = true;
    }


    internal enum CompileEndAction
    {
        Nothing = 0,
        UnloadProjectAdded = 1,
        DeleteProjectAdded = 2
    }

    // if DialogPage string array not persisted
    // https://stackoverflow.com/questions/32751040/store-array-in-options-using-dialogpage
}
