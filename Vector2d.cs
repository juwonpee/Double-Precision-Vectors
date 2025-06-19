using System;
using System.Runtime.CompilerServices;
using System.Globalization;
using UnityEngine;

namespace DoublePrecision {

	[Serializable] public struct Vector2d {
		
        // *Undocumented*
        public const double kEpsilon = 4.94065645841247E-324;
        // *Undocumented*
        public const double kEpsilonNormalSqrt = 1e-15F;
		public double x, y;

		#region Constructors
		public Vector2d (double x) { this.x = x; this.y = 0;}
		public Vector2d (double x, double y) { this.x = x; this.y = y; }
		public Vector2d (double x, double y, double z) { this.x = x; this.y = y; }
		public Vector2d (Vector3 a) { x = a.x; y = a.y; }

		#endregion

		#region Methods

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(double newX, double newY, double newZ) { x = newX; y = newY; }
		

        // Multiplies every component of this vector by the same component of /scale/.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Scale(Vector2d scale) { x *= scale.x; y *= scale.y; }

		
		// used to allow Vector3s to be used as keys in hash tables
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() { return x.GetHashCode() ^ (y.GetHashCode() << 2); }

		// also required for being able to use Vector3s as keys in hash tables
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object other)
        {
            if (other is Vector2d v)
                return Equals(v);
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector2d other)
        {
            return x == other.x && y == other.y;
        }

		#endregion

		#region Static Functions

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2d Lerp(Vector2d a, Vector2d b, double t) {
			t = Math.Clamp(t, 0, 1);
			
            return new Vector2d(
                a.x + (b.x - a.x) * t,
                a.y + (b.y - a.y) * t
            );
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2d LerpUnclamped(Vector2d a, Vector2d b, double t)
        {
            return new Vector2d(
                a.x + (b.x - a.x) * t,
                a.y + (b.y - a.y) * t
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2d MoveTowards(Vector2d current, Vector2d target, double maxDistanceDelta)
        {
            // avoid vector ops because current scripting backends are terrible at inlining
            double toVector_x = target.x - current.x;
            double toVector_y = target.y - current.y;

			double sqdist = toVector_x * toVector_x + toVector_y * toVector_y;

            if (sqdist == 0 || (maxDistanceDelta >= 0 && sqdist <= maxDistanceDelta * maxDistanceDelta))
                return target;
            var dist = Math.Sqrt(sqdist);

            return new Vector2d(
				current.x + toVector_x / dist * maxDistanceDelta,
                current.y + toVector_y / dist * maxDistanceDelta
				);
        }

		 // Gradually changes a vector towards a desired goal over time.
        public static Vector2d SmoothDamp(Vector2d current, Vector2d target, ref Vector2d currentVelocity, double smoothTime, double maxSpeed, double deltaTime)
        {
            double output_x = 0f;
            double output_y = 0f;

            // Based on Game Programming Gems 4 Chapter 1.10
            smoothTime = Math.Max(0.0001F, smoothTime);
            double omega = 2F / smoothTime;

            double x = omega * deltaTime;
            double exp = 1F / (1F + x + 0.48F * x * x + 0.235F * x * x * x);

            double change_x = current.x - target.x;
            double change_y = current.y - target.y;
            Vector2d originalTo = target;

            // Clamp maximum speed
            double maxChange = maxSpeed * smoothTime;

            double maxChangeSq = maxChange * maxChange;
			double sqrmag = change_x * change_x + change_y * change_y;
            if (sqrmag > maxChangeSq)
            {
                var mag = Math.Sqrt(sqrmag);
                change_x = change_x / mag * maxChange;
                change_y = change_y / mag * maxChange;
            }

            target.x = current.x - change_x;
            target.y = current.y - change_y;

            double temp_x = (currentVelocity.x + omega * change_x) * deltaTime;
            double temp_y = (currentVelocity.y + omega * change_y) * deltaTime;

            currentVelocity.x = (currentVelocity.x - omega * temp_x) * exp;
            currentVelocity.y = (currentVelocity.y - omega * temp_y) * exp;

            output_x = target.x + (change_x + temp_x) * exp;
            output_y = target.y + (change_y + temp_y) * exp;

            // Prevent overshooting
            double origMinusCurrent_x = originalTo.x - current.x;
            double origMinusCurrent_y = originalTo.y - current.y;
            double outMinusOrig_x = output_x - originalTo.x;
            double outMinusOrig_y = output_y - originalTo.y;

            if (origMinusCurrent_x * outMinusOrig_x + origMinusCurrent_y * outMinusOrig_y > 0)
            {
                output_x = originalTo.x;
                output_y = originalTo.y;

                currentVelocity.x = (output_x - originalTo.x) / deltaTime;
                currentVelocity.y = (output_y - originalTo.y) / deltaTime;
            }

            return new Vector2d(output_x, output_y);
        }

		
		public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return x;
                    case 1: return y;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector2d index!");
                }
            }

            set
            {
                switch (index)
                {
                    case 0: x = value; break;
                    case 1: y = value; break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector2d index!");
                }
            }
        }



