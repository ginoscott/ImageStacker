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
	public static class ExtensionMethods
	{
		public static void GetImgViewInfo(this ImageView imgView, Action<Dictionary<string, int>> callback)
		{
			var vto = imgView.ViewTreeObserver;
			EventHandler delg = null;
			delg = delegate
			{
				// get the width, x, and y coordinates of the imageview and
				// pass it into the callback function
				Dictionary<string, int> dict = new Dictionary<string, int>();
				dict.Add("width", imgView.Width);
				//imgView.get
				int[] pos = new int[2];
				imgView.GetLocationOnScreen(pos);
				dict.Add("x", pos[0]);
				dict.Add("y", pos[1]);
				callback.Invoke(dict);
				if(vto.IsAlive)
				{
					vto.GlobalLayout -= delg;
				}
			};
			vto.GlobalLayout += delg;
		}
	}
}