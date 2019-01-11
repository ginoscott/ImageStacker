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
	class GridItemViewHolder : Java.Lang.Object
	{
		public ImageView Image { get; set; }
		public TextView Desc { get; set; }
	}
}