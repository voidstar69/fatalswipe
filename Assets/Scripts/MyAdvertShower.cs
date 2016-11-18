using UnityEngine;
using UnityEngine.Advertisements;

public class MyAdvertShower : MonoBehaviour {

	void Awake()
	{
		// Default logging is ERRORs and WARNINGs only.
		Advertisement.debugLevel = Advertisement.DebugLevel.ERROR | Advertisement.DebugLevel.WARNING;

//		Advertisement.debugLevel = Advertisement.DebugLevel.ERROR | Advertisement.DebugLevel.WARNING |
//								   Advertisement.DebugLevel.INFO | Advertisement.DebugLevel.DEBUG;

		if (Advertisement.isSupported) {
			Advertisement.allowPrecache = true;
			const string appId = "18824";
			Advertisement.Initialize(appId, false); // not test-mode
		} else {
			Debug.Log("Platform not supported");
		}

//		Debug.Log("Platform is supported");
	}

	public static void ShowAdvert()
	{
		if(Advertisement.isReady())
		{
			// Show with default zone and print result to debug log
			// TODO: pausing here seems to freeze the game after the 2nd advert closes, so avoid pausing!
			Advertisement.Show(null, new ShowOptions
			{
				pause = false,
				resultCallback = result => { Debug.Log(result.ToString()); }
			});
		}
	}

	// A test of a button launching an advert
//	void OnGUI()
//	{
//		if(GUI.Button(new Rect(10, 10, 150, 50), Advertisement.isReady() ? "Show Ad" : "Waiting...")) {
//			// Show with default zone and print result to debug log
//			// TODO: pausing here seems to freeze the game after the 2nd advert closes, so avoid pausing!
//			Advertisement.Show(null, new ShowOptions {
//				pause = false,
//				resultCallback = result => {
//					Debug.Log(result.ToString());
//				}
//			});
//		}
//	}
}