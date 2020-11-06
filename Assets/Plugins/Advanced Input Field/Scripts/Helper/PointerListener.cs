//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace AdvancedInputFieldPlugin
{
	/// <summary>Class for press and release events</summary>
	public class PointerListener: MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		/// <summary>Event for presses</summary>
		[Serializable]
		private class OnPointerDownEvent: UnityEvent { };

		/// <summary>Event for releases</summary>
		[Serializable]
		private class OnPointerUpEvent: UnityEvent { };

		/// <summary>Event handler for presses</summary>
		[SerializeField]
		private OnPointerDownEvent onPointerDown;

		/// <summary>Event handler for releases</summary>
		[SerializeField]
		private OnPointerUpEvent onPointerUp;

		/// <summary>Registers press and release event callbacks</summary>
		/// <param name="pointerDownCallback">Callback for press events</param>
		/// <param name="pointerUpCallback">Callback for release events</param>
		public void RegisterCallbacks(Action pointerDownCallback, Action pointerUpCallback)
		{
			onPointerDown.AddListener(() => pointerDownCallback());
			onPointerUp.AddListener(() => pointerUpCallback());
		}

		/// <summary>Event callback for presses</summary>
		/// <param name="eventData">The event data for the press</param>
		public void OnPointerDown(PointerEventData eventData)
		{
			onPointerDown.Invoke();
		}

		/// <summary>Event callback for releases</summary>
		/// <param name="eventData">The event data for the release</param>
		public void OnPointerUp(PointerEventData eventData)
		{
			onPointerUp.Invoke();
		}
	}
}
