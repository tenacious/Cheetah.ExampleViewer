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
    public interface IExampleCode
    {
        void Reset();

        void Run();

        List<CheetahCurve> GetCurrentElements();

    }

    /// <summary>
    /// 
    /// </summary>
    public static class Helper
    {
        public static void GetUpdated<TRet>(ref TRet oldCurve, ICollection<CheetahCurve> resultGeometry) where TRet : CheetahCurve
        {
            var oldId = oldCurve.Id;

            var updatedGeo = resultGeometry.SingleOrDefault(x => x.Id == oldId);

            if (updatedGeo != null) oldCurve = (TRet)updatedGeo;
        }

    }

}
