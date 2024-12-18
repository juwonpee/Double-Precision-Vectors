using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Bindings;
using UnityEngine.Internal;
using UnityEngine.Scripting;
using Unity.IL2CPP.CompilerServices;

namespace UnityEngine {

    [Serializable] 
    public struct Quaterniond
    {
        public double w,x,y,z;
        public Quaterniond(Quaternion q) {
            this.x = q.x;
            this.y = q.y;
            this.z = q.z;
            this.w = q.w;
        }

        public Quaterniond(Vector4d v4) {
            this.x = v4.x;
            this.y = v4.y;
            this.z = v4.z;
            this.w = v4.w;
        }
        public Quaterniond(Vector4 v4) {
            this.x = v4.x;
            this.y = v4.y;
            this.z = v4.z;
            this.w = v4.w;
        }

        public Quaterniond(double x, double y, double z, double w) {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Quaterniond(float x, float y, float z, float w) {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static Quaterniond identity = new Quaterniond(0d, 0d, 0d, 1d);
        
        public double this[int index] {
            get {
                return index switch {
                    0 => x,
                    1 => y,
                    2 => z,
                    3 => w,
                    _ => throw new IndexOutOfRangeException("Invalid Quaterniond index!"),
                };
                
            }
            set {
                switch (index) {
                    case 0:
                        this.x = value;
                        break;
                    case 1:
                        this.y = value;
                        break;
                    case 2:
                        this.z = value;
                        break;
                    case 3:
                        this.w = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Quaterniond index!");
                }
            }
        }

        public Quaterniond normalized {
            get {
                double magnitude = Math.Sqrt(x*x + y*y + z*z + w*w);
                return new Quaterniond (
                    this.x / magnitude,
                    this.y / magnitude,
                    this.z / magnitude,
                    this.w / magnitude
                );
            }
        }

        public static explicit operator Quaternion(Quaterniond q) {
            return new Quaternion((float)q.x, (float)q.y, (float)q.z, (float)q.w);
        }


        // TODO: IMPLEMENT THIS
        public static double Angle(Quaterniond a, Quaterniond b) {
            return 0;
        }

        public static Quaterniond AngleAxis(double angle, Vector3d axis) {
            // Convert angle to radians
            double rad = angle * Mathf.Deg2Rad;
            axis.Normalize();

            double halfAngle = rad * 0.5d;
            double s = Mathd.Sin(halfAngle);

            return new Quaterniond(
                axis.x * s,
                axis.y * s,
                axis.z * s,
                Mathd.Cos(halfAngle)
            );
        }

        // TODO: IMPLEMENT THIS
        public static double Dot(Quaterniond a, Quaterniond b) {
            return 0;
        }

        // TODO: IMPLEMENT THIS
        public static Quaterniond Euler(double x, double y, double z) {
            return Quaterniond.identity;
        }
        
        // TODO: IMPLEMENT THIS
        public static Quaterniond Euler(Vector3d euler) {
            return Quaterniond.identity;
        }

        // TODO: IMPLEMENT THIS
        public static Quaterniond FromToRotation(Vector3d fromDirection, Vector3d toDirection) {
            return Quaterniond.identity;
        }

        // TODO: IMPLEMENT THIS
        public static Quaterniond Inverse(Quaterniond rotation) {
            return Quaterniond.identity;
        }

        // TODO: IMPLEMENT THIS
        public static Quaterniond Lerp(Quaterniond a, Quaterniond b, double t) {
            return Quaterniond.identity;
        }

        // TODO: IMPLEMENT THIS
        public static Quaterniond LerpUnclamped(Quaterniond a, Quaterniond b, double t) {
            return Quaterniond.identity;
        }

        // TODO: IMPLEMENT THIS
        public static Quaterniond LookRotation(Vector3d forwards, [DefaultValue("Vector3d.up")]Vector3d upwards) {
            return Quaterniond.identity;
        }

        // TODO: IMPLEMENT THIS
        public static Quaterniond Normalize(Quaterniond q) {
            return Quaterniond.identity;
        }

        // TODO: IMPLEMENT THIS
        public static Quaterniond RotateTowards(Quaterniond from, Quaterniond to, double maxDegreesDelta) {
            return Quaterniond.identity;
        }

        // TODO: IMPLEMENT THIS
        public static Quaterniond Slerp(Quaterniond a, Quaterniond b, double t) {
            return Quaterniond.identity;
        }

        // TODO: IMPLEMENT THIS
        public static Quaterniond SLerpUnclamped(Quaterniond a, Quaterniond b, double t) {
            return Quaterniond.identity;
        }
    }
}
