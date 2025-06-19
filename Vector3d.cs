using System;
using System.Runtime.CompilerServices;
using System.Globalization;
using UnityEngine;

namespace DoublePrecision {

	[Serializable] public struct Vector3d {
		
        // *Undocumented*
        public const double kEpsilon = 4.94065645841247E-324;
        // *Undocumented*
        public const double kEpsilonNormalSqrt = 1e-15F;
		public double x,y,z;

		#region Constructors
		public Vector3d (double x) { this.x = x; this.y = this.z = 0; }
		public Vector3d (double x, double y) { this.x = x; this.y = y; this.z = 0; }
		public Vector3d (double x, double y, double z) { this.x = x; this.y = y; this.z = z; }
		public Vector3d (Vector3 a) { x = a.x; y = a.y; z = a.z;  }

		#endregion

		#region Methods

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(double newX, double newY, double newZ) { x = newX; y = newY; z = newZ; }
		

        // Multiplies every component of this vector by the same component of /scale/.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Scale(Vector3d scale) { x *= scale.x; y *= scale.y; z *= scale.z; }

		
		// used to allow Vector3s to be used as keys in hash tables
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() { return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2); }

		// also required for being able to use Vector3s as keys in hash tables
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object other)
        {
            if (other is Vector3d v)
                return Equals(v);
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector3d other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

		#endregion

		#region Static Functions

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3d Lerp(Vector3d a, Vector3d b, double t) {
			t = Math.Clamp(t, 0, 1);
			
            return new Vector3d(
                a.x + (b.x - a.x) * t,
                a.y + (b.y - a.y) * t,
                a.z + (b.z - a.z) * t
            );
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3d LerpUnclamped(Vector3d a, Vector3d b, double t)
        {
            return new Vector3d(
                a.x + (b.x - a.x) * t,
                a.y + (b.y - a.y) * t,
                a.z + (b.z - a.z) * t
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3d MoveTowards(Vector3d current, Vector3d target, double maxDistanceDelta)
        {
            // avoid vector ops because current scripting backends are terrible at inlining
            double toVector_x = target.x - current.x;
            double toVector_y = target.y - current.y;
            double toVector_z = target.z - current.z;

            double sqdist = toVector_x * toVector_x + toVector_y * toVector_y + toVector_z * toVector_z;

            if (sqdist == 0 || (maxDistanceDelta >= 0 && sqdist <= maxDistanceDelta * maxDistanceDelta))
                return target;
            var dist = Math.Sqrt(sqdist);

            return new Vector3d(current.x + toVector_x / dist * maxDistanceDelta,
                current.y + toVector_y / dist * maxDistanceDelta,
                current.z + toVector_z / dist * maxDistanceDelta);
        }

		 // Gradually changes a vector towards a desired goal over time.
        public static Vector3d SmoothDamp(Vector3d current, Vector3d target, ref Vector3d currentVelocity, double smoothTime, double maxSpeed, double deltaTime)
        {
            double output_x = 0f;
            double output_y = 0f;
            double output_z = 0f;

            // Based on Game Programming Gems 4 Chapter 1.10
            smoothTime = Math.Max(0.0001F, smoothTime);
            double omega = 2F / smoothTime;

            double x = omega * deltaTime;
            double exp = 1F / (1F + x + 0.48F * x * x + 0.235F * x * x * x);

            double change_x = current.x - target.x;
            double change_y = current.y - target.y;
            double change_z = current.z - target.z;
            Vector3d originalTo = target;

            // Clamp maximum speed
            double maxChange = maxSpeed * smoothTime;

            double maxChangeSq = maxChange * maxChange;
            double sqrmag = change_x * change_x + change_y * change_y + change_z * change_z;
            if (sqrmag > maxChangeSq)
            {
                var mag = Math.Sqrt(sqrmag);
                change_x = change_x / mag * maxChange;
                change_y = change_y / mag * maxChange;
                change_z = change_z / mag * maxChange;
            }

            target.x = current.x - change_x;
            target.y = current.y - change_y;
            target.z = current.z - change_z;

            double temp_x = (currentVelocity.x + omega * change_x) * deltaTime;
            double temp_y = (currentVelocity.y + omega * change_y) * deltaTime;
            double temp_z = (currentVelocity.z + omega * change_z) * deltaTime;

            currentVelocity.x = (currentVelocity.x - omega * temp_x) * exp;
            currentVelocity.y = (currentVelocity.y - omega * temp_y) * exp;
            currentVelocity.z = (currentVelocity.z - omega * temp_z) * exp;

            output_x = target.x + (change_x + temp_x) * exp;
            output_y = target.y + (change_y + temp_y) * exp;
            output_z = target.z + (change_z + temp_z) * exp;

            // Prevent overshooting
            double origMinusCurrent_x = originalTo.x - current.x;
            double origMinusCurrent_y = originalTo.y - current.y;
            double origMinusCurrent_z = originalTo.z - current.z;
            double outMinusOrig_x = output_x - originalTo.x;
            double outMinusOrig_y = output_y - originalTo.y;
            double outMinusOrig_z = output_z - originalTo.z;

            if (origMinusCurrent_x * outMinusOrig_x + origMinusCurrent_y * outMinusOrig_y + origMinusCurrent_z * outMinusOrig_z > 0)
            {
                output_x = originalTo.x;
                output_y = originalTo.y;
                output_z = originalTo.z;

                currentVelocity.x = (output_x - originalTo.x) / deltaTime;
                currentVelocity.y = (output_y - originalTo.y) / deltaTime;
                currentVelocity.z = (output_z - originalTo.z) / deltaTime;
            }

            return new Vector3d(output_x, output_y, output_z);
        }

		
		public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return x;
                    case 1: return y;
                    case 2: return z;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3d index!");
                }
            }

            set
            {
                switch (index)
                {
                    case 0: x = value; break;
                    case 1: y = value; break;
                    case 2: z = value; break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3d index!");
                }
            }
        }



        // Multiplies two vectors component-wise.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d Scale(Vector3d a, Vector3d b) { return new Vector3d(a.x * b.x, a.y * b.y, a.z * b.z); }
		
        // Cross Product of two vectors.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d Cross(Vector3d lhs, Vector3d rhs)
        {
            return new Vector3d(
                lhs.y * rhs.z - lhs.z * rhs.y,
                lhs.z * rhs.x - lhs.x * rhs.z,
                lhs.x * rhs.y - lhs.y * rhs.x);
        }

        // Reflects a vector off the plane defined by a normal.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d Reflect(Vector3d inDirection, Vector3d inNormal)
        {
            double factor = -2F * Dot(inNormal, inDirection);
            return new Vector3d(factor * inNormal.x + inDirection.x,
                factor * inNormal.y + inDirection.y,
                factor * inNormal.z + inDirection.z);
        }

        // *undoc* --- we have normalized property now
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d Normalize(Vector3d value)
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
        public Vector3d normalized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Vector3d.Normalize(this); }
        }

        // Dot Product of two vectors.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Dot(Vector3d lhs, Vector3d rhs) { return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z; }

        // Projects a vector onto another vector.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d Project(Vector3d vector, Vector3d onNormal)
        {
            double sqrMag = Dot(onNormal, onNormal);
            if (sqrMag < kEpsilon)
                return zero;
            else
            {
                double dot = Dot(vector, onNormal);
                return new Vector3d(onNormal.x * dot / sqrMag,
                    onNormal.y * dot / sqrMag,
                    onNormal.z * dot / sqrMag);
            }
        }

        // Projects a vector onto a plane defined by a normal orthogonal to the plane.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d ProjectOnPlane(Vector3d vector, Vector3d planeNormal)
        {
            double sqrMag = Dot(planeNormal, planeNormal);
            if (sqrMag < kEpsilon)
                return vector;
            else
            {
            	double dot = Dot(vector, planeNormal);
                return new Vector3d(vector.x - planeNormal.x * dot / sqrMag,
                    vector.y - planeNormal.y * dot / sqrMag,
                    vector.z - planeNormal.z * dot / sqrMag);
            }
        }

        // Returns the angle in degrees between /from/ and /to/. This is always the smallest
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Angle(Vector3d from, Vector3d to)
        {
            // sqrt(a) * sqrt(b) = sqrt(a * b) -- valid for real numbers
            double denominator = Math.Sqrt(from.sqrMagnitude * to.sqrMagnitude);
            if (denominator < kEpsilonNormalSqrt)
                return 0d;

			double dot = Math.Clamp(Dot(from, to) / denominator, -1d, 1d);
            return Math.Acos(dot) * Mathd.Rad2Deg;
        }

        // The smaller of the two possible angles between the two vectors is returned, therefore the result will never be greater than 180 degrees or smaller than -180 degrees.
        // If you imagine the from and to vectors as lines on a piece of paper, both originating from the same point, then the /axis/ vector would point up out of the paper.
        // The measured angle between the two vectors would be positive in a clockwise direction and negative in an anti-clockwise direction.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double SignedAngle(Vector3d from, Vector3d to, Vector3d axis)
        {
            double unsignedAngle = Angle(from, to);

            double cross_x = from.y * to.z - from.z * to.y;
            double cross_y = from.z * to.x - from.x * to.z;
            double cross_z = from.x * to.y - from.y * to.x;
            double sign = Mathd.Sign(axis.x * cross_x + axis.y * cross_y + axis.z * cross_z);
            return unsignedAngle * sign;
        }

        // Returns the distance between /a/ and /b/.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Distance(Vector3d a, Vector3d b)
        {
            double diff_x = a.x - b.x;
            double diff_y = a.y - b.y;
            double diff_z = a.z - b.z;
            return (double)Math.Sqrt(diff_x * diff_x + diff_y * diff_y + diff_z * diff_z);
        }

        // Returns a copy of /vector/ with its magnitude clamped to /maxLength/.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d ClampMagnitude(Vector3d vector, double maxLength)
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
                double normalized_z = vector.z / mag;
                return new Vector3d(normalized_x * maxLength,
                    normalized_y * maxLength,
                    normalized_z * maxLength);
            }
            return vector;
        }

        // *undoc* --- there's a property now
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Magnitude(Vector3d vector) { return (double)Math.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z); }

        // Returns the length of this vector (RO).
        public double magnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (double)Math.Sqrt(x * x + y * y + z * z); }
        }

        // *undoc* --- there's a property now
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double SqrMagnitude(Vector3d vector) { return vector.x * vector.x + vector.y * vector.y + vector.z * vector.z; }

        // Returns the squared length of this vector (RO).
        public double sqrMagnitude { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return x * x + y * y + z * z; } }

        // Returns a vector that is made from the smallest components of two vectors.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d Min(Vector3d lhs, Vector3d rhs)
        {
            return new Vector3d(Mathd.Min(lhs.x, rhs.x), Mathd.Min(lhs.y, rhs.y), Mathd.Min(lhs.z, rhs.z));
        }

        // Returns a vector that is made from the largest components of two vectors.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d Max(Vector3d lhs, Vector3d rhs)
        {
            return new Vector3d(Mathd.Max(lhs.x, rhs.x), Mathd.Max(lhs.y, rhs.y), Mathd.Max(lhs.z, rhs.z));
        }

        static readonly Vector3d zeroVector = new Vector3d(0F, 0F, 0F);
        static readonly Vector3d oneVector = new Vector3d(1F, 1F, 1F);
        static readonly Vector3d upVector = new Vector3d(0F, 1F, 0F);
        static readonly Vector3d downVector = new Vector3d(0F, -1F, 0F);
        static readonly Vector3d leftVector = new Vector3d(-1F, 0F, 0F);
        static readonly Vector3d rightVector = new Vector3d(1F, 0F, 0F);
        static readonly Vector3d forwardVector = new Vector3d(0F, 0F, 1F);
        static readonly Vector3d backVector = new Vector3d(0F, 0F, -1F);
        static readonly Vector3d positiveInfinityVector = new Vector3d(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
        static readonly Vector3d negativeInfinityVector = new Vector3d(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);

        // Shorthand for writing @@Vector3d(0, 0, 0)@@
        public static Vector3d zero { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return zeroVector; } }
        // Shorthand for writing @@Vector3d(1, 1, 1)@@
        public static Vector3d one { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return oneVector; } }
        // Shorthand for writing @@Vector3d(0, 0, 1)@@
        public static Vector3d forward { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return forwardVector; } }
        public static Vector3d back { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return backVector; } }
        // Shorthand for writing @@Vector3d(0, 1, 0)@@
        public static Vector3d up { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return upVector; } }
        public static Vector3d down { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return downVector; } }
        public static Vector3d left { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return leftVector; } }
        // Shorthand for writing @@Vector3d(1, 0, 0)@@
        public static Vector3d right { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return rightVector; } }
        // Shorthand for writing @@Vector3d(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity)@@
        public static Vector3d positiveInfinity { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return positiveInfinityVector; } }
        // Shorthand for writing @@Vector3d(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity)@@
        public static Vector3d negativeInfinity { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return negativeInfinityVector; } }

        // Adds two vectors.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d operator+(Vector3d a, Vector3d b) { return new Vector3d(a.x + b.x, a.y + b.y, a.z + b.z); }
        // Subtracts one vector from another.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d operator-(Vector3d a, Vector3d b) { return new Vector3d(a.x - b.x, a.y - b.y, a.z - b.z); }
        // Negates a vector.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d operator-(Vector3d a) { return new Vector3d(-a.x, -a.y, -a.z); }
        // Multiplies a vector by a number.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d operator*(Vector3d a, double d) { return new Vector3d(a.x * d, a.y * d, a.z * d); }
        // Multiplies a vector by a number.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d operator*(double d, Vector3d a) { return new Vector3d(a.x * d, a.y * d, a.z * d); }
        // Divides a vector by a number.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d operator/(Vector3d a, double d) { return new Vector3d(a.x / d, a.y / d, a.z / d); }

        // Returns true if the vectors are equal.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator==(Vector3d lhs, Vector3d rhs)
        {
            // Returns false in the presence of NaN values.
            double diff_x = lhs.x - rhs.x;
            double diff_y = lhs.y - rhs.y;
            double diff_z = lhs.z - rhs.z;
            double sqrmag = diff_x * diff_x + diff_y * diff_y + diff_z * diff_z;
            return sqrmag < kEpsilon * kEpsilon;
        }

        // Returns true if vectors are different.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator!=(Vector3d lhs, Vector3d rhs)
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
            return string.Format("({0}, {1}, {2})", x.ToString(format, formatProvider), y.ToString(format, formatProvider), z.ToString(format, formatProvider));
        }









		#endregion

	}
}
