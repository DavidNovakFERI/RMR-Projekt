package crc64a27fdd34899c7945;


public class MapHandler_OnMapReadyCallback
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.google.android.gms.maps.OnMapReadyCallback
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onMapReady:(Lcom/google/android/gms/maps/GoogleMap;)V:GetOnMapReady_Lcom_google_android_gms_maps_GoogleMap_Handler:Android.Gms.Maps.IOnMapReadyCallbackInvoker, Xamarin.GooglePlayServices.Maps\n" +
			"";
		mono.android.Runtime.register ("App.MapHandler+OnMapReadyCallback, App", MapHandler_OnMapReadyCallback.class, __md_methods);
	}

	public MapHandler_OnMapReadyCallback ()
	{
		super ();
		if (getClass () == MapHandler_OnMapReadyCallback.class) {
			mono.android.TypeManager.Activate ("App.MapHandler+OnMapReadyCallback, App", "", this, new java.lang.Object[] {  });
		}
	}

	public void onMapReady (com.google.android.gms.maps.GoogleMap p0)
	{
		n_onMapReady (p0);
	}

	private native void n_onMapReady (com.google.android.gms.maps.GoogleMap p0);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
