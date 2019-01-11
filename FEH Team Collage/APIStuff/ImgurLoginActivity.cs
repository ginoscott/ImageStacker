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
using Android.Webkit;
using Android.Util;

namespace FEH_Team_Collage.APIStuff
{
	[Activity(Label = "Imgur Login Activity")]
	class ImgurLoginActivity : Activity
	{
		private WebView mWebView;

		private static String TAG = "ImgurLoginActivity";


		protected override void OnCreate(Bundle savedInstanceState) {
			base.OnCreate(savedInstanceState);

			FrameLayout root = new FrameLayout(this);
			mWebView = new WebView(this);
			root.AddView(mWebView);
			SetContentView(root);
			
			SetupWebView();

			// so after this page below is visited, we get redirected to whatever it is I specified in my imgur app registration,
			//	which is identical to what it is I set it to in the AndroidManifest file. Since they match up, I can
			//	intercept all the token in SetupWebView (well, the ImgurWVC class) and save them somewhere. 
			mWebView.LoadUrl("https://api.imgur.com/oauth2/authorize?client_id=" + AppConstants.IMGUR_CLIENT_ID + "&response_type=token");
		}
		
		private void SetupWebView() {
			mWebView.SetWebViewClient(new ImgurWVC(this));
		}


		class ImgurWVC : WebViewClient
		{
			private Activity mActivity;

			private static Java.Util.Regex.Pattern accessTokenPattern = Java.Util.Regex.Pattern.Compile("access_token=([^&]*)");
			private static Java.Util.Regex.Pattern refreshTokenPattern = Java.Util.Regex.Pattern.Compile("refresh_token=([^&]*)");
			private static Java.Util.Regex.Pattern expiresInPattern = Java.Util.Regex.Pattern.Compile("expires_in=(\\d+)");

			public ImgurWVC(Activity activity) {
				mActivity = activity;
			}

			public override Boolean ShouldOverrideUrlLoading(WebView view, String url) {
				try { // intercept the tokens
					  // http://example.com#access_token=ACCESS_TOKEN&token_type=Bearer&expires_in=3600
					Boolean tokensURL = false;
					if (url.StartsWith(AppConstants.IMGUR_REDIRECT_URL)) {
						tokensURL = true;
						Java.Util.Regex.Matcher m;

						m = refreshTokenPattern.Matcher(url);
						m.Find();
						String refreshToken = m.Group(1);

						m = accessTokenPattern.Matcher(url);
						m.Find();
						String accessToken = m.Group(1);

						m = expiresInPattern.Matcher(url);
						m.Find();
						long expiresIn = Convert.ToInt64(m.Group(1));

						ImgurAuthorization.GetInstance().SaveRefreshToken(refreshToken, accessToken, expiresIn);

						// notify user of successful login
						mActivity.RunOnUiThread(() => {
							Toast.MakeText(MainActivity.GetAppContext(), "Logged In.", ToastLength.Short).Show();
							mActivity.SetResult(Result.Ok);
							mActivity.Finish();
						});
					}
					return tokensURL;
				}
				catch (Exception ex) {
					// if user clicks on "deny" instead of logging in, we will get an error
					Log.Error(TAG, "Error during login", ex);
					mActivity.RunOnUiThread(() => {
						Toast.MakeText(MainActivity.GetAppContext(), "Login failed.", ToastLength.Short).Show();
						mActivity.Finish();
					});
					return false;
				}
			}
		}
	}
	

}