﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class MathConstants
{
    public static float GravitationalConstant { get { return 6.674e-11f; } }
    public static float StandardPressure { get { return 101325f; } }
    public static float IdealGasConstant { get { return 8.214f; } }
}

public static class MathUtility
{
    public static int RandomIndex(int count)
    {
        return (int)(Random.value* 0.999999f* count);
    }

    public static T RandomElement<T>(this IEnumerable<T> enumerable)
    {
        List<T> list = new List<T>(enumerable);

        return list[RandomIndex(list.Count)];
    }

    public static T WeightedRandomElement<T>(this Dictionary<T, float> weighted_elements)
    {
        float total_weight = 0;
        foreach (T element in weighted_elements.Keys)
            total_weight += weighted_elements[element];

        float value = Random.value * total_weight;

        foreach (T element in weighted_elements.Keys)
        {
            float weight = weighted_elements[element];

            if (value <= weight)
                return element;
            else
                value -= weight;
        }

        return default(T);
    }

    public static T WeightedRandomElement<T>(this IEnumerable<KeyValuePair<T, float>> weighted_elements)
    {
        return weighted_elements.ToDictionary(pair => pair.Key, pair => pair.Value)
            .WeightedRandomElement();
    }

    public static T RemoveRandomElement<T>(List<T> list)
    {
        return list.TakeAt(RandomIndex(list.Count));
    }

    public static bool Roll(float p)
    {
        return Random.value > (1- p);
    }

    public static bool Flip()
    {
        return Roll(0.5f);
    }

    static float SimpleRecursiveLimit(float scale, int recursions_left)
    {
        if (Mathf.Abs(scale) > 0.5f)
            return scale > 0 ? float.PositiveInfinity : float.NegativeInfinity;

        if (recursions_left == 0)
            return scale;

        return scale* (1 + SimpleRecursiveLimit(scale, recursions_left - 1));
    }

    public static float SimpleRecursiveLimit(float scale)
    {
        return SimpleRecursiveLimit(scale, 5);
    }

    public static T Gather<T>(IEnumerable<T> enumerable, System.Func<T, T, T> function)
    {
        if (enumerable.Count() < 2)
            throw new System.Exception("Tried to Gather() an Enumerable with less than 2 elements.");

        IEnumerator<T> enumerator = enumerable.GetEnumerator();

        T first = enumerator.Current;
        enumerator.MoveNext();
        T second = enumerator.Current;

        T gather = function(first, second);

        while (enumerator.MoveNext())
            gather = function(gather, enumerator.Current);

        return gather;
    }

    public static bool NearlyEqual(float a, float b, float tolerance = 0.001f)
    {
        return a > (b - tolerance) && a < (b + tolerance);
    }

    public static List<List<T>> Permute<T>(IEnumerable<T> enumerable)
    {
        if (enumerable.Count() == 1)
            return Utility.List(new List<T>(enumerable));

        List<List<T>> permutations= new List<List<T>>();

        foreach(T element in enumerable)
        {
            List<T> short_list = new List<T>(enumerable);
            short_list.Remove(element);

            List<List<T>> short_permutations = Permute(short_list);

            foreach(List<T> permutation in short_permutations)
            {
                permutation.Add(element);
                permutations.Add(permutation);
            }
        }

        return permutations;
    }

    public static List<List<T>> Choose<T>(List<List<T>> options)
    {
        if (options.Count == 0)
            return Utility.List(new List<T>());

        List<List<T>> remaining_options = new List<List<T>>(options);
        List<T> option = remaining_options.TakeAt(0);

        List<List<T>> choice_sets = new List<List<T>>();
        foreach (T choice in option)
        {
            List<List<T>> new_paths = Choose(remaining_options);
            Utility.ForEach(new_paths, delegate (List<T> path) { path.Insert(0, choice); });

            choice_sets.AddRange(new_paths);
        }

        return choice_sets;
    }

