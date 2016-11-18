using System;
using UnityEngine;

public class Singleton : MonoBehaviour {

	public Map map;
	public Texture facebookIcon;

	private string lastResponse = "No response yet!";

	public void OnGUI()
	{
		// Do not allow any Facebook errors to crash app
		try
		{
			if(GUI.Button(new Rect(Screen.width - 80, 0, 80, 80), facebookIcon))
			{
				FacebookInitAndPost();
			}
		}
		catch(Exception ex)
		{
			Debug.LogException(ex);
		}

		// Show score
		GUI.Label(new Rect(0, 0,  Screen.width, 50), "Score: " + map.Score.ToString());

		// Debugging
//		GUI.Label(new Rect(0, Screen.height - 50,  Screen.width, 50), lastResponse);
	}

	public void FacebookInitAndPost()
	{
		InitFacebook(() => {
			LoginUser(() => PostOnFacebook());
		});
	}
	
	private void InitFacebook(Action postInit)
	{
		FB.Init(() => {
			Debug.Log("FB.Init completed: Is user logged in? " + FB.IsLoggedIn);
			FB.ActivateApp();
			postInit();
		});
	}

	private Action postLoginCallback;

	private void LoginUser(Action postLogin)
	{
		postLoginCallback = postLogin;

		if(FB.IsLoggedIn)
		{
			postLoginCallback();
		}
		else
		{
			FB.Login(scope: "",
			         callback: LoginCallback);
//			FB.Login("email,publish_actions", LoginCallback);
		}
	}

	private void LoginCallback(FBResult result)
	{
		if (result.Error != null)
			lastResponse = "Error Response:\n" + result.Error;
		else if (!FB.IsLoggedIn)
		{
			lastResponse = "Login cancelled by Player";
		}
		else
		{
			lastResponse = "Login was successful!";
			postLoginCallback();
		}
	}

	private void PostOnFacebook()
	{
		lastResponse = "PostOnFacebook called";

		FB.Feed(
			link: "https://play.google.com/store/apps/details?id=com.HalfBit.ExperimentalGame1",	// Google Play Store
//			link: "http://www.amazon.com/gp/mas/dl/android?p=com.HalfBit.ExperimentalGame1",		// Amazon App Store
			linkName: "Fatal Swipe game for Android",
			linkCaption: "Hard puzzle game with retro look",
			linkDescription: "Can you solve these difficult puzzles?",
			callback: PostCallback
			);
	}

	private void PostCallback(FBResult result)
	{
		Debug.Log("Singleton.PostCallback called");
		lastResponse = "PostCallback called";

		// Some platforms return the empty string instead of null.
		if (!String.IsNullOrEmpty (result.Error))
		{
			lastResponse = "Error Response:\n" + result.Error;
		}
		else if (!String.IsNullOrEmpty (result.Text))
		{
			lastResponse = "Success Response:\n" + result.Text;
		}
		else if (result.Texture != null)
		{
			lastResponse = "Success Response: texture\n";
		}
		else
		{
			lastResponse = "Empty Response\n";
		}

		Debug.Log(lastResponse);
	}
}