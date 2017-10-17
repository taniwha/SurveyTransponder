/*
This file is part of Survey Transponder.

Survey Transponder is free software: you can redistribute it and/or
modify it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Survey Transponder is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Survey Transponder.  If not, see
<http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using KSP.IO;

namespace SurveyTransponder {

	public class ST_Transponder : PartModule, IModuleInfo, ITargetable
	{
		[KSPField(isPersistant = true)]
		public bool deployed;

		[KSPField(isPersistant = true)]
		public string transponderName;

        [KSPField]
        public bool stagingIcon = true;

		public override string GetInfo ()
		{
			return "Transponder";
		}

		public string GetPrimaryField ()
		{
			return null;
		}

		public string GetModuleTitle ()
		{
			return "ST Stransponder";
		}

		public Callback<Rect> GetDrawModulePanelCallback ()
		{
			return null;
		}

		// ITargetable interface
		public Vector3 GetFwdVector ()
		{
			return transform.forward;
		}
		public string GetName ()
		{
			return vessel.vesselName;
		}
		public string GetDisplayName ()
		{
			return vessel.vesselName;
		}
		public Vector3 GetObtVelocity ()
		{
			return vessel.obt_velocity;
		}
		public Orbit GetOrbit ()
		{
			return vessel.orbit;
		}
		public OrbitDriver GetOrbitDriver ()
		{
			return vessel.orbitDriver;
		}
		public Vector3 GetSrfVelocity ()
		{
			return vessel.srf_velocity;
		}
		public Transform GetTransform ()
		{
			if (!this) {
				return null;
			}
			return transform;
		}
		public Vessel GetVessel ()
		{
			return vessel;
		}
		public VesselTargetModes GetTargetingMode ()
		{
			return VesselTargetModes.DirectionAndVelocity;
		}
		public bool GetActiveTargetable()
		{
			return false;
		}


		public void FixedUpdate ()
		{
		}

		public override void OnAwake ()
		{
		}

		public override void OnStart(PartModule.StartState state)
		{
            if (stagingIcon)
			    part.stagingIcon = "PROBE";
			if (deployed) {
				ST_Tracker.instance.AddTransponder (this);
			}
		}

		void Deploy ()
		{
			if (!deployed) {
				deployed = true;
				if (!string.IsNullOrEmpty (transponderName)) {
					vessel.vesselName = transponderName;
				}
			}
		}

		IEnumerator<YieldInstruction> WaitAndTransmit ()
		{
			yield return new WaitForSeconds (2.0f);
			Deploy ();
			yield return new WaitForSeconds (0.2f);
			FlightGlobals.fetch.SetVesselTarget (this);
			ST_Tracker.instance.AddTransponder (this);
		}

		public override void OnActive ()
		{
			if (!deployed) {
				StartCoroutine (WaitAndTransmit ());
			}
		}

		void OnDestroy ()
		{
			Disable ();
		}

		public void SetName (string name)
		{
			transponderName = name;
			if (deployed && !string.IsNullOrEmpty (transponderName)) {
				vessel.vesselName = transponderName;
			}
		}

		[KSPEvent (guiActive = true, guiActiveEditor = true,
				   guiName = "Rename Transponder", active = true,
				   externalToEVAOnly = true, guiActiveUnfocused = true,
				   unfocusedRange = 2)]
		public void ShowRenameUI ()
		{
			ST_RenameWindow.ShowGUI (this);
		}

		[KSPEvent (guiActive = true, guiActiveEditor = false,
				   guiName = "Disable Transponder", active = true,
				   externalToEVAOnly = true, guiActiveUnfocused = true,
				   unfocusedRange = 2)]
		public void Disable ()
		{
			if (FlightGlobals.fetch != null &&
				FlightGlobals.fetch.VesselTarget == (ITargetable) this) {
				FlightGlobals.fetch.SetVesselTarget (null);
			}
			if (ST_Tracker.instance != null) {
				ST_Tracker.instance.RemoveTransponder (this);
			}
			Events["Disable"].active = false;
		}
	}
}