    static List<int> primes = new List<int> { 2, 3, 5, 7, 11, 13, 17, 19 };
    public static List<int> GetPrimeFactors(int number)
    {
        List<int> prime_factors = new List<int>();

        foreach (int prime in primes)
        {
            while(number % prime == 0)
            {
                number /= prime;
                prime_factors.Add(prime);
            }
        }


        int guess = primes[primes.Count - 1] + 2;
        while (number != 1 && guess <= Mathf.Sqrt(number))
        {
            if (number % guess == 0)
            {
                primes.Add(guess);

                number /= guess;
                prime_factors.Add(guess);
            }

            guess += 2;
        }

        if (number != 1)
        {
            primes.Add(number);

            prime_factors.Add(number);
        }

        return prime_factors;
    }

    //Rotation measured from vector (1, 0) counterclockwise
    public static float GetRotation(Vector2 vector)
    {
        int quadrant;
        if (vector.x > 0)
        {
            if (vector.y > 0)
                quadrant = 0;
            else
                quadrant = 3;
        }
        else
        {
            if (vector.y > 0)
                quadrant = 1;
            else
                quadrant = 2;
        }

        float pi_over_2 = Mathf.PI / 2;
        float asin_y = Mathf.Abs(Mathf.Asin(vector.normalized.y));

        return quadrant * pi_over_2 + ((quadrant % 2 == 0) ? asin_y : pi_over_2 - asin_y);
    }

    public static int Pow(int base_, int exponent)
    {
        int result = 1;

        for (int i = 0; i < exponent; i++)
            result *= base_;

        return result;
    }

    public static int RoundDown(this float value)
    {
        if (value >= 0)
            return (int)value;
        else
            return (int)(value - 1);
    }

    public static int Round(this float value)
    {
        return RoundDown(value + 0.5f);
    }

    public static Vector2 MakeUniformScale(float scale)
    {
        return new Vector2(scale, scale);
    }

    //http://en.wikipedia.org/wiki/Halton_sequence
    public static float HaltonSequence(int base_, int i)
    {
        float result = 0;
        float f = 1.0f;
        while (i > 0)
        {
            f = f / base_;
            result += f * (i % base_);
            i = i / base_;
        }

        return result;
    }

    public static float RadiansToDegrees(float radians)
    {
        return 360 * radians / (2 * Mathf.PI);
    }

    public static float DegreesToRadians(float degrees)
    {
        return (2 * Mathf.PI) * degrees / 360;
    }

    //This always returns a value between 0 and divisor - 1
    public static int Mod(int dividend, int divisor)
    {
        while (dividend < 0)
            dividend += divisor;

        return dividend % divisor;
    }

    public static float Gaussian(float x, float standard_deviation = 1)
    {
        return Mathf.Pow((float)System.Math.E, -0.5f * Mathf.Pow(x / standard_deviation, 2)) / 
               (standard_deviation * Mathf.Sqrt(2 * Mathf.PI));
    }

    static float GaussianTailHeight = Gaussian(-3);
    static float GaussianPeakHeight = Gaussian(0);
    public static float GetSmoothedLerpFactor(float factor)
    {
        factor = Mathf.Max(0, Mathf.Min(1, factor));

        return (Gaussian(factor * 3 - 3) - GaussianTailHeight) /
               (GaussianPeakHeight - GaussianTailHeight);
    }

    public static float SmoothLerp(float a, float b, float factor)
    {
        return Mathf.Lerp(a, b, GetSmoothedLerpFactor(factor));
    }

    public static IEnumerable<float> GetFloatRange(int sample_count, float scale = 1, float bias = 0)
    {
        return Enumerable.Range(0, sample_count)
            .Select(value => (value / (sample_count - 1.0f) * scale + bias));
    }

    public static float Distance(this Vector3 point, Line line)
    {
        Vector3 displacement = line.Point - point;
        Vector3 perpendicular_direction = displacement.Crossed(line.Direction)
                                                      .Crossed(line.Direction)
                                                      .normalized;

        return Mathf.Abs(perpendicular_direction.Dot(displacement));
    }

