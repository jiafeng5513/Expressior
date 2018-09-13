using System;
using System.Collections.Generic;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

namespace Analysis
{
    [IsVisibleInDynamoLibrary(false)]
    public interface IStructuredData<TLocation, TValue>
    {
        /// <summary>
        /// A collection of calculation locations.
        /// </summary>
        IEnumerable<TLocation> ValueLocations { get; }

        /// <summary>
        /// A list of values corresponding to each location.
        /// </summary>
        IList<TValue> Values { get; } 
    }
    [IsVisibleInDynamoLibrary(false)]
    public interface ISurfaceData<TLocation, TValue> : IStructuredData<TLocation, TValue>
    {
        /// <summary>
        /// The analysis surface.
        /// </summary>
        Surface Surface { get; }
    }
    [IsVisibleInDynamoLibrary(false)]
    public interface ICurveData<TLocation, TValue> : IStructuredData<TLocation, TValue>
    {
        /// <summary>
        /// The analysis curve.
        /// </summary>
        Curve Curve { get; }
    }

    /// <summary>
    /// An analysis model.
    /// </summary>
    /// <typeparam name="TLocation">The analysis location type. (i.e. UV, Point)</typeparam>
    /// <typeparam name="TValue">The analysis return type. (i.e. double, vector, SIUnit)</typeparam>
    [IsVisibleInDynamoLibrary(false)]
    public interface IAnalysisModel<TLocation, TValue>
    {
        event EventHandler AnalysisCompleted;
        void Analyze(bool parallel);
        IEnumerable<IStructuredData<TLocation, TValue>> GetResultsByName(string name);
    }
}
