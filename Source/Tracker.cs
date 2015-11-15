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
				name = vsl.vesselName;

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

		static string texture_path = KSPUtil.ApplicationRootPath.Replace("\\", "/") + "GameData/SurveyTransponder/Textures";
		static Texture2D icon_tgt_idle;
		static Texture2D icon_tgt_targeted;


		static Rect winpos;
		static GUIStyle normal;
		static GUIStyle no_signal;
		static GUIContent tgt_idle;
		static GUIContent tgt_targeted;
		static bool styles_init;

		Dictionary<uint, TransponderInfo> transponders;

		void Awake ()
		{
			//Debug.Log ("[ST Tracker] Awake");
			transponders = new Dictionary<uint, TransponderInfo> ();
			instance = this;
		}

		void OnDestroy ()
		{
			instance = null;
		}

		void Start ()
		{
			//Debug.Log ("[ST Tracker] Start");
			enabled = false;
			StartCoroutine (ScanTransponders ());
		}

		IEnumerator<YieldInstruction> ScanTransponders ()
		{
			while (true) {
				while (transponders.Count < 1) {
					//Debug.Log (String.Format ("[ST Tracker] ScanTransponders: {0}", 0));
					yield return null;
				}
				//Debug.Log (String.Format ("[ST Tracker] ScanTransponders a: {0}", transponders.Count));
				List<uint> keys = new List<uint> (transponders.Keys);
				for (int i = 0; i < keys.Count; i++) {
					uint id = keys[i];
					double UT = Planetarium.GetUniversalTime ();
					if (transponders[id].transponder == null &&
						UT - transponders[id].lostContact > 10) {
						Debug.Log (String.Format ("[ST Tracker] Remove Transponder: {0} {1} {2} {3}", transponders[id].name, UT, transponders[id].lostContact, UT - transponders[id].lostContact));
						transponders.Remove (id);
					} else {
						transponders[id].Update ();
					}
					yield return null;
				}
				//Debug.Log (String.Format ("[ST Tracker] ScanTransponders b: {0}", transponders.Count));
				enabled = transponders.Count > 0;
			}
		}

		void InfoWindow (int windowID)
		{
			GUILayout.BeginVertical ();

			List<uint> keys = new List<uint> (transponders.Keys);
			keys.Sort ();
			for (int i = 0; i < keys.Count; i++) {
				TransponderInfo ti = transponders[keys[i]];
				bool targeted = false;

				GUIStyle style = normal;
				if (ti.transponder == null) {
					style = no_signal;
				} else {
					targeted = FlightGlobals.fetch.VesselTarget == (ti.transponder as ITargetable);
				}

				GUILayout.BeginHorizontal ();
				if (targeted) {
					if (GUILayout.Button (tgt_targeted, style)) {
						FlightGlobals.fetch.SetVesselTarget (null);
					}
				} else {
					if (GUILayout.Button (tgt_idle, style)) {
						FlightGlobals.fetch.SetVesselTarget (ti.transponder);
					}
				}
				GUILayout.FlexibleSpace ();
				GUILayout.Label (ti.name, style);
				GUILayout.FlexibleSpace ();
				GUILayout.Label (ti.situation.ToString (), style);
				GUILayout.FlexibleSpace ();
				if (ti.transponder != null) {
					GUILayout.Label (String.Format ("{0:F0}", ti.altitude), style);
					GUILayout.FlexibleSpace();
					GUILayout.Label (String.Format ("{0:F0}", ti.velocity), style);
				} else {
					GUILayout.Label ("no signal", style);
				}
				GUILayout.EndHorizontal ();
			}

			GUILayout.EndVertical ();
			GUI.DragWindow ();
		}

		void OnGUI ()
		{
			if (winpos.x == 0 && winpos.y == 0) {
				winpos.x = 10;
				winpos.y = 10;
				winpos.width = 300;
				winpos.height = 100;
			}
			if (!styles_init) {
				styles_init = true;

				icon_tgt_idle = new Texture2D(16, 16, TextureFormat.ARGB32, false);
				icon_tgt_targeted = new Texture2D(16, 16, TextureFormat.ARGB32, false);

				LoadImageFromFile (ref icon_tgt_idle, "icon_tgt_idle.png");
				LoadImageFromFile (ref icon_tgt_targeted, "icon_tgt_targeted.png");

				tgt_idle = new GUIContent (icon_tgt_idle, "Set as target");
				tgt_targeted = new GUIContent (icon_tgt_targeted, "Unset as target");

				normal = new GUIStyle (GUI.skin.box);
				normal.padding = new RectOffset (8, 8, 8, 8);
				normal.normal.textColor = Color.white;
				normal.focused.textColor = Color.white;

				no_signal = new GUIStyle (GUI.skin.box);
				no_signal = new GUIStyle (GUI.skin.box);
				no_signal.padding = new RectOffset (8, 8, 8, 8);
				no_signal.normal.textColor = Color.red;
			}
			string ver = SurveyTransponderVersionReport.GetVersion ();
			winpos = GUILayout.Window (GetInstanceID (), winpos, InfoWindow,
									   "Transponder State: " + ver,
									   GUILayout.MinWidth (200));
		}

		public void AddTransponder (ST_Transponder ponder)
		{
			uint id = ponder.part.flightID;
			//Debug.Log (String.Format ("[ST Tracker] AddTransponder: {0}", id));
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
			Debug.Log (String.Format ("[ST Tracker] RemoveTransponder: {0}", id));
			if (!transponders.ContainsKey (id)) {
				return;
			}
			double UT = Planetarium.GetUniversalTime ();
			Debug.Log (String.Format ("[ST Tracker] RemoveTransponder: {0} {1}", transponders[id].transponder, UT));
			if (transponders[id].transponder != null) {
				transponders[id].transponder = null;
				transponders[id].lostContact = UT;
			}
		}

        public static bool LoadImageFromFile(ref Texture2D tex, String filename)
        {
            bool ret = false;
			string path = texture_path + "/" + filename;

			if (System.IO.File.Exists(path)) {
				tex.LoadImage(System.IO.File.ReadAllBytes(path));
				ret = true;
			}
            return ret;
        }
	}
}
