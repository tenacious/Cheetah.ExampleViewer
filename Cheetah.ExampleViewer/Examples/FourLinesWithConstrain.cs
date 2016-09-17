using CloudInvent.Cheetah.Data;
using CloudInvent.Cheetah.Data.DataSetBuilder;
using CloudInvent.Cheetah.Data.Geometry;
using CloudInvent.Cheetah.Data.ValueReference;
using CloudInvent.Cheetah.Parametric;
using CloudInvent.Cheetah.Solver.Cpu11;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cheetah.ExampleViewer
{
    //based on : https://github.com/CloudInvent/Cheetah.Examples/blob/master/Cheetah.Solver.API.Examples/Example1.FourLinesWithConstraints/Program1.cs

    [DisplayName("Four Lines With Constrain")]
    public class FourLinesWithConstrain : IExampleCode
    {
        CheetahLine2D line1;
        CheetahLine2D line2;
        CheetahLine2D line3;
        CheetahLine2D line4;

        public void Reset()
        {
            //My sketched geometry , positioned approximately in space
            line1 = new CheetahLine2D(0, 0, 10, 1);
            line2 = new CheetahLine2D(10, 0, 10, 11);
            line3 = new CheetahLine2D(10, 10, 1, 10);
            line4 = new CheetahLine2D(0, 10, 1, 1);
        }

        [DisplayName("Parellel Constrain Active")]
        public bool IsParallelActive { get; set; }

        [DisplayName("Coincidence Constrain Active")]
        public bool IsCoincidenceActive { get; set; }

        [DisplayName("Perpendicular Constrain Active")]
        public bool IsPerpendicularActive { get; set; }

        public void Run()
        {
            // 1. Creating data set
            var dataSet = new CheetahDataSet();

            if (IsCoincidenceActive)
            {
                dataSet.AddCoincidence(line1, IdentifiableValueReferences.LineEnd,
                    line2, IdentifiableValueReferences.LineStart);

                dataSet.AddCoincidence(line2, IdentifiableValueReferences.LineEnd,
                    line3, IdentifiableValueReferences.LineStart);

                dataSet.AddCoincidence(line3, IdentifiableValueReferences.LineEnd,
                    line4, IdentifiableValueReferences.LineStart);

                dataSet.AddCoincidence(line4, IdentifiableValueReferences.LineEnd,
                    line1, IdentifiableValueReferences.LineStart);

            }

            if (IsPerpendicularActive)
            {
                dataSet.AddPerpendicular(line1, line2);
                dataSet.AddPerpendicular(line2, line3);
            }

            if (IsParallelActive)
            {
                dataSet.AddParallel(line2, line4);
            }

            // 4. Creating solver object
            var solver = new SolverCpu11();

            // 5. Creating parametric object and setting tolerance (by default 1E-12)
            var parametric = new CheetahParametricBasic(() => solver, false, true, true);

            const double precision = 1E-14; // Working with better accuracy then default 1E-12

            CheetahParametricBasic.Settings.Precision = precision;

            // 6. Initializing parametric object using data set
            if (!parametric.Init(dataSet, null, null))
                throw new Exception("Something goes wrong");

            // 7. Regenerating constrained model (running solver)
            if (!parametric.Evaluate())
                throw new Exception("Something goes wrong");

            // 8. Retrieving results (we created rectangle that is "closest" to the initial lines)
            var rslt = parametric.GetSolution(true);

            //CheetahHelper.UpdateGeometryCoordinate(GetCurrentElements(), resultGeometry);

            if (rslt.Any())
            {
                Helper.GetUpdated(ref line1, rslt);
                Helper.GetUpdated(ref line2, rslt);
                Helper.GetUpdated(ref line3, rslt);
                Helper.GetUpdated(ref line4, rslt);
            }

            GC.Collect();

        }

        public List<CheetahCurve> GetCurrentElements()
        {
            return new List<CheetahCurve> { line1, line2, line3, line4 };
        }
    }
}
