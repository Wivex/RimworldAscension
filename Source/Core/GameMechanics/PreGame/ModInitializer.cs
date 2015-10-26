using System;
using UnityEngine;
using Verse;

namespace RA
{
	public class ModInitializer : ITab
	{
		public GameObject gameObject;

		public ModInitializer()
		{
			this.gameObject = new GameObject("RAController");
			this.gameObject.AddComponent<ModController>();
			UnityEngine.Object.DontDestroyOnLoad(this.gameObject);
		}

        protected override void FillTab()
		{
		}
	}
}