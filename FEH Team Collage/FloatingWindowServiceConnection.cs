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
	class FloatingWindowServiceConnection : Java.Lang.Object, IServiceConnection
	{
		private AdjustCropRectActivity _activity;

		public FloatingWindowServiceConnection(AdjustCropRectActivity activity)
		{
			_activity = activity;
		}

		public void OnServiceConnected(ComponentName name, IBinder service)
		{
			FloatingWindowServiceBinder fwServiceBinder = service as FloatingWindowServiceBinder;
			if(fwServiceBinder != null)
			{
				_activity.fwService = fwServiceBinder.GetFloatingWindowService();
				_activity.isBound = true;
			}
		}

		public void OnServiceDisconnected(ComponentName name)
		{
			_activity.isBound = false;
		}
	}
}