    public static float Distance(this Vector3 point, LineSegment line_segment)
    {
        Vector3 direction = line_segment.P1 - line_segment.P0;
        float t = (point - line_segment.P0).Dot(direction.normalized);

        if (t <= 0 || t >= direction.magnitude)
            return Mathf.Min(point.Distance(line_segment.P0), point.Distance(line_segment.P1));
        else
            return Distance(point, new Line(line_segment.P0, direction));
    }

    public static Sphere GetBoundingSphere(IEnumerable<Vector3> points)
    {
        return GetBoundingSphere(points.Select(point => new Sphere(point, 0)));
    }

    public static Sphere GetBoundingSphere(IEnumerable<Sphere> spheres)
    {
        spheres = spheres.Distinct();

        Sphere bounding_sphere = new Sphere();

        foreach (Sphere sphere in spheres)
        {
            Dictionary<Sphere, float> radii = spheres.ToDictionary(
                other_sphere => other_sphere, 
                other_sphere => ((sphere.Point - other_sphere.Point).magnitude +
                                (sphere.Radius + other_sphere.Radius)) / 2);

            Sphere farthest_sphere = radii.Keys.MaxElement(other_sphere => radii[other_sphere]);
            float radius = radii[farthest_sphere];

            if (radius> bounding_sphere.Radius)
            {
                bounding_sphere.Radius = radius;

                Vector3 direction = (farthest_sphere.Point - sphere.Point).normalized;
                bounding_sphere.Point = sphere.Point + direction * (radius - sphere.Radius);
            }
        }

        return bounding_sphere;
    }

    public static bool IsColliding(this Sphere a, Sphere b)
    {
        return a.Point.Distance(b.Point) < (a.Radius + b.Radius);
    }

    public static float Root(System.Func<float, float> function,
                             System.Func<float, float> derivative,
                             float error = 1e-6f,
                             float guess = 0)
    {
        float x = guess;

        float dx = 0;
        int iterations = 0;

        do
        {
            dx = function(x) / derivative(x);
            x -= dx;

            iterations++;

        } while (Mathf.Abs(dx / x) > error && 
                 iterations < 100);

        return x;
    }

    public static float EllipseCircumference(float semimajor_axis, float eccentricity, float error = -1)
    {
        if (error < 0)
            error = semimajor_axis / 1000000;

        float factor = 2 * Mathf.PI * semimajor_axis;


        float sum = 1;

        int i = 0;
        float numerator_product = 1;
        float denominator_product = 1;
        float next_term;

        do
        {
            numerator_product *= (1 + i * 2);
            denominator_product *= (i + 1) * 2;

            next_term =
                Mathf.Pow(numerator_product / denominator_product, 2) *
                Mathf.Pow(eccentricity, (i + 1) * 2) /
                (1 + i * 2);

            sum -= next_term;
            i++;

        } while ((next_term * factor) > error && i < 100);

        return sum * factor;
    }
}

public abstract class GenericFunction<T>
{
    public abstract float Compute(T x);
}

public abstract class Function : GenericFunction<float>
{
    public float Integrate(float x0, float x1)
    {
        float total = 0;

        int sample_count = 100;
        float width = (x1 - x0) / sample_count;

        for (int i = 0; i < sample_count; i++)
            total += Compute(Mathf.Lerp(x0, x1, i / (float)sample_count) + (width / 2)) * width;

        return total;
    } 
}

public abstract class ProbabilityDistribution : Function
{
    public abstract float Minimum
    {
        get;
    }

    public abstract float Maximum
    {
        get;
    }

    public float Range
    {
        get { return Maximum - Minimum; }
    }

    public float Median
    {
        get { return InverseCDF(0.5f); }
    }

    float InverseCDF(float percentile, float test_sample, int iteration)
    {
        float test_percentile = CDF(test_sample);

        if (iteration < 10)
        {
            if (percentile > test_percentile)
                return InverseCDF(percentile, test_sample + Mathf.Pow(0.5f, iteration + 2) * Range, iteration + 1);
            else
                return InverseCDF(percentile, test_sample - Mathf.Pow(0.5f, iteration + 2) * Range, iteration + 1);
        }

        return test_sample;
    }

