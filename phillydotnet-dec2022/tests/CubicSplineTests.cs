using MathNet.Numerics.Interpolation;
using Serilog;
using tests.Util;
using Xunit.Abstractions;

namespace tests;

#pragma warning disable xUnit1033
public class CubicSplineTests : BaseTest, IClassFixture<ConfigurationFixture>
#pragma warning restore xUnit1033
{
    [Fact]
    public void test_cubic_spline_stuff()
    {
        var xCoords = new double[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var yCoords = new double[] { 10, 30, 50, -40, -60, 0, 10, 20 };
        var naturalSpline = CubicSpline.InterpolateNatural(xCoords, yCoords);

        //
        //  endpoints have f''(x) = 0
        Assert.Equal(0.0, naturalSpline.Differentiate2(1), 4);
        Assert.Equal(0.0, naturalSpline.Differentiate2(8), 4);

        //
        //  obtain local extrema
        var criticalPoints = naturalSpline.StationaryPoints();
        Assert.True(criticalPoints.Length > 0);
        foreach (var criticalPoint in criticalPoints)
        {
            Log.Logger.Information(
                "Critical point {x} interpolates at {y} with first derivative {d1} and second derivative {d2}"
                , criticalPoint
                , naturalSpline.Interpolate(criticalPoint)
                , naturalSpline.Differentiate(criticalPoint)
                , naturalSpline.Differentiate2(criticalPoint));
        }
    }

    #region Constructor

    public CubicSplineTests(ConfigurationFixture configuration, ITestOutputHelper output) : base(configuration,
        output)
    {
    }

    #endregion
}