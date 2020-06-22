using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class EventManager : MonoBehaviour {
	
	private Dictionary <EventEnum, UnityEvent> eventDictionary;
	
	private static EventManager eventManager;
	
	public static EventManager Instance
	{
		get
		{
			if (!eventManager)
			{
				eventManager = FindObjectOfType (typeof (EventManager)) as EventManager;
				
				if (!eventManager)
				{
					Debug.LogError ("There needs to be one active EventManger script on a GameObject in your scene.");
				}
				else
				{
					eventManager.Init (); 
				}
			}
			
			return eventManager;
		}
	}
	
	void Init ()
	{
		if (eventDictionary == null)
		{
			eventDictionary = new Dictionary<EventEnum, UnityEvent>();
		}
	}
	
	public static void StartListening (EventEnum eventName, UnityAction listener)
	{
		UnityEvent thisEvent = null;
		if (Instance.eventDictionary.TryGetValue (eventName, out thisEvent))
		{
			thisEvent.AddListener (listener);
		} 
		else
		{
			thisEvent = new UnityEvent ();
			thisEvent.AddListener (listener);
			Instance.eventDictionary.Add (eventName, thisEvent);
		}
	}
	
	public static void StopListening (EventEnum eventName, UnityAction listener)
	{
		if (eventManager == null) return;
		UnityEvent thisEvent = null;
		if (Instance.eventDictionary.TryGetValue (eventName, out thisEvent))
		{
			thisEvent.RemoveListener (listener);
		}
	}
	
	public static void TriggerEvent (EventEnum eventName)
	{
		Debug.Log("EventManager.TriggerEvent(" + eventName.ToString());
		UnityEvent thisEvent = null;
		if (Instance.eventDictionary.TryGetValue (eventName, out thisEvent))
		{
			thisEvent.Invoke ();
		}
	}
}

public enum EventEnum
{
	CursorDefault
	,CursorSelect
	,CursorAttack
	,CursorGrab
	,CursorMove

	,StartShopping
	,StartPlacingBTObject
	,CancelPlacingBTObject
	,PlacedBTObject
	,PlayerReadyForMatch
	,MatchCountdownStarted
	,MatchStarted

}