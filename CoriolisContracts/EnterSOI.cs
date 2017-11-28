using System;
using System.Collections.Generic;
using System.Linq;
using Contracts;
using UnityEngine;
using Contracts.Parameters;
using Contracts.Agents;
using FinePrint.Utilities;

namespace CoriolisContracts
{
    public class EnterSOI : Contract
    {
        private CelestialBody body;
        private System.Random rand = new System.Random();

        protected override bool Generate()
        {
            EnterSOI[] SOIContracts = ContractSystem.Instance.GetCurrentContracts<EnterSOI>();
            int offers = 0;
            int active = 0;
            int MaxOffers = 1;
            int MaxActive = 3;

            for (int i = 0; i < SOIContracts.Length; i++)
            {
                EnterSOI m = SOIContracts[i];
                if (m.ContractState == State.Offered)
                {
                    offers++;
                }
                else if (m.ContractState == State.Active)
                {
                    active++;
                }
            }

            if (offers >= MaxOffers)
                return false;

            else if (active >= MaxActive)
                return false;

            List<CelestialBody> bodies = new List<CelestialBody>();
            Func<CelestialBody, bool> cb = null;

            switch(prestige)
            {
                case ContractPrestige.Trivial:
                    cb = delegate (CelestialBody b)
                    {
                        if (b == Planetarium.fetch.Sun)
                        {
                            return false;
                        }
                        if (b.scienceValues.RecoveryValue > 4)
                        {
                            return false;
                        }
                        return true;
                    };
                    bodies.AddRange(ProgressUtilities.GetBodiesProgress(ProgressType.ORBIT, true, cb));
                    bodies.AddRange(ProgressUtilities.GetNextUnreached(2, cb));
                    break;

                case ContractPrestige.Significant:
                    cb = delegate (CelestialBody b)
                    {
                        if (b == Planetarium.fetch.Sun)
                        {
                            return false;
                        }
                        if (b == Planetarium.fetch.Home)
                        {
                            return false;
                        }
                        if (b.scienceValues.RecoveryValue > 8)
                        {
                            return false;
                        }
                        return true;
                    };
                    bodies.AddRange(ProgressUtilities.GetBodiesProgress(ProgressType.FLYBY, true, cb));
                    bodies.AddRange(ProgressUtilities.GetNextUnreached(2, cb));
                    break;

                case ContractPrestige.Exceptional:
                    cb = delegate (CelestialBody b)
                    {
                        if (b == Planetarium.fetch.Home)
                        {
                            return false;
                        }

                        if (Planetarium.fetch.Home.orbitingBodies.Count > 0)
                        {
                            foreach (CelestialBody B in Planetarium.fetch.Home.orbitingBodies)
                            {
                                if (b == B)
                                {
                                    return false;
                                }
                            }
                        }

                        if (b.scienceValues.RecoveryValue < 4)
                        {
                            return false;
                        }
                        return true;
                    };
                    bodies.AddRange(ProgressUtilities.GetBodiesProgress(ProgressType.FLYBY, true, cb));
                    bodies.AddRange(ProgressUtilities.GetNextUnreached(4, cb));
                    break;
            }

            if (bodies.Count <= 0)
            {
                return false;
            }

            body = bodies[rand.Next(0, bodies.Count)];

            if (body == null)
            {
                return false;
            }

            this.AddParameter(new Contracts.Parameters.EnterSOI());

            if (this.ParameterCount == 0)
            {
                return false;
            }

            float primaryModifier = ((float)rand.Next(80, 121) / 100f);
            float diffModifier = 1 + ((float)this.Prestige * 0.5f);

            float Mod = primaryModifier * diffModifier;

            this.agent = AgentList.Instance.GetAgent("Kerbin World - Firsts Record - Keeping Society");
            

            base.SetExpiry(1, 100);
            base.SetScience(20F);
            base.SetDeadlineYears(100F);
            base.SetReputation(5F, -2F);
            base.SetFunds(30000F, 50000F, -3000F);

            return true;
        }

        public override bool CanBeCancelled()
        {
            return true;
        }

        public override bool CanBeDeclined()
        {
            return true;
        }

        protected override string GetHashString()
        {
            return string.Format("{0}{1}",body.bodyName, (int)this.prestige);
        }

        protected override string GetTitle()
        {
            return string.Format("Enter the SOI of {0}", body.bodyName);
        }

        protected override string GetDescription()
        {
            if (body == null)
            {
                return "Something went wrong here!";
            }
            string story = "We at the {0} have decided that you need to extend the reach of your space program. It was deided that the best way to do this was to explore new places in the universe. But first you have to prove you can get there. This is why we're here.";
            return string.Format(story, this.agent.Name, body.bodyName);
        }

        protected override string GetSynopsys()
        {
            if (body == null)
            {
                return "Someting went wrong here!";
            }

            return string.Format("Enter the SOI of {0} by encountering it.", body.bodyName);
        }

        protected override string MessageCompleted()
        {
            if (body == null)
            {
                return "Someting went wrong here!";
            }

            return string.Format("You entered the SOI of {0},have some Snacks!", body.bodyName);
        }

        protected override void OnLoad(ConfigNode node)
        {
            body = node.parse("Target_SOI", body);
            if (body == null)
            {
                Debug.LogError("Error while loading Enter the SOI contract target body; removing contract now.");
                this.Unregister();
                ContractSystem.Instance.Contracts.Remove(this);
                return;
            }
            if (this.ParameterCount == 0)
            {
                Debug.LogWarning("No Parameters Loaded For 'Enter the SOI' Contract; Removing Now.");
                this.Unregister();
                ContractSystem.Instance.Contracts.Remove(this);
                return;
            }
        }

        protected override void OnSave(ConfigNode node)
        {
            if (body == null)
            {
                return;
            }

            node.AddValue("Target_SOI", body.flightGlobalsIndex);
        }

        public override bool MeetRequirements()
        {
            return ProgressTracking.Instance.NodeComplete(new string[] { Planetarium.fetch.Home.bodyName, "Escape" });
        }

        public static CelestialBody TargetBody(Contract c)
        {
            if (c == null || c.GetType() != typeof(EnterSOI))
            {
                return null;
            }

            try
            {
                EnterSOI Instance = (EnterSOI)c;
                return Instance.body;
            }
            catch(Exception e)
            {
                Debug.LogError("Errorwhile accessing EnterSoi contract Target Body\n" + e);
                return null;
            }
        }

        public CelestialBody Body
        {
            get { return body; }
        }
    }
}
