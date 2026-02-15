namespace GrblHalSender.Utility;

public class CalibrationTriangle
{
        
        
    public double Delta { get; set; }

    public CalibrationTriangle()
    {
            
    }

    public Triangle CalculateTriangle(double a, double b)
    {
        var aSqr = Math.Pow(a, 2);
        var bSqr = Math.Pow(b, 2);

        //radian 1.5708 = 90 degrees  × π/180
        var cSide = Math.Sqrt(bSqr + aSqr - 2 * (b * a) * Math.Cos(1.5708));
        return CalculateAngle(a, b, cSide);
    }


    public Triangle CalculateResults(double a, double b, double c)
    {
        return   CalculateAngle(a, b, c);
    }
    public Triangle CalculateAngle(double a, double b, double c)
    {
        var aSqr = Math.Pow(a, 2);
        var bSqr = Math.Pow(b, 2);
        var cSqr = Math.Pow(c, 2);
        var aAngle = (bSqr + cSqr - aSqr) / ((b * c) * 2);
        var bAngle = (aSqr + cSqr - bSqr) / ((a * c) * 2);
        var cAngle = (aSqr + bSqr - cSqr) / ((a * b) * 2);
        var radA = Math.Acos(aAngle);
        var radB = Math.Acos(bAngle);
        var radC = Math.Acos(cAngle);
        var angleA = radA * (180 / Math.PI);
        var angleB = radB * (180 / Math.PI);
        var angleC = radC * (180 / Math.PI);
        var formattedAngleC = Math.Round(angleA, 4);
        var formattedAngleA = Math.Round(angleB, 4);
        var formattedAngleB = Math.Round(angleC, 4);
        return new Triangle(a, b, c, formattedAngleA, formattedAngleB, formattedAngleC);
        
    }

    public double CalculateDelta(Triangle triangle)
    {
        var cor = 90 - triangle.AngleB;
        var delta = Math.Sin(cor * (Math.PI / 180)) * triangle.AngleB;
        var formattedDelta = Math.Round(delta, 4);
        return formattedDelta;
    }

}


public struct Triangle
{
    public double SideA { get; set; }
    public double SideB { get; set; }
    public double SideC { get; set; }

    public double AngleA { get; set; }
    public double AngleB { get; set; }
    public double AngleC { get; set; }
    public Triangle(double sideA, double sideB, double sideC, double angleA, double angleB, double angleC)
    {
        SideA = sideA;
        SideB = sideB;
        SideC = sideC;
        AngleA = angleA;
        AngleB = angleB;
        AngleC = angleC;
    }

}