using UnityEngine;
using System;
using System.Collections;
using Cysharp.Threading.Tasks;

namespace GoShared {

	public static class GOCoroutineUtils {

		/**
	      * Usage: StartCoroutine(CoroutineUtils.Chain(...))
	      * For example:
	      *     StartCoroutine(CoroutineUtils.Chain(
	      *         CoroutineUtils.Do(() => Debug.Log("A")),
	      *         CoroutineUtils.WaitForSeconds(2),
	      *         CoroutineUtils.Do(() => Debug.Log("B"))));
	      */
		public static async UniTask Chain(MonoBehaviour host, params UniTask[] actions) {
			await UniTask.WhenAll(actions);
			//foreach (IEnumerator action in actions) {
			//	yield return host.StartCoroutine(action);
			//}
		}

		/**
	      * Usage: StartCoroutine(CoroutineUtils.DelaySeconds(action, delay))
	      * For example:
	      *     StartCoroutine(CoroutineUtils.DelaySeconds(
	      *         () => DebugUtils.Log("2 seconds past"),
	      *         2);
	      */
		public static async UniTask DelaySeconds(Action action, float delay) {
			await UniTask.WaitForSeconds(delay);
			action();
		}

		public static async UniTask WaitForSeconds(float time) {
			await UniTask.WaitForSeconds(time);
		}

		public static async UniTask Do(Action action) {
			action();
			await UniTask.DelayFrame(0);
			//yield return 0;
		}
	}
}