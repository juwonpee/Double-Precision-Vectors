using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityEngine {

    [Serializable] public struct Quaterniond
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
                    0 => w,
                    1 => x,
                    2 => y,
                    3 => z,
                    _ => throw new IndexOutOfRangeException("Invalid Quaterniond index!"),
                };
                
            }
            set {
                switch (index) {
                    case 0:
                        this.w = value;
                        break;
                    case 1:
                        this.x = value;
                        break;
                    case 2:
                        this.y = value;
                        break;
                    case 3:
                        this.z = value;
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
    }
}
