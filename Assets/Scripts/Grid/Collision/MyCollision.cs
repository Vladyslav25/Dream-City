using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyCustomCollsion
{
    public class MyCollision
    {
        public static bool SphereSphere(Vector2 _pos1, float _radius1, Vector2 _pos2, float _radius2)
        {
            if (Vector2.SqrMagnitude(_pos2 - _pos1) <= (_radius1 + _radius2) * (_radius1 + _radius2))
                return true;
            return false;
        }

        //http://www.jeffreythompson.org/collision-detection/poly-poly.php
        public static bool PolyPoly(Vector2[] p1, Vector2[] p2)
        {

            // go through each of the vertices, plus the next
            // vertex in the list
            int next = 0;
            for (int current = 0; current < p1.Length; current++)
            {

                // get next vertex in list
                // if we've hit the end, wrap around to 0
                next = current + 1;
                if (next == p1.Length) next = 0;

                // get the PVectors at our current position
                // this makes our if statement a little cleaner
                Vector2 vc = p1[current];    // c for "current"
                Vector2 vn = p1[next];       // n for "next"

                // now we can use these two points (a line) to compare
                // to the other polygon's vertices using polyLine()
                bool collision = polyLine(p2, vc.x, vc.y, vn.x, vn.y);
                if (collision) return true;

                // optional: check if the 2nd polygon is INSIDE the first
                collision = polyPoint(p1, p2[0].x, p2[0].y);
                if (collision) return true;
            }

            return false;
        }


        // POLYGON/LINE
        public static bool polyLine(Vector2[] vertices, float x1, float y1, float x2, float y2)
        {

            // go through each of the vertices, plus the next
            // vertex in the list
            int next = 0;
            for (int current = 0; current < vertices.Length; current++)
            {

                // get next vertex in list
                // if we've hit the end, wrap around to 0
                next = current + 1;
                if (next == vertices.Length) next = 0;

                // get the PVectors at our current position
                // extract X/Y coordinates from each
                float x3 = vertices[current].x;
                float y3 = vertices[current].y;
                float x4 = vertices[next].x;
                float y4 = vertices[next].y;

                // do a Line/Line comparison
                // if true, return 'true' immediately and
                // stop testing (faster)
                bool hit = lineLine(x1, y1, x2, y2, x3, y3, x4, y4);
                if (hit)
                {
                    return true;
                }
            }

            // never got a hit
            return false;
        }


        // LINE/LINE
        public static bool lineLine(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {

            // calculate the direction of the lines
            float uA = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));
            float uB = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));

            // if uA and uB are between 0-1, lines are colliding
            if (uA >= 0 && uA <= 1 && uB >= 0 && uB <= 1)
            {
                return true;
            }
            return false;
        }


        // POLYGON/POINT
        // used only to check if the second polygon is
        // INSIDE the first
        public static bool polyPoint(Vector2[] vertices, float px, float py)
        {
            bool collision = false;

            // go through each of the vertices, plus the next
            // vertex in the list
            int next = 0;
            for (int current = 0; current < vertices.Length; current++)
            {

                // get next vertex in list
                // if we've hit the end, wrap around to 0
                next = current + 1;
                if (next == vertices.Length) next = 0;

                // get the PVectors at our current position
                // this makes our if statement a little cleaner
                Vector2 vc = vertices[current];    // c for "current"
                Vector2 vn = vertices[next];       // n for "next"

                // compare position, flip 'collision' variable
                // back and forth
                if (((vc.y > py && vn.y < py) || (vc.y < py && vn.y > py)) &&
                     (px < (vn.x - vc.x) * (py - vc.y) / (vn.y - vc.y) + vc.x))
                {
                    collision = !collision;
                }
            }
            return collision;
        }
    }
}