        // Multiplies two vectors component-wise.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2d Scale(Vector2d a, Vector2d b) { return new Vector2d(a.x * b.x, a.y * b.y); }
		
        // Reflects a vector off the plane defined by a normal.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2d Reflect(Vector2d inDirection, Vector2d inNormal)
        {
            double factor = -2F * Dot(inNormal, inDirection);
            return new Vector2d(factor * inNormal.x + inDirection.x,
                factor * inNormal.y + inDirection.y);
        }

        // *undoc* --- we have normalized property now
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2d Normalize(Vector2d value)
        {
            double mag = Magnitude(value);
            if (mag > kEpsilon)
                return value / mag;
            else
                return zero;
        }

        // Makes this vector have a ::ref::magnitude of 1.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            double mag = Magnitude(this);
            if (mag > kEpsilon)
                this = this / mag;
            else
                this = zero;
        }

        // Returns this vector with a ::ref::magnitude of 1 (RO).
        public Vector2d normalized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Vector2d.Normalize(this); }
        }

        // Dot Product of two vectors.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Dot(Vector2d lhs, Vector2d rhs) { return lhs.x * rhs.x + lhs.y * rhs.y; }

        // Projects a vector onto another vector.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2d Project(Vector2d vector, Vector2d onNormal)
        {
            double sqrMag = Dot(onNormal, onNormal);
            if (sqrMag < kEpsilon)
                return zero;
            else
            {
                double dot = Dot(vector, onNormal);
                return new Vector2d(
					onNormal.x * dot / sqrMag,
                    onNormal.y * dot / sqrMag);
            }
        }


        // Returns the angle in degrees between /from/ and /to/. This is always the smallest
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Angle(Vector2d from, Vector2d to)
        {
            // sqrt(a) * sqrt(b) = sqrt(a * b) -- valid for real numbers
            double denominator = Math.Sqrt(from.sqrMagnitude * to.sqrMagnitude);
            if (denominator < kEpsilonNormalSqrt)
                return 0d;

			double dot = Math.Clamp(Dot(from, to) / denominator, -1d, 1d);
            return Math.Acos(dot) * Mathd.Rad2Deg;
        }

        // // The smaller of the two possible angles between the two vectors is returned, therefore the result will never be greater than 180 degrees or smaller than -180 degrees.
        // // If you imagine the from and to vectors as lines on a piece of paper, both originating from the same point, then the /axis/ vector would point up out of the paper.
        // // The measured angle between the two vectors would be positive in a clockwise direction and negative in an anti-clockwise direction.
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // public static double SignedAngle(Vector2d from, Vector2d to, Vector2d axis)
        // {
        //     double unsignedAngle = Angle(from, to);

        //     double cross_x = from.y * to.z - from.z * to.y;
        //     double cross_y = from.z * to.x - from.x * to.z;
        //     double cross_z = from.x * to.y - from.y * to.x;
        //     double sign = Mathd.Sign(axis.x * cross_x + axis.y * cross_y + axis.z * cross_z);
        //     return unsignedAngle * sign;
        // }

        // Returns the distance between /a/ and /b/.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Distance(Vector2d a, Vector2d b)
        {
            double diff_x = a.x - b.x;
            double diff_y = a.y - b.y;
            return (double)Math.Sqrt(diff_x * diff_x + diff_y * diff_y);
        }

        // Returns a copy of /vector/ with its magnitude clamped to /maxLength/.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2d ClampMagnitude(Vector2d vector, double maxLength)
        {
            double sqrmag = vector.sqrMagnitude;
            if (sqrmag > maxLength * maxLength)
            {
                double mag = (double)Math.Sqrt(sqrmag);
                //these intermediate variables force the intermediate result to be
                //of double precision. without this, the intermediate result can be of higher
                //precision, which changes behavior.
                double normalized_x = vector.x / mag;
                double normalized_y = vector.y / mag;
                return new Vector2d(normalized_x * maxLength,
                    normalized_y * maxLength);
            }
            return vector;
        }

        // *undoc* --- there's a property now
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Magnitude(Vector2d vector) { return (double)Math.Sqrt(vector.x * vector.x + vector.y * vector.y); }

        // Returns the length of this vector (RO).
        public double magnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (double)Math.Sqrt(x * x + y * y); }
        }

        // *undoc* --- there's a property now
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double SqrMagnitude(Vector2d vector) { return vector.x * vector.x + vector.y * vector.y; }

        // Returns the squared length of this vector (RO).
        public double sqrMagnitude { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return x * x + y * y; } }

        // Returns a vector that is made from the smallest components of two vectors.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2d Min(Vector2d lhs, Vector2d rhs)
        {
            return new Vector2d(Mathd.Min(lhs.x, rhs.x), Mathd.Min(lhs.y, rhs.y));
        }

        // Returns a vector that is made from the largest components of two vectors.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2d Max(Vector2d lhs, Vector2d rhs)
        {
            return new Vector2d(Mathd.Max(lhs.x, rhs.x), Mathd.Max(lhs.y, rhs.y));
        }

        static readonly Vector2d zeroVector = new Vector2d(0F, 0F);
        static readonly Vector2d oneVector = new Vector2d(1F, 1F);
        static readonly Vector2d upVector = new Vector2d(0F, 1F);
        static readonly Vector2d downVector = new Vector2d(0F, -1F);
        static readonly Vector2d leftVector = new Vector2d(-1F, 0F);
        static readonly Vector2d rightVector = new Vector2d(1F, 0F);
        static readonly Vector2d positiveInfinityVector = new Vector2d(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
        static readonly Vector2d negativeInfinityVector = new Vector2d(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);

        // Shorthand for writing @@Vector2d(0, 0, 0)@@
        public static Vector2d zero { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return zeroVector; } }
        // Shorthand for writing @@Vector2d(1, 1, 1)@@
        public static Vector2d one { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return oneVector; } }
        // Shorthand for writing @@Vector2d(0, 1, 0)@@
        public static Vector2d up { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return upVector; } }
        public static Vector2d down { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return downVector; } }
        public static Vector2d left { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return leftVector; } }
        // Shorthand for writing @@Vector2d(1, 0, 0)@@
        public static Vector2d right { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return rightVector; } }
        // Shorthand for writing @@Vector2d(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity)@@
        public static Vector2d positiveInfinity { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return positiveInfinityVector; } }
        // Shorthand for writing @@Vector2d(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity)@@
        public static Vector2d negativeInfinity { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return negativeInfinityVector; } }

        // Adds two vectors.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2d operator+(Vector2d a, Vector2d b) { return new Vector2d(a.x + b.x, a.y + b.y); }
        // Subtracts one vector from another.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2d operator-(Vector2d a, Vector2d b) { return new Vector2d(a.x - b.x, a.y - b.y); }
        // Negates a vector.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2d operator-(Vector2d a) { return new Vector2d(-a.x, -a.y); }
        // Multiplies a vector by a number.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2d operator*(Vector2d a, double d) { return new Vector2d(a.x * d, a.y * d); }
        // Multiplies a vector by a number.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2d operator*(double d, Vector2d a) { return new Vector2d(a.x * d, a.y * d); }
        // Divides a vector by a number.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2d operator/(Vector2d a, double d) { return new Vector2d(a.x / d, a.y / d); }

        // Returns true if the vectors are equal.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator==(Vector2d lhs, Vector2d rhs)
        {
            // Returns false in the presence of NaN values.
            double diff_x = lhs.x - rhs.x;
            double diff_y = lhs.y - rhs.y;
			double sqrmag = diff_x * diff_x + diff_y * diff_y;
            return sqrmag < kEpsilon * kEpsilon;
        }

        // Returns true if vectors are different.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator!=(Vector2d lhs, Vector2d rhs)
        {
            // Returns true in the presence of NaN values.
            return !(lhs == rhs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return ToString(null, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string format)
        {
            return ToString(format, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
                format = "F2";
            if (formatProvider == null)
                formatProvider = CultureInfo.InvariantCulture.NumberFormat;
            return string.Format("({0}, {1}, {2})", x.ToString(format, formatProvider), y.ToString(format, formatProvider));
        }









		#endregion

	}
}
