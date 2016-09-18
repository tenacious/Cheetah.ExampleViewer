using CloudInvent.Cheetah.Data;
using CloudInvent.Cheetah.Data.DataSetBuilder;
using CloudInvent.Cheetah.Data.Geometry;
using CloudInvent.Cheetah.Data.Interfaces;
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
    public class TangentSegment : IExampleCode
    {
        CheetahLine2D line1;
        CheetahLine2D line2;

        CheetahCircle2D circle1;
        CheetahCircle2D circle2;

        public void Reset()
        {
            // 1. Creating geometric model

            Circle1RadiusValue = 10;
            CircleRadiusValue = 50;

            //line1 = new CheetahLine2D(0.5, -0.5, 8.5, 0.5);
            //line2 = new CheetahLine2D(10.5, -11.5, 9.5, 8.5);
            line1 = new CheetahLine2D(0.5, 10, 70, 70);
            line2 = new CheetahLine2D(10.5, -21.5, 70, -50);

            circle1 = new CheetahCircle2D(-1, 0, Circle1RadiusValue);
            circle2 = new CheetahCircle2D(70, 0, CircleRadiusValue);
        }

        [DisplayName("Circle 1 Radius Value")]
        public double Circle1RadiusValue { get; set; }

        [DisplayName("Circle 2 Radius Value")]
        public double CircleRadiusValue { get; set; }

        [DisplayName("End Point on circle")]
        public bool IsPointOnCurve { get; set; }

        [DisplayName("Tangent Constrain Active")]
        public bool IsTangentActive { get; set; }

        //[DisplayName("Perpendicular Constrain Active")]
        //public bool IsPerpendicularActive { get; set; }

        //[DisplayName("Equal Radius Constrain Active")]
        //public bool IsEqualRadiusActive { get; set; }



        //[DisplayName("Equal Segment Constrain Active")]
        //public bool IsEqualSegmentActive { get; set; }

        //[DisplayName("Is Vertical")]
        //public bool IsVertical { get; set; }

        public void Run()
        {
            var dataSet = new CheetahDataSet();

            dataSet.AddDiameter(circle1, Circle1RadiusValue);
            dataSet.AddDiameter(circle2, CircleRadiusValue);

            if (IsPointOnCurve)
            {
                dataSet.AddPointOnCurve(line1, IdentifiableValueReferences.LineStart, circle1);
                dataSet.AddPointOnCurve(line1, IdentifiableValueReferences.LineEnd, circle2);

                dataSet.AddPointOnCurve(line2, IdentifiableValueReferences.LineStart, circle1);
                dataSet.AddPointOnCurve(line2, IdentifiableValueReferences.LineEnd, circle2);

            }


            if (IsTangentActive)
            {
                dataSet.AddTangent(circle1, line1);
                dataSet.AddTangent(circle2, line1);

                dataSet.AddTangent(circle1, line2);
                dataSet.AddTangent(circle2, line2);

            }


            // 2. Creating solver object
            var solver = new SolverCpu11(null);

            // 3. Creating parametric object and setting tolerance (by default 1E-12)
            var parametric = new CheetahParametricBasic(() => solver, false);

            const double precision = 1E-15; // Working with much better accuracy then the default 1E-12 

            CheetahParametricBasic.Settings.Precision = precision;

            // 4. Initializing parametric object using data set
            if (!parametric.Init(dataSet, null, null))
                throw new Exception("Something goes wrong");

            // 5. Running constraints solver
            if (!parametric.Evaluate(true))
                throw new Exception("Something goes wrong");

            // 6. Retrieving results (we created rectangle with fillets that is "closest" to the initial lines and arcs)
            var rslt = parametric.GetSolution(true);

            if (rslt.Any())
            {
                Helper.GetUpdated(ref line1, rslt);
                Helper.GetUpdated(ref line2, rslt);

                Helper.GetUpdated(ref circle1, rslt);
                Helper.GetUpdated(ref circle2, rslt);
            }
        }

        public List<CheetahCurve> GetCurrentElements()
        {
            return new List<CheetahCurve>() { line1, line2, circle1, circle2 };
        }
    }
}