    public float InverseCDF(float percentile)
    {
        return InverseCDF(percentile, Minimum + Range / 2, 0);
    }

    public float CDF(float x)
    {
        x = Mathf.Clamp(x, Minimum, Maximum);

        return Integrate(Minimum, x) / Integrate(Minimum, Maximum);
    }

    public float GetSample(float percentile)
    {
        return InverseCDF(percentile);
    }

    public float GetRandomSample()
    {
        return InverseCDF(Random.value);
    }
} 

public class UniformDistribution : ProbabilityDistribution
{
    public override float Minimum
    {
        get { return 0.0f; }
    }

    public override float Maximum
    {
        get { return 1.0f; }
    }

    public override float Compute(float x)
    {
        return x;
    }
}

public class NormalDistribution : ProbabilityDistribution
{
    protected float mean, standard_deviation;

    public override float Minimum
    {
        get { return mean - MaximumDeviation; }
    }

    public override float Maximum
    {
        get { return mean + MaximumDeviation; }
    }

    protected float MaximumDeviation
    {
        get { return standard_deviation * 3; }
    }

    public NormalDistribution(float mean_, float maximum_deviation)
    {
        mean = mean_;
        standard_deviation = maximum_deviation / 3;
    }

    public override float Compute(float x)
    {
        if (x < Minimum || x > Maximum)
            return 0;

        float sample = Mathf.Pow
                       (
                            (float)System.Math.E,
                            -Mathf.Pow(x - mean, 2) / (2 * Mathf.Pow(standard_deviation, 2))
                       )
                      / standard_deviation
                      / Mathf.Sqrt(2 * Mathf.PI);

        return sample;
    }
}

//This needs a separate skew value for bottom half and top half of samples
public class SkewedNormalDistribution : ProbabilityDistribution
{
    NormalDistribution normal_distribtion;
    float base_mean;
    float skew;

    public override float Minimum
    {
        get
        {
            if(skew < 1)
                return base_mean + (normal_distribtion.Minimum - base_mean) / skew;
            else
                return normal_distribtion.Minimum;
        }
    }

    public override float Maximum
    {
        get
        {
            if (skew >= 1)
                return base_mean + (normal_distribtion.Maximum - base_mean) * skew;
            else
                return normal_distribtion.Maximum;
        }
    }

    public SkewedNormalDistribution(float mean, float range, float skew_)
    {
        normal_distribtion = new NormalDistribution(mean, range);
        base_mean = mean;

        skew = skew_;
    }

    public override float Compute(float x)
    {
        if(skew < 1)
            return normal_distribtion.Compute(x > base_mean ? x : base_mean + ((x - base_mean) * skew));
        else
            return normal_distribtion.Compute(x < base_mean ? x : base_mean + ((x - base_mean) / skew));
    }
}

public class ChoiceFunction : ProbabilityDistribution
{
    float probability;

    public ChoiceFunction(float probability_)
    {
        probability = Mathf.Clamp(probability_, 0, 1);
    }

    public override float Minimum { get { return -1; } }
    public override float Maximum { get { return 1; } }

    public override float Compute(float x)
    {
        if (x < 0)
            return 1 - probability;
        else
            return probability;
    }
}

public struct Line
{
    public Vector3 Point;
    public Vector3 Direction;

    public Line(Vector3 point, Vector3 direction)
    {
        Point = point;
        Direction = direction;
    }
}

public struct LineSegment
{
    public Vector3 P0;
    public Vector3 P1;

    public LineSegment(Vector3 p0, Vector3 p1)
    {
        P0 = p0;
        P1 = p1;
    }
}

public struct Sphere
{
    public Vector3 Point;
    public float Radius;

    public Sphere(Vector3 point, float radius)
    {
        Point = point;
        Radius = radius;
    }
}
