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

	[KSPAddon (KSPAddon.Startup.Flight, false)]
	public class ST_Tracker : MonoBehaviour
	{
		class TransponderInfo
		{
			public ST_Transponder transponder;
			public string name;
			public double lostContact;
			public double altitude;
			public double velocity;
			public Vessel.Situations situation;

			public TransponderInfo (ST_Transponder ponder)
			{
				transponder = ponder;
				name = ponder.vessel.vesselName;
				altitude = ponder.vessel.altitude;
				velocity = 0;	// updated later
			}

			public void Update ()
			{
				if (transponder == null) {
					return;
				}
				Vessel vsl = transponder.vessel;

				altitude = vsl.altitude;
				situation = vsl.situation;

				// Velocity/CoM stuff taken from MechJeb2
				Vector3d CoM = Vector3d.zero;
				Vector3d obtVel = Vector3d.zero;
				double mass = 0;

				for (int i = 0; i < vsl.parts.Count; i++) {
					Part p = vsl.parts[i];
					if (p.rb != null) {
						mass += p.rb.mass;
						CoM = CoM + (p.rb.worldCenterOfMass * p.rb.mass);
						obtVel += p.rb.velocity * p.rb.mass;
					}
				}
				CoM /= mass;
				if (vsl.packed) {
					obtVel = vsl.obt_velocity;
				} else {
					// XXX This could be a problem for non-active vessels...
					obtVel = obtVel / mass + Krakensbane.GetFrameVelocity () + vsl.orbit.GetRotFrameVel(vsl.orbit.referenceBody).xzy;
				}

				velocity = (obtVel - vsl.mainBody.getRFrmVel (CoM)).magnitude;
			}
		}

		public static ST_Tracker instance
		{
			get;
			private set;
		}

		Dictionary<uint, TransponderInfo> transponders;

		void Awake ()
		{
			transponders = new Dictionary<uint, TransponderInfo> ();
			instance = this;
			enabled = false;
		}

		void OnDestroy ()
		{
			instance = null;
		}

		void Start ()
		{
			StartCoroutine (ScanTransponders ());
		}

		IEnumerator<YieldInstruction> ScanTransponders ()
		{
			while (true) {
				while (transponders.Count < 1) {
					yield return null;
				}
				List<uint> keys = new List<uint> (transponders.Keys);
				for (int i = 0; i < keys.Count; i++) {
					uint id = keys[i];
					double UT = Planetarium.GetUniversalTime ();
					if (transponders[id].transponder == null &&
						UT - transponders[id].lostContact > 10) {
						transponders.Remove (id);
					} else {
						transponders[id].Update ();
					}
					yield return null;
				}
				enabled = transponders.Count > 0;
			}
		}

		void Update ()
		{
			if (transponders.Count < 1) {
				enabled = false;
				return;
			}
			enabled = true;
		}

		public void AddTransponder (ST_Transponder ponder)
		{
			uint id = ponder.part.flightID;
			if (transponders.ContainsKey (id)) {
				// may have temporarlily lost contact
				transponders[id].transponder = ponder;
				return;
			}
			transponders[id] = new TransponderInfo (ponder);
		}

		public void RemoveTransponder (ST_Transponder ponder)
		{
			uint id = ponder.part.flightID;
			if (!transponders.ContainsKey (id)) {
				return;
			}
			double UT = Planetarium.GetUniversalTime ();
			if (transponders[id].transponder != null) {
				transponders[id].transponder = null;
				transponders[id].lostContact = UT;
			}
		}
	}
}
