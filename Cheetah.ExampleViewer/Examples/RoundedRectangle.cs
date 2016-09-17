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
    public class RoundedRectangle : IExampleCode
    {
        CheetahLine2D line1;
        CheetahLine2D line2;
        CheetahLine2D line3;
        CheetahLine2D line4;

        CheetahArc2D arc1;
        CheetahArc2D arc2;
        CheetahArc2D arc3;
        CheetahArc2D arc4;

        public void Reset()
        {
            // 1. Creating geometric model

            line1 = new CheetahLine2D(0.5, -0.5, 8.5, 0.5);
            line2 = new CheetahLine2D(10.5, 1.5, 9.5, 8.5);
            line3 = new CheetahLine2D(9.5, 9.5, 0.5, 10.5);
            line4 = new CheetahLine2D(0.5, 8.5, -0.5, 1.5);

            arc1 = new CheetahArc2D(new CheetahPoint2D(1.5, 1.5), -Math.PI, -Math.PI * 0.5, 1.25);
            arc2 = new CheetahArc2D(new CheetahPoint2D(9.25, 0.75), -Math.PI * 0.5, 0.0, 0.5);
            arc3 = new CheetahArc2D(new CheetahPoint2D(9.5, 9.5), Math.PI * 0.1, Math.PI * 0.5, 1.5);
            arc4 = new CheetahArc2D(new CheetahPoint2D(1.2, 8.5), Math.PI * 0.25, Math.PI, 0.75);
        }

        [DisplayName("Parellel Constrain Active")]
        public bool IsParallelActive { get; set; }

        [DisplayName("Coincidence Constrain Active")]
        public bool IsCoincidenceActive { get; set; }

        [DisplayName("Perpendicular Constrain Active")]
        public bool IsPerpendicularActive { get; set; }

        [DisplayName("Tangent Constrain Active")]
        public bool IsTangentActive { get; set; }

        [DisplayName("Equal Radius Constrain Active")]
        public bool IsEqualRadiusActive { get; set; }

        [DisplayName("Arc Radius Value")]
        public double ArcRadiusValue { get; set; }

        [DisplayName("Equal Segment Constrain Active")]
        public bool IsEqualSegmentActive { get; set; }

        public void Run()
        {
            var dataSet = new CheetahDataSet();

            if (IsCoincidenceActive)
            {

                dataSet.AddCoincidence(arc1, IdentifiableValueReferences.ArcEnd,
                    line1, IdentifiableValueReferences.LineStart);

                dataSet.AddCoincidence(line1, IdentifiableValueReferences.LineEnd,
                    arc2, IdentifiableValueReferences.ArcStart);

                dataSet.AddCoincidence(arc2, IdentifiableValueReferences.ArcEnd,
                    line2, IdentifiableValueReferences.LineStart);

                dataSet.AddCoincidence(line2, IdentifiableValueReferences.LineEnd,
                    arc3, IdentifiableValueReferences.ArcStart);

                dataSet.AddCoincidence(arc3, IdentifiableValueReferences.ArcEnd,
                    line3, IdentifiableValueReferences.LineStart);

                dataSet.AddCoincidence(line3, IdentifiableValueReferences.LineEnd,
                    arc4, IdentifiableValueReferences.ArcStart);

                dataSet.AddCoincidence(arc4, IdentifiableValueReferences.ArcEnd,
                    line4, IdentifiableValueReferences.LineStart);

                dataSet.AddCoincidence(line4, IdentifiableValueReferences.LineEnd,
                    arc1, IdentifiableValueReferences.ArcStart);

            }
            // 3.2. Creating perpendicular constraints between line segments 1 and 2

            if (IsPerpendicularActive)
            {
                dataSet.AddPerpendicular(line1, line2);
            }

            // 3.3. Creating two parallel constraints between line segments 1 and 3 and line segments 2 and 4 

            if (IsParallelActive)
            {
                dataSet.AddParallel(line1, line3);
                dataSet.AddParallel(line2, line4);
            }

            // 3.4. Creating "equal radius" constraints for all arcs

            if (IsEqualRadiusActive)
            {
                dataSet.AddEqual(arc1, arc2);
                dataSet.AddEqual(arc2, arc3);
                dataSet.AddEqual(arc3, arc4);
            }

            if (IsTangentActive)
            {
                dataSet.AddTangent(arc1, line1);
                dataSet.AddTangent(arc2, line1);
                dataSet.AddTangent(arc2, line2);
                dataSet.AddTangent(arc3, line2);
                dataSet.AddTangent(arc3, line3);
                dataSet.AddTangent(arc4, line3);
                dataSet.AddTangent(arc4, line4);
                dataSet.AddTangent(arc1, line4);
            }

            if (ArcRadiusValue > 0)
            {
                dataSet.AddRadius(arc1, ArcRadiusValue);
            }

            if (IsEqualSegmentActive)
            {
                dataSet.AddEqual(line1, line2);
                dataSet.AddEqual(line2, line3);
                dataSet.AddEqual(line3, line4);
            }

            // 2. Creating solver object
            var solver = new SolverCpu11();

            // 3. Creating parametric object and setting tolerance (by default 1E-12)
            var parametric = new CheetahParametricBasic(() => solver, false);

            const double precision = 1E-15; // Working with much better accuracy then the default 1E-12 

            CheetahParametricBasic.Settings.Precision = precision;

            // 4. Initializing parametric object using data set
            if (!parametric.Init(dataSet, null, null))
                throw new Exception("Something goes wrong");

            // 5. Running constraints solver
            if (!parametric.Evaluate())
                throw new Exception("Something goes wrong");

            // 6. Retrieving results (we created rectangle with fillets that is "closest" to the initial lines and arcs)
            var rslt = parametric.GetSolution(true);

            if (rslt.Any())
            {
                Helper.GetUpdated(ref line1, rslt);
                Helper.GetUpdated(ref line2, rslt);
                Helper.GetUpdated(ref line3, rslt);
                Helper.GetUpdated(ref line4, rslt);

                Helper.GetUpdated(ref arc1, rslt);
                Helper.GetUpdated(ref arc2, rslt);
                Helper.GetUpdated(ref arc3, rslt);
                Helper.GetUpdated(ref arc4, rslt);
            }
        }

        public List<CheetahCurve> GetCurrentElements()
        {
            return new List<CheetahCurve>() { line1, line2, line3, line4, arc1, arc2, arc3, arc4 };
        }
    }
}




