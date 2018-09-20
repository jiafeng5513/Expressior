using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
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
    [IsVisibleInDynamoLibrary(false)]
    public enum InsolationUnit
    {
        WattHoursPerMeterSquared,
        KilowattHoursPerMeterSquared,
        BTUPerFootSquared
    }

    [SupressImportIntoVM]
    public class BaseUnit
    {
        private static double epsilon = 1e-6;
        internal double _value;

        private static LengthUnit _hostApplicationInternalLengthUnit = DynamoUnits.LengthUnit.Meter;
        private static AreaUnit _hostApplicationInternalAreaUnit = DynamoUnits.AreaUnit.SquareMeter;
        private static VolumeUnit _hostApplicationInternalVolumeUnit = DynamoUnits.VolumeUnit.CubicMeter;

        private static string _numberFormat = "f4";
        private static string _generalNumberFormat = "G";

        public static double Epsilon
        {
            get { return epsilon; }
        }

        public static LengthUnit HostApplicationInternalLengthUnit
        {
            get { return _hostApplicationInternalLengthUnit; }
            set { _hostApplicationInternalLengthUnit = value; }
        }

        public static AreaUnit HostApplicationInternalAreaUnit
        {
            get { return _hostApplicationInternalAreaUnit; }
            set { _hostApplicationInternalAreaUnit = value; }
        }

        public static VolumeUnit HostApplicationInternalVolumeUnit
        {
            get { return _hostApplicationInternalVolumeUnit; }
            set { _hostApplicationInternalVolumeUnit = value; }
        }

        public static string NumberFormat
        {
            get { return _numberFormat; }
            set { _numberFormat = value; }
        }

        public static string GeneralNumberFormat
        {
            get { return _generalNumberFormat; }
            set { _generalNumberFormat = value; }
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
        /// The value of the unit converted into the
        /// unit type stored on the unit. Ex. If the object
        /// has LengthUnit.DecimalFoot, for a Value of 1.0, this
        /// will return 3.28084
        /// </summary>
        [IsVisibleInDynamoLibrary(false)]
        public abstract double UnitValue { get; }

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

        [Obsolete("SIUnit.Add is obsolete. Please use + instead.", false)]
        public abstract SIUnit Add(SIUnit x);

        [Obsolete("SIUnit.Add is obsolete. Please use + instead.", false)]
        public abstract SIUnit Add(double x);

        [Obsolete("SIUnit.Subtract is obsolete. Please use - instead.", false)]
        public abstract SIUnit Subtract(SIUnit x);

        [Obsolete("SIUnit.Subtract is obsolete. Please use - instead.", false)]
        public abstract SIUnit Subtract(double x);

        [Obsolete("SIUnit.Multiply is obsolete. Please use * instead.", false)]
        public abstract SIUnit Multiply(SIUnit x);

        [Obsolete("SIUnit.Multiply is obsolete. Please use * instead.", false)]
        public abstract SIUnit Multiply(double x);

        [Obsolete("SIUnit.Divide is obsolete. Please use / instead.", false)]
        public abstract dynamic Divide(SIUnit x);

        [Obsolete("SIUnit.Divide is obsolete. Please use / instead.", false)]
        public abstract SIUnit Divide(double x);

        [Obsolete("SIUnit.Modulo is obsolete. Please use % instead.", false)]
        public abstract SIUnit Modulo(SIUnit x);

        [Obsolete("SIUnit.Modulo is obsolete. Please use % instead.", false)]
        public abstract SIUnit Modulo(double x);

        [Obsolete("SIUnit.Round is obsolete. Please use Round instead.", false)]
        public abstract SIUnit Round();

        [Obsolete("SIUnit.Ceiling is obsolete. Please use Ceiling instead.", false)]
        public abstract SIUnit Ceiling();

        [Obsolete("SIUnit.Floor is obsolete. Please use Floor instead.", false)]
        public abstract SIUnit Floor();

        #region operator overloads;

        [Obsolete("SIUnit.+ is obsolete. Please use + instead.", false)]
        public static SIUnit operator +(SIUnit x, SIUnit y)
        {
            return x.Add(y);
        }

        [Obsolete("SIUnit.+ is obsolete. Please use + instead.", false)]
        public static SIUnit operator +(SIUnit x, double y)
        {
            return x.Add(y);
        }

        [Obsolete("SIUnit.+ is obsolete. Please use + instead.", false)]
        public static double operator +(double x, SIUnit y)
        {
            return x + y.Value;
        }

        [Obsolete("SIUnit.- is obsolete. Please use - instead.", false)]
        public static SIUnit operator -(SIUnit x, SIUnit y)
        {
            return x.Subtract(y);
        }

        [Obsolete("SIUnit.- is obsolete. Please use - instead.", false)]
        public static SIUnit operator -(SIUnit x, double y)
        {
            return x.Subtract(y);
        }

        [Obsolete("SIUnit.- is obsolete. Please use - instead.", false)]
        public static double operator -(double x, SIUnit y)
        {
            return x - y.Value;
        }

        [Obsolete("SIUnit.* is obsolete. Please use * instead.", false)]
        public static SIUnit operator *(SIUnit x, SIUnit y)
        {
            return x.Multiply(y);
        }

        [Obsolete("SIUnit.* is obsolete. Please use * instead.", false)]
        public static SIUnit operator *(SIUnit x, double y)
        {
            return x.Multiply(y);
        }

        [Obsolete("SIUnit.* is obsolete. Please use * instead.", false)]
        public static SIUnit operator *(double x, SIUnit y)
        {
            return y.Multiply(x);
        }

        [Obsolete("SIUnit./ is obsolete. Please use / instead.", false)]
        public static dynamic operator /(SIUnit x, SIUnit y)
        {
            //units will cancel
            if (x.GetType() == y.GetType())
            {
                return x.Value / y.Value;
            }

            return x.Divide(y);
        }

        [Obsolete("SIUnit./ is obsolete. Please use / instead.", false)]
        public static SIUnit operator /(SIUnit x, double y)
        {
            return x.Divide(y);
        }

        [Obsolete("SIUnit.% is obsolete. Please use % instead.", false)]
        public static SIUnit operator %(SIUnit x, SIUnit y)
        {
            return x.Modulo(y);
        }

        [Obsolete("SIUnit.% is obsolete. Please use % instead.", false)]
        public static SIUnit operator %(SIUnit x, double y)
        {
            return x.Modulo(y);
        }

        [Obsolete("SIUnit.% is obsolete. Please use % instead.", false)]
        public static double operator %(double x, SIUnit y)
        {
            return x % y.Value;
        }

        [Obsolete("SIUnit.> is obsolete. Please use > instead.", false)]
        public static bool operator >(double x, SIUnit y)
        {
            return x > y.Value;
        }

        [Obsolete("SIUnit.> is obsolete. Please use > instead.", false)]
        public static bool operator >(SIUnit x, double y)
        {
            return x.Value > y;
        }

        [Obsolete("SIUnit.> is obsolete. Please use > instead.", false)]
        public static bool operator >(SIUnit x, SIUnit y)
        {
            return x.GetType() == y.GetType() && x.Value > y.Value;
        }

        [Obsolete("SIUnit.< is obsolete. Please use < instead.", false)]
        public static bool operator <(double x, SIUnit y)
        {
            return x < y.Value;
        }

        [Obsolete("SIUnit.< is obsolete. Please use < instead.", false)]
        public static bool operator <(SIUnit x, double y)
        {
            return x.Value < y;
        }

        [Obsolete("SIUnit.< is obsolete. Please use < instead.", false)]
        public static bool operator <(SIUnit x, SIUnit y)
        {
            return x.GetType() == y.GetType() && x.Value < y.Value;
        }

        [Obsolete("SIUnit.>= is obsolete. Please use >= instead.", false)]
        public static bool operator >=(double x, SIUnit y)
        {
            return x >= y.Value;
        }

        [Obsolete("SIUnit.>= is obsolete. Please use >= instead.", false)]
        public static bool operator >=(SIUnit x, double y)
        {
            return x.Value >= y;
        }

        [Obsolete("SIUnit.>= is obsolete. Please use >= instead.", false)]
        public static bool operator >=(SIUnit x, SIUnit y)
        {
            return x.GetType() == y.GetType() && x.Value >= y.Value;
        }

        [Obsolete("SIUnit.<= is obsolete. Please use <= instead.", false)]
        public static bool operator <=(double x, SIUnit y)
        {
            return x <= y.Value;
        }

        [Obsolete("SIUnit.<= is obsolete. Please use <= instead.", false)]
        public static bool operator <=(SIUnit x, double y)
        {
            return x.Value <= y;
        }

        [Obsolete("SIUnit.<= is obsolete. Please use <= instead.", false)]
        public static bool operator <=(SIUnit x, SIUnit y)
        {
            return x.GetType() == y.GetType() && x.Value <= y.Value;
        }

        [Obsolete("SIUnit.ToSIUnit is obsolete.", false)]
        public static SIUnit ToSIUnit(object value)
        {
            return value as SIUnit;
        }

        #endregion

        public static Dictionary<string, double> Conversions
        {
            get
            {
                return new Dictionary<string, double>();
            }
        }

        [Obsolete("SIUnit.ConvertToHostUnits is obsolete. Please use Convert Between Units.", false)]
        public abstract double ConvertToHostUnits();
    }







}
