using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HalfEdge : IEquatable<HalfEdge> {
    public Vector2 start;
    public HalfEdge next;
    public HalfEdge pair;
    public NavTri face;
    public short type;     // I want to have this be a more memory efficient data type in the future. packed binary format?

    public HalfEdge prev() {
        return next.next;
    }

    public Vector2 nextStart() {
        return next.start;
    }

    public Vector2 prevStart() {
        return next.next.start;
    }

    public static void set(HalfEdge e,Vector2 start,HalfEdge next,HalfEdge pair,NavTri face) {
        e.start = start;
        e.next = next;
        e.pair = pair;
        e.face = face;
    }

    public override bool Equals(object other) {
        if(other is EdgePair) { return Equals(other as EdgePair); }
        if (other.GetType() != this.GetType()) { return false; }
        return Equals(other as HalfEdge);
    }

    public bool Equals(HalfEdge other) {
        return (start == other.start && next.start == other.next.start);
    }

    public bool Equals(EdgePair other) {
        return (start.Equals(other.a) && next.start.Equals(other.b)) || (start.Equals(other.b) && next.start.Equals(other.a));
    }

    public EdgePair GetEdgePair() {
        return new EdgePair(start,next.start);
    }

    public bool CompareEdgePair(EdgePair other) {
        return (start.Equals(other.a) && next.start.Equals(other.b)) || (start.Equals(other.b) && next.start.Equals(other.a));
    }

    public override int GetHashCode() {
        unchecked {
            var hashCode = 17;
            hashCode = (hashCode * 397) + start.GetHashCode();
            hashCode = (hashCode * 397) + next.start.GetHashCode();
            return hashCode;
        }
    }

    public static HalfEdge NextNullPairForwards(HalfEdge h) {
        //if (h.pair != null) {
        //    Debug.LogError("next null pair forwards pair is not null");
        //}

        if (h == null) {
            Debug.LogError("starting edge for null pair forwards is null");
        }

        if(h.next.pair == null) {
            Debug.LogError("starting edge PAIR for null pair forwards is null");
            Debug.DrawLine(h.start,h.next.start,Color.white);
            Debug.DrawLine(h.next.start,h.next.next.start,Color.black);
        }

        int counter = 0;
        do {
            
            h = h.next.pair;

            try {
                if (h.next.pair == null) {

                }
            } catch (Exception e) {
                Utils.debugStarPoint(h.start,0.1f,Color.white);
                Debug.DrawLine(h.start,h.next.start,Color.white);
                throw e;
            }
            
            if (Utils.WhileCounterIncrementAndCheck(ref counter))
                break;

        } while (h.next.pair != null);

        //try {

        //} catch (Exception e) {
        //    Utilities
        //    Debug.DrawLine(h.start,h.next.start,Color.white);
        //    throw e;
        //}
               
        return h.next;
    }

    public static HalfEdge NextNullPairBackwards(HalfEdge h) {
        //if(h.pair != null) {
        //    Debug.LogError("next null pair forwards pair is not null");
        //}

        int counter = 0;
        while (h.next.next.pair != null){
            h = h.next.next.pair;
            if (Utils.WhileCounterIncrementAndCheck(ref counter)) {

                for(int i = 0; i < 10; i++) {
                    Debug.DrawLine(h.start,h.next.next.start,Color.yellow);
                    h = h.next.next.pair;
                }

                return null;
            }                
        }

        return h.next.next;
    }
}