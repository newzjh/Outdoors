//Thanks to Benblo
//https://gist.github.com/benblo/10732554

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GoShared
{
    //	public class GORoutine
    //	{

    //		public bool finished = false;
    //		public UnityWebRequest www = null;

    //		public static async GORoutine start( UniTask _routine, MonoBehaviour owner)
    //		{
    //			if (!Application.isPlaying)
    //			{
    //				GORoutine coroutine = new GORoutine(_routine);
    //				coroutine.start();
    //				return coroutine;
    //			}
    //			else
    //			{
    //				owner.StartCoroutine(_routine);
    //                return null;
    //			}
    //		}

    ////		public static IEnumerator start ( IEnumerator _routine, MonoBehaviour owner)
    ////		{
    ////			if (!Application.isPlaying) {
    ////				GORoutine coroutine = new GORoutine(_routine);
    ////				coroutine.start();
    ////				yield return coroutine;
    ////			} else {
    ////				yield return owner.StartCoroutine (_routine);
    ////			}
    ////		}


    //		public static GORoutine start(UnityWebRequest www, MonoBehaviour owner)
    //		{
    //			if (!Application.isPlaying) {
    //				GORoutine coroutine = new GORoutine(www);
    //				coroutine.start();
    //				return coroutine;
    //			} else {
    //				owner.StartCoroutine (HandleUnityWebRequest(www));
    //				return null;
    //			}
    //		}

    //		readonly UniTask routine;
    //		GORoutine( UniTask _routine )
    //		{
    //			routine = _routine;
    //		}

    //		GORoutine(UnityWebRequest www_ )
    //		{
    //			//routine = null;
    //			www = www_;
    //		}

    //		void start()
    //		{
    //			//Debug.Log("start");
    //			#if UNITY_EDITOR
    //			EditorApplication.update += update;
    //			#endif
    //		}
    //		public void stop()
    //		{
    //			#if UNITY_EDITOR
    //			EditorApplication.update -= update;
    //			#endif
    //		}

    //		void update()
    //		{
    //			/* NOTE: no need to try/catch MoveNext,
    //			 * if an IEnumerator throws its next iteration returns false.
    //			 * Also, Unity probably catches when calling EditorApplication.update.
    //			 */
    //			if (www != null) {
    //				if (www.isDone)
    //				{
    //					Debug.Log ("UnityWebRequest is finished");
    //					finished = true;
    //					stop();
    //				}
    //			} else {
    //				if (!routine.MoveNext())
    //				{
    //					finished = true;
    //					stop();
    //				}
    //			}
    //		}


    //		public UniTask WaitFor()
    //		{
    //			while(!finished)
    //			{
    //				await UniTask.NextFrame();
    //			}
    //		}

    //		static UniTask HandleUnityWebRequest(UnityWebRequest r )
    //		{
    //			yield return r;
    //		}
    //	}
}
