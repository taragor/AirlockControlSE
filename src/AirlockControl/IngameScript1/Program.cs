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
        private const string innerPostfix = " inner";
        private const string outerPostfix = " outer";
        private const string controlPostfix = "";
        private const float pressureDelta = 0.05f;


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
            Echo("0");
            NRFS._prog = this;
            NRFS._term = GridTerminalSystem; 
            Airlock i_airLock = new Airlock("ForwardHangar");
            Echo(i_airLock.PressureStates());
            i_airLock.HeartBeat();

        }

        public static class NRFS
        {
            public static IMyGridTerminalSystem _term { get; set; } = null;
            public static Program _prog { get; set; } = null;

        }

        public class Airlock
        {

            private enum State {openOuter, openInner};

            private State m_state { get; set; } = State.openInner;
            private string m_name;
            private PressureStatus m_innerPressure;
            private PressureStatus m_outerPressure;
            private PressureStatus m_airLockPressure;
            private DoorGroup m_innerDoors;
            private DoorGroup m_outerDoors;

            public Airlock(string name)
            {
                
                m_name = name;
                NRFS._prog.Echo("1");
                m_state = State.openInner;
                m_innerPressure = new PressureStatus(groupPrefix + ":" + name + innerPostfix, false);
                m_outerPressure = new PressureStatus(groupPrefix + ":" + name + outerPostfix, false);
                m_airLockPressure = new PressureStatus(groupPrefix + ":" + name + controlPostfix, true);
                m_innerDoors = new DoorGroup(groupPrefix + ":" + name + innerPostfix);
                m_outerDoors = new DoorGroup(groupPrefix + ":" + name + outerPostfix);
            }

            private void refreshDoorState()
            {
                float innerPressure = 0;
                float controlPressure = 0;
                float outerPressure = 0;
                m_innerPressure.GetOxygenLevel(ref innerPressure);
                m_airLockPressure.GetOxygenLevel(ref controlPressure);
                m_outerPressure.GetOxygenLevel(ref outerPressure);
                if (controlPressure + pressureDelta >= innerPressure && innerPressure >= controlPressure - pressureDelta)
                {
                    m_innerDoors.Open();
                }
                else
                {
                    m_innerDoors.Close();
                }
                if (controlPressure + pressureDelta >= outerPressure && outerPressure >= controlPressure - pressureDelta)
                {
                    m_outerDoors.Open();
                }
                else
                {
                    m_outerDoors.Close();
                }
            }

            public void HeartBeat()
            {
                float targetPressure = 0;
                float airLockPressure = 0;
                m_airLockPressure.GetOxygenLevel(ref airLockPressure);
                if(m_state == State.openOuter)
                {
                    m_outerPressure.GetOxygenLevel(ref targetPressure);
                }
                else
                {
                    m_innerPressure.GetOxygenLevel(ref targetPressure);
                }
                if(targetPressure >= airLockPressure)
                {
                    m_airLockPressure.RePressurize();
                }
                else
                {
                    m_airLockPressure.DePressurize();
                }
                this.refreshDoorState();
                m_innerDoors.HeartBeat();
                m_outerDoors.HeartBeat();
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

            private class DoorGroup
            {
                private List<IMyDoor> doors = new List<IMyDoor>();

                public enum DoorGroupState { open, opening, closed, closing, undef};
                public DoorGroupState m_State { get; private set; } = DoorGroupState.undef;

                public DoorGroup(string groupName)
                {
                    NRFS._term.GetBlockGroupWithName(groupName).GetBlocksOfType<IMyDoor>(doors);
                    m_State = DoorGroupState.closing;

                }

                public void Open() => m_State = DoorGroupState.opening;
                public void Close() => m_State = DoorGroupState.closing;

                public void HeartBeat()
                {
                    switch (m_State){
                        case DoorGroupState.opening:
                            Boolean open = true;
                            foreach(IMyDoor door in doors)
                            {
                                door.Enabled = true;
                                door.OpenDoor();
                                if (!(door.Status == DoorStatus.Open)) {
                                    open = false;
                                }
                            }
                            if (open)
                            {
                                m_State = DoorGroupState.open;
                            }
                            break;

                        case DoorGroupState.closing:
                            Boolean closed = true;
                            foreach(IMyDoor door in doors)
                            {
                                door.Enabled = true;
                                door.CloseDoor();
                                if(door.Status != DoorStatus.Closed)
                                {
                                    closed = false;
                                }
                            }
                            if (closed)
                            {
                                m_State = DoorGroupState.closed;
                            }
                            break;

                        case DoorGroupState.closed:
                            foreach(IMyDoor door in doors)
                            {
                                door.Enabled = false;
                            }
                            break;
                    }


                }

            }

            private class PressureStatus
            {
                public enum PressureState { dePressurizing, rePressurizing, unKnown }

                public PressureState m_state { get; private set; } = PressureState.unKnown;
                private List<IMyAirVent> m_airVents;
                private Boolean m_controlled { get; } = false;
                public PressureStatus(string groupName, Boolean controlled)
                {
                    m_airVents = new List<IMyAirVent>();
                    m_controlled = controlled;
                    List<IMyTerminalBlock> iBlocks = new List<IMyTerminalBlock>();
                    IMyBlockGroup iGroup = NRFS._term.GetBlockGroupWithName(groupName);
                    iGroup.GetBlocks(iBlocks);
                    NRFS._prog.Echo(iBlocks.Count().ToString());
                    iGroup.GetBlocksOfType<IMyAirVent>(m_airVents);
                }

                public void DePressurize()
                {
                    if (m_controlled) 
                    {
                        m_state = PressureState.dePressurizing;
                    }
                }

                public void RePressurize()
                {
                    if (m_controlled)
                    {
                        m_state = PressureState.rePressurizing;
                    }
                }

                public void HeartBeat()
                {
                    if (m_controlled)
                    {
                        switch (m_state)
                        {
                            case PressureState.dePressurizing:
                                foreach(IMyAirVent vent in m_airVents)
                                {
                                    vent.Depressurize = true;
                                }
                                break;
                            case PressureState.rePressurizing:
                                foreach(IMyAirVent vent in m_airVents)
                                {
                                    vent.Depressurize = false;
                                }
                                break;
                        }
                    }
                    else
                    {
                        Boolean pressurizing = true;
                        foreach(IMyAirVent vent in m_airVents)
                        {
                            if (vent.Depressurize)
                            {
                                pressurizing = false;
                            }
                        }
                        if (pressurizing)
                        {
                            m_state = PressureState.dePressurizing;
                        }
                        else
                        {
                            m_state = PressureState.rePressurizing;
                        }
                    }
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
