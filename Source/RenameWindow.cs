/*
This file is part of Extraplanetary Launchpads.

Extraplanetary Launchpads is free software: you can redistribute it and/or
modify it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Extraplanetary Launchpads is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Extraplanetary Launchpads.  If not, see
<http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using KSP.IO;

namespace SurveyTransponder {

	[KSPAddon (KSPAddon.Startup.EveryScene, false)]
	public class ST_RenameWindow: MonoBehaviour
	{
		private static ST_Transponder xpondInstance = null;
		private static ST_RenameWindow windowInstance = null;
		private static Rect windowpos = new Rect(Screen.width * 0.35f,Screen.height * 0.1f,1,1);
		private static string newName;

		void Awake ()
		{
			if (!HighLogic.LoadedSceneIsEditor
				&& !HighLogic.LoadedSceneIsFlight) {
				Destroy (this);
				return;
			}
			windowInstance = this;
			enabled = false;
		}

		public static void HideGUI ()
		{
			if (windowInstance != null) {
				windowInstance.enabled = false;
				InputLockManager.RemoveControlLock ("ST_Rename_window_lock");
			}
		}

		public static void ShowGUI (ST_Transponder xpond)
		{
			xpondInstance = xpond;
			newName = xpond.transponderName;
			if (windowInstance != null) {
				windowInstance.enabled = true;
			}
		}

		void WindowGUI (int windowID)
		{
			GUILayout.BeginVertical ();

			GUILayout.BeginHorizontal ();
			newName = GUILayout.TextField (newName);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace ();
			if (GUILayout.Button ("OK")) {
				xpondInstance.SetName (newName);
				HideGUI ();
			}
			GUILayout.FlexibleSpace ();
			if (GUILayout.Button ("Cancel")) {
				HideGUI ();
			}
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();

			GUILayout.EndVertical ();

			GUI.DragWindow (new Rect (0, 0, 10000, 20));

		}

		void OnGUI ()
		{
			GUI.skin = HighLogic.Skin;
			windowpos = GUILayout.Window (GetInstanceID (), windowpos,
										  WindowGUI, "Rename Transponder",
										  GUILayout.Width(500));
			if (windowpos.Contains (new Vector2 (Input.mousePosition.x, Screen.height - Input.mousePosition.y))) {
				InputLockManager.SetControlLock ("ST_Rename_window_lock");
			} else {
				InputLockManager.RemoveControlLock ("ST_Rename_window_lock");
			}
		}
	}
}
