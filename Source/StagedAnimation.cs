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

	public class ST_StagedAnimation : PartModule, IModuleInfo, IMultipleDragCube
	{
		Animation Anim = null;

		[KSPField(isPersistant = true)]
		public bool deployed;

		[KSPField (isPersistant = false)]
		public string deployAnimationName;

		public override string GetInfo ()
		{
			return "StagedAnimation";
		}

		public string GetPrimaryField ()
		{
			return null;
		}

		public string GetModuleTitle ()
		{
			return "ST Staged Animation";
		}

		public Callback<Rect> GetDrawModulePanelCallback ()
		{
			return null;
		}

		public override void OnAwake ()
		{
		}

		void FindAnimation()
		{
			Animation[] animations = part.FindModelAnimators();
			if (animations != null) {
				Anim = animations[0];
				//Anim.wrapMode = WrapMode.Once;
				foreach (AnimationState astate in Anim) {
					Debug.Log (String.Format ("[ST FindAnimation] {0}", astate.name));
				}
			}
		}

		public override void OnStart(PartModule.StartState state)
		{
			part.stagingIcon = "PROBE";
			FindAnimation ();
			if (Anim != null) {
				if (!deployed) {
					Anim[deployAnimationName].normalizedTime = 0;
					Anim[deployAnimationName].normalizedSpeed = 0;
					SetDragState (0);
				} else {
					Anim[deployAnimationName].normalizedTime = 1;
					Anim[deployAnimationName].normalizedSpeed = 0;
					SetDragState (1);
				}
				Anim[deployAnimationName].enabled = true;
				Anim.Play (deployAnimationName);
			}
		}

		void Deploy ()
		{
			if (!deployed) {
				deployed = true;
				if (Anim != null) {
					Anim[deployAnimationName].speed = 1;
					Anim[deployAnimationName].enabled = true;
					Anim.Play (deployAnimationName);
				}
			}
		}

		void FixedUpdate ()
		{
			if (Anim != null) {
				float t;
				if (Anim.IsPlaying (deployAnimationName)) {
					t = Anim[deployAnimationName].normalizedTime;
				} else {
					t = deployed ? 1 : 0;
				}
				SetDragState (t);
			}
		}

		IEnumerator<YieldInstruction> WaitAndDeploy ()
		{
			yield return new WaitForSeconds (2.0f);
			Deploy ();
		}

		public override void OnActive ()
		{
			if (!deployed) {
				StartCoroutine (WaitAndDeploy ());
			}
		}

		void OnDestroy ()
		{
		}

		void SetDragState (float t)
		{
			part.DragCubes.SetCubeWeight ("A", t);
			part.DragCubes.SetCubeWeight ("B", 1 - t);
		}

		public string[] GetDragCubeNames()
		{
			return new string[] {"A", "B"};
		}

		public void AssumeDragCubePosition(string name)
		{
			FindAnimation ();
			if (Anim != null) {
				Anim[deployAnimationName].speed = 0f;
				Anim[deployAnimationName].enabled = true;
				Anim[deployAnimationName].weight = 1f;

				switch (name)
				{
					case "A":
						Anim[deployAnimationName].normalizedTime = 1f;
						break;
					case "B":
						Anim[deployAnimationName].normalizedTime = 0f;
						break;
				}
			}
		}

		public bool UsesProceduralDragCubes()
		{
			return false;
		}
	}
}
