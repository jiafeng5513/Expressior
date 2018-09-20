using System;
using Autodesk.DesignScript.Runtime;

namespace DynamoUnits
{
    public class NumFormatTransfer
    {
        private static string _numberFormat = "f4";
        
        public static string NumberFormat
        {
            get { return _numberFormat; }
            set { _numberFormat = value; }
        }
    }
}
