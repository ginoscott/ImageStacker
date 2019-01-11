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
using Android.Graphics;

namespace FEH_Team_Collage
{
	[Service]
	public class FloatingWindowService : Service
	{
		private IWindowManager _windowManager;
		private WindowManagerLayoutParams _layoutParams;
		private View _floatingView;
		private FloatingWindowServiceBinder _fwBinder;


		[return: GeneratedEnum]
		public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
		{


			return base.OnStartCommand(intent, flags, startId);
		}

		public override void OnCreate()
		{
			base.OnCreate();

			// inflate the layout
			_floatingView = LayoutInflater.From(this).Inflate(Resource.Layout.floatingWindow_Layout, null);

			// set the params
			_layoutParams = new WindowManagerLayoutParams(
				0,
				0,
				WindowManagerTypes.SystemOverlay,
				WindowManagerFlags.NotFocusable,
				Android.Graphics.Format.Translucent)
			{
				Gravity = GravityFlags.Top | GravityFlags.Left,
				X = 0,
				Y = 0
			};

			// add the view
			_windowManager = GetSystemService(WindowService).JavaCast<IWindowManager>();
			_windowManager.AddView(_floatingView, _layoutParams);
		}

		// call this to update the position/size of the layout onscreen
		public void UpdatePosition(int x, int y, int height, int width)
		{
			_layoutParams.X = x;
			_layoutParams.Y = y;
			_layoutParams.Width = width;
			_layoutParams.Height = height;
			_windowManager.UpdateViewLayout(_floatingView, _layoutParams);
		}


		public override IBinder OnBind(Intent intent)
		{
			_fwBinder = new FloatingWindowServiceBinder(this);
			return _fwBinder;
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			
			if(_floatingView != null)
			{
				_windowManager.RemoveView(_floatingView);
			}
		}
	}
}