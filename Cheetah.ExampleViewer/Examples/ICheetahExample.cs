using CloudInvent.Cheetah.Data.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cheetah.ExampleViewer
{
    /// <summary>
    /// Interface for every example class
    /// </summary>
    public interface ICheetahExample
    {
        void Reset();

        void Run();

        List<CheetahCurve> GetCurrentElements();

    }

    /// <summary>
    /// 
    /// </summary>
    public static class CheetahHelper
    {
        /// <summary>
        /// 
        /// </summary>
        //public static void UpdateGeometryCoordinate(List<CheetahCurve> oldCurveCollection, ICollection<CheetahCurve> resultGeometry)
        //{
        //    if (!resultGeometry.Any()) return;

        //    for (var i = 0; i < oldCurveCollection.Count; i++)
        //        oldCurveCollection[i] = resultGeometry.Single(x => x.Id == oldCurveCollection[i].Id);
        //}


        public static void GetUpdated<TRet>(ref TRet oldCurve, ICollection<CheetahCurve> resultGeometry) where TRet : CheetahCurve
        {
            var oldId = oldCurve.Id;

            var updatedGeo = resultGeometry.SingleOrDefault(x => x.Id == oldId);

            if (updatedGeo != null) oldCurve = (TRet)updatedGeo;
        }

    }

}
