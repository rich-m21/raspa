#if !UNITY_EDITOR && UNITY_WSA
using UnityEngine;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace AdvancedInputFieldPlugin
{
	public class UWPThreadHelper: MonoBehaviour
	{
		private class SavedUnityMessage
		{
			private Component targetComponent;
			private string methodName;
			private string message;

			public SavedUnityMessage(Component targetComponent, string methodName, string message)
			{
				this.targetComponent = targetComponent;
				this.methodName = methodName;
				this.message = message;
			}

			public void Broadcast()
			{
				if(targetComponent != null)
				{
					targetComponent.BroadcastMessage(methodName, message);
				}
			}
		}

		private ThreadsafeQueue<SavedUnityMessage> messagesForUnityThread;

		public static UWPThreadHelper Instance { get; private set; }

		public static void CreateInstance()
		{
			GameObject gameObject = new GameObject("UWPThreadHelper");
			Instance = gameObject.AddComponent<UWPThreadHelper>();
		}

		public static void RunOnUWPThread(DispatchedHandler action)
		{
			CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, action);
		}

		public static void ScheduleMessageOnUnityThread(Component targetComponent, string methodName, string message)
		{
			if(Instance.messagesForUnityThread != null)
			{
				Instance.messagesForUnityThread.Enqueue(new SavedUnityMessage(targetComponent, methodName, message));
			}
		}

		private void Awake()
		{
			messagesForUnityThread = new ThreadsafeQueue<SavedUnityMessage>();
		}

		private void Update()
		{
			ExecuteMessagesOnUnityThread();
		}

		private void OnDestroy()
		{
			if(messagesForUnityThread != null)
			{
				messagesForUnityThread.Clear();
			}
		}

		private void ExecuteMessagesOnUnityThread()
		{
			if(messagesForUnityThread != null)
			{
				while(messagesForUnityThread.Count > 0)
				{
					SavedUnityMessage unityMessage = messagesForUnityThread.Dequeue();
					unityMessage.Broadcast();
				}
			}
		}
	}
}
#endif