using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoriolisContracts
{
    static class CoriolisTools
    {
        public static CelestialBody parse(this ConfigNode node, string name, CelestialBody original)
        {
            if (!node.HasValue(name))
                return original;

            CelestialBody c = original;

            string s = node.GetValue(name);

            int body;

            if (!int.TryParse(s, out body))
                return c;

            if (FlightGlobals.Bodies.Count > body)
                c = FlightGlobals.Bodies[body];
            else
            {
                //Debug.Log("Parsing value [{0}] = {1}", name, c.displayName);
                return original;
            }

            return c;
        }
    }
}
