using System;
using Autodesk.DesignScript.Runtime;

namespace DynamoUnits
{
    [SupressImportIntoVM]
    [IsVisibleInDynamoLibrary(false)]
    public enum LengthUnit
    {
        DecimalInch,
        FractionalInch,
        DecimalFoot,
        FractionalFoot,
        Millimeter,
        Centimeter,
        Meter
    }

    [SupressImportIntoVM]
    [IsVisibleInDynamoLibrary(false)]
    public enum AreaUnit
    {
        SquareInch,
        SquareFoot,
        SquareMillimeter,
        SquareCentimeter,
        SquareMeter
    }

    [SupressImportIntoVM]
    [IsVisibleInDynamoLibrary(false)]
    public enum VolumeUnit
    {
        CubicInch,
        CubicFoot,
        CubicMillimeter,
        CubicCentimeter,
        CubicMeter
    }



    [SupressImportIntoVM]
    public class BaseUnit
    {
        private static double epsilon = 1e-6;
        internal double _value;


        private static string _numberFormat = "f4";
        

        public static string NumberFormat
        {
            get { return _numberFormat; }
            set { _numberFormat = value; }
        }
    }

    [IsVisibleInDynamoLibrary(false)]
    public abstract class SIUnit : BaseUnit
    {
        /// <summary>
        /// The internal value of the unit.
        /// </summary>
        [Obsolete("SIUnit.Value is obsolete")]
        public double Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// Construct an SIUnit object with a value.
        /// </summary>
        /// <param name="value"></param>
        protected SIUnit(double value)
        {
            _value = value;
        }

        /// <summary>
        /// Implemented in child classes to control how units are converted
        /// from a string representation to an SI value.
        /// </summary>
        /// <param name="value"></param>
        [Obsolete("SIUnit.SetValueFromString is obsolete.", false)]
        public abstract void SetValueFromString(string value);

    }







}
