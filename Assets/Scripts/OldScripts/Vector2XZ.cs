using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// dont actually use this
// you can if you want
// but eehhhhh
public struct Vector2XZ : IEquatable<Vector2XZ>
{
    public static Vector2XZ Zero { get { return new Vector2XZ(0,0); } }
    public static Vector2XZ Infinity { get { return new Vector2XZ(Mathf.Infinity,Mathf.Infinity); } }

    public float x;
    public float z;

    public Vector2XZ(float x,float z) {
        this.x = x;
        this.z = z;
    }

    public static Vector2XZ operator +(Vector2XZ a) => a;
    public static Vector2XZ operator -(Vector2XZ a) => new Vector2XZ(-a.x,-a.z);

    public static Vector2XZ operator +(Vector2XZ a,Vector2XZ b) => new Vector2XZ(a.x + b.x,a.z + b.z);
    public static Vector2XZ operator -(Vector2XZ a,Vector2XZ b) => new Vector2XZ(a.x - b.x,a.z - b.z);

    public static Vector2XZ operator /(Vector2XZ a,float f) => new Vector2XZ(a.x / f,a.z / f);
    public static Vector2XZ operator *(Vector2XZ a,float f) => new Vector2XZ(a.x * f,a.z * f);
    public static Vector2XZ operator *(float f,Vector2XZ a) => new Vector2XZ(a.x * f,a.z * f);

    public static bool operator ==(Vector2XZ a,Vector2XZ b) => (a.x == b.x) && (a.z == b.z);
    public static bool operator !=(Vector2XZ a,Vector2XZ b) => (a.x != b.x) || (a.z != b.z);

    public static implicit operator Vector3(Vector2XZ v) => new Vector3(v.x,0,v.z);

    public bool Equals(Vector2XZ other) {
        return other == this;
    }

    public override int GetHashCode() {
        var hashCode = 1553271884;
        hashCode = hashCode * -1521134295 + x.GetHashCode();
        hashCode = hashCode * -1521134295 + z.GetHashCode();
        return hashCode;
    }

    public override bool Equals(object obj) {
        if (this.GetType() != obj.GetType()) {
            return false;
        } else {
            return Equals((Vector2XZ)obj);
        }
    }

    public override string ToString() {
        return x.ToString() + " , " + z.ToString();
    }
}