using System;
using System.Runtime.InteropServices;

namespace NppPlugin.DllExport
{
    /// <summary>
    /// Allows exporting of C# to native C.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DllExportAttribute : Attribute
    {
        public CallingConvention CallingConvention { get; set; } = CallingConvention.StdCall;
        public string ExportName { get; set; } = null;
        public DllExportAttribute(string export_name = null, CallingConvention calling_convention = CallingConvention.StdCall)
        {
            this.ExportName = export_name;
            this.CallingConvention = calling_convention;
        }
    }
}