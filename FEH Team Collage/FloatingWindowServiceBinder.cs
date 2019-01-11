using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace FEH_Team_Collage
{
	public class FloatingWindowServiceBinder : Binder
	{
		private FloatingWindowService _fwService;

		public FloatingWindowServiceBinder(FloatingWindowService fwService)
		{
			_fwService = fwService;
		}

		public FloatingWindowService GetFloatingWindowService()
		{
			return _fwService;
		}
	}
}