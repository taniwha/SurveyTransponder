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
using FinePrint;

namespace SurveyTransponder {

	[KSPAddon (KSPAddon.Startup.SpaceCentre, true)]
	public class ST_AllowWater : MonoBehaviour
	{
		public static ST_AllowWater instance
		{
			get;
			private set;
		}

		void Awake ()
		{
			instance = this;
			DontDestroyOnLoad(this);
		}

		void OnDestroy ()
		{
			instance = null;
		}

		void Start ()
		{
			StartCoroutine (WaitAndSetAllowWater ());
		}

		IEnumerator<YieldInstruction> WaitAndSetAllowWater ()
		{
			yield return null;
			if (ContractDefs.config == null) {
				// probably not in career mode
				yield break;
			}
			var survey = ContractDefs.config.GetNode ("Survey");
			if (survey == null) {
				yield break;
			}
			foreach (var survey_def in survey.GetNodes ("SURVEY_DEFINITION")) {
				foreach (var param in survey_def.GetNodes ("PARAM")) {
					if (param.GetValue ("Experiment") == "seismicScan") {
						// seismic scans cannot be done when splashed.
						continue;
					}
					// nor can atmosphric analysis scans, but the can't be done
					// on the ground, either.
					if (param.HasValue ("AllowGround")
					    && param.GetValue ("AllowGround") == "True"
						&& param.HasValue ("AllowWater")) {
						Debug.Log (String.Format("[ST AW] forcing {0}.{1}.AllowWater",
												 survey_def.GetValue ("Title"),
												 param.GetValue ("Experiment")));
						param.SetValue ("AllowWater", "True");
					}
				}
			}
		}
	}
}
