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
using Java.Net;
using Android.Text;

namespace FEH_Team_Collage.APIStuff
{
	class ImgurAuthorization
	{
		private static ImgurAuthorization INSTANCE;
		static String SHARED_PREFERENCES_NAME = "imgur_auth";

		public static ImgurAuthorization GetInstance() {
			if (INSTANCE == null)
				INSTANCE = new ImgurAuthorization();
			return INSTANCE;
		}

		public ImgurAuthorization() {

		}

		public Boolean IsLoggedIn() {
			Context context = MainActivity.GetAppContext();
			ISharedPreferences prefs = context.GetSharedPreferences(SHARED_PREFERENCES_NAME, 0);
			return !TextUtils.IsEmpty(prefs.GetString("access_token", null));
		}

		public void AddToHttpURLConnection(HttpURLConnection conn) {
			Context context = MainActivity.GetAppContext();
			ISharedPreferences prefs = context.GetSharedPreferences(SHARED_PREFERENCES_NAME, 0);
			String accessToken = prefs.GetString("access_token", null);

			if (!TextUtils.IsEmpty(accessToken)) {
				conn.SetRequestProperty("Authorization", "Bearer " + accessToken);
			}
			else {
				conn.SetRequestProperty("Authorization", "Client-ID " + AppConstants.IMGUR_CLIENT_ID);
			}
		}

		public void SaveRefreshToken(String refreshToken, String accessToken, long expiresIn) {
			Context context = MainActivity.GetAppContext();
			context.GetSharedPreferences(SHARED_PREFERENCES_NAME, 0)
					.Edit()
					.PutString("access_token", accessToken)
					.PutString("refresh_token", refreshToken)
					.PutLong("expires_in", expiresIn)
					.Commit();
		}

		public void ResetTokens() {
			Context context = MainActivity.GetAppContext();
			context.GetSharedPreferences(SHARED_PREFERENCES_NAME, 0)
					.Edit()
					.PutString("access_token", null)
					.PutString("refresh_token", null)
					.PutLong("expires_in", 0)
					.Commit();
		}
	}
}