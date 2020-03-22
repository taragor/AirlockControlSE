using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        // This file contains your actual script.
        //
        // You can either keep all your code here, or you can create separate
        // code files to make your program easier to navigate while coding.
        //
        // In order to add a new utility class, right-click on your project, 
        // select 'New' then 'Add Item...'. Now find the 'Space Engineers'
        // category under 'Visual C# Items' on the left hand side, and select
        // 'Utility Class' in the main area. Name it in the box below, and
        // press OK. This utility class will be merged in with your code when
        // deploying your final script.
        //
        // You can also simply create a new utility class manually, you don't
        // have to use the template if you don't want to. Just do so the first
        // time to see what a utility class looks like.
        // 
        // Go to:
        // https://github.com/malware-dev/MDK-SE/wiki/Quick-Introduction-to-Space-Engineers-Ingame-Scripts
        //
        // to learn more about ingame scripts.

        //CONSTANTS:

        private const string groupPrefix = "AAL";
        private const string innerPostfix = "inner";
        private const string outerPostfix = "outer";
        private const string controlPostfix = "";

        public Program()
        {
            // The constructor, called only once every session and
            // always before any other method is called. Use it to
            // initialize your script. 
            //     
            // The constructor is optional and can be removed if not
            // needed.
            // 
            // It's recommended to set Runtime.UpdateFrequency 
            // here, which will allow your script to run itself without a 
            // timer block.

            //CONSTANTS

        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // The main entry point of the script, invoked every time
            // one of the programmable block's Run actions are invoked,
            // or the script updates itself. The updateSource argument
            // describes where the update came from. Be aware that the
            // updateSource is a  bitfield  and might contain more than 
            // one update type.
            // 
            // The method itself is required, but the arguments above
            // can be removed if not needed.
            Airlock i_airLock = new Airlock(argument);
            Echo(i_airLock.PressureStates());
        }

        private class Airlock : MyGridProgram
        {
            private enum State {openOuter, openInner};

            private State m_state;
            private string m_name;
            private PressureStatus m_innerPressure;
            private PressureStatus m_outerPressure;
            private PressureStatus m_airLockPressure;

            public Airlock(string name)
            {
                m_name = name;
                m_state = State.openInner;
                m_innerPressure = new PressureStatus(groupPrefix + ":" + name + " " + innerPostfix);
                m_outerPressure = new PressureStatus(groupPrefix + ":" + name + " " + outerPostfix);
                m_airLockPressure = new PressureStatus(groupPrefix + ":" + name);
            }

            public String PressureStates()
            {
                String Outstring;
                float pressure = 0;
                if (m_innerPressure.GetOxygenLevel(ref pressure))
                {
                    Outstring = "InnerPressureLevel: OK:" + pressure.ToString();
                }
                else
                {
                    Outstring = "InnerPressureLevel: ERR:" + pressure.ToString();
                }

                if (m_airLockPressure.GetOxygenLevel(ref pressure))
                {
                    Outstring +=  "\nAirLockPressureLevel: OK:" + pressure.ToString();
                }
                else
                {
                    Outstring += "\nAirLockPressureLevel: ERR:" + pressure.ToString();
                }

                if (m_outerPressure.GetOxygenLevel(ref pressure))
                {
                    Outstring += "\nOuterPressureLevel: OK:" + pressure.ToString();
                }
                else
                {
                    Outstring += "\nOuterPressureLevel: ERR:" + pressure.ToString();
                }
                return Outstring;
            }

            private class PressureStatus : MyGridProgram
            {
                private List<IMyAirVent> m_airVents;
                public PressureStatus(string groupName)
                {
                    m_airVents = new List<IMyAirVent>();
                    GridTerminalSystem.GetBlockGroupWithName(groupName).GetBlocksOfType<IMyAirVent>(m_airVents);
                }

                public bool GetOxygenLevel(ref float pressure)
                {
                    float pressureSum = 0;
                    foreach (IMyAirVent currentVent in m_airVents)
                    {
                        if (!currentVent.CanPressurize)
                        {
                            pressure = 0;
                            return false;
                        }
                        pressureSum += currentVent.GetOxygenLevel();
                    }
                    pressure = (pressureSum / m_airVents.Count);
                    return true;
                }

            }
        }
    }
}